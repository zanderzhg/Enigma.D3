using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;
using Enigma.Wpf;
using Enigma.D3.MapHack;
using Enigma.D3.MemoryModel;
using System.Diagnostics;
using Enigma.Memory;
using Enigma.D3.MemoryModel.SymbolPatching;
using Enigma.D3.MapHack.Markers;

namespace Enigma.D3.Bootloader
{
    internal class App : Application
    {
        [STAThread]
        public static void Main() => new App().Run();

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!e.Args.Any(x => x.ToLowerInvariant() == "--no-eula-prompt"))
            {
                if (MessageBox.Show("The use of this software breaks Blizzard EULA and may result in your account getting banned. This software may also cause the game client to crash.\r\n\r\n" +
                    "Continue at your own risk.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
                {
                    Shutdown();
                    return;
                }
            }

            MapMarkerOptions.Instance.Load();

            MainWindow = Shell.Instance;
            MainWindow.Show();
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    using (var ctx = CreateMemoryContext())
                    using (var watcher = new WatcherThread(ctx))
                    {
                        Trace.WriteLine("Attached to process.");

                        Shell.Instance.IsAttached = true;
                        Minimap minimap = null;
                        OverlayWindow overlay = null;
                        Execute.OnUIThread(() =>
                        {
                            Canvas canvas = new Canvas();
                            overlay = OverlayWindow.Create((ctx.Memory.Reader as ProcessMemoryReader).Process, canvas);
                            overlay.Show();
                            minimap = new Minimap(canvas);
                        });
                        watcher.AddTask(minimap.Update);
                        watcher.Start();
                        (ctx.Memory.Reader as ProcessMemoryReader).Process.WaitForExit();
                        Execute.OnUIThread(() => overlay.Close());
                    }
                    Shell.Instance.IsAttached = false;
                }
            }, TaskCreationOptions.LongRunning);
        }

        private MemoryContext CreateMemoryContext()
        {
            var ctx = default(MemoryContext);
            while (ctx == null)
            {
                var processes = Process.GetProcessesByName("Diablo III64");
                if (processes.Any())
                {
                    var process = default(Process);
                    if (processes.Length == 1)
                    {
                        process = processes[0];
                    }
                    else
                    {
                        Execute.OnUIThread(() =>
                        {
                            var selector = new MultiProcessSelector(processes);
                            selector.Owner = Shell.Instance;
                            selector.ShowDialog();
                            process = selector.SelectedProcess;
                        });
                    }

                    if (process != null)
                    {
                        ctx = MemoryContext.FromProcess(process);
                        break;
                    }
                }
                else
                {
                    Trace.WriteLine("Could not find any process.");
                }
                Thread.Sleep(1000);
            }
            Trace.WriteLine("Found a process.");

            while (true)
            {
                try
                {
                    SymbolPatcher64.UpdateSymbolTable(ctx);
                    Trace.WriteLine("Symbol table updated.");
                    return ctx;
                }
                catch (Exception exception)
                {
                    Trace.WriteLine($"Could not update symbol table, optimized for patch {SymbolPatcher64.VerifiedBuild}, running {ctx.MainModuleVersion.Revision}: " + exception.Message);
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
