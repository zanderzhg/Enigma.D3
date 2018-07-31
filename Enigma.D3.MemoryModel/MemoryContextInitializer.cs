using Enigma.D3.MemoryModel.SymbolPatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel
{
    public class MemoryContextInitializer
    {
        /// <summary>
        /// A default initializer that logs to <see cref="Trace"/> and does not handle multi-boxing (uses first process).
        /// </summary>
        public static MemoryContextInitializer Default { get; } = new MemoryContextInitializer { Log = (message) => Trace.WriteLine(message) };

        /// <summary>
        /// Function invoked when multiple processes are found during invokation.
        /// Returns the desired process to create context for (null continues process scan).
        /// Setting this to null results in first found process to be used.
        /// </summary>
        public Func<Process[], Process> MultiProcessHandler { get; set; }

        /// <summary>
        /// Action invoked for log messages. Setting this to null disables logging.
        /// </summary>
        public Action<string> Log { get; set; }

        /// <summary>
        /// Waits for a process and returns a <see cref="MemoryContext"/> for it after patching <see cref="SymbolTable.Current"/>.
        /// </summary>
        public MemoryContext Invoke()
        {
            var ctx = default(MemoryContext);
            while (ctx == null)
            {
                var processes = Process.GetProcessesByName("Diablo III64");
                if (processes.Any())
                {
                    var process = default(Process);
                    if (processes.Length == 1 || MultiProcessHandler == null)
                    {
                        process = processes[0];
                    }
                    else
                    {
                        process = MultiProcessHandler.Invoke(processes);
                    }

                    if (process != null)
                    {
                        ctx = MemoryContext.FromProcess(process);
                        break;
                    }
                }
                else
                {
                    Log?.Invoke("Could not find any process.");
                }
                Thread.Sleep(1000);
            }
            Log?.Invoke("Found a process.");

            while (true)
            {
                try
                {
                    SymbolPatcher64.UpdateSymbolTable(ctx);
                    Log?.Invoke("Symbol table updated.");
                    return ctx;
                }
                catch (Exception exception)
                {
                    Log?.Invoke($"Could not update symbol table, optimized for patch {SymbolPatcher64.VerifiedBuild}, running {ctx.MainModuleVersion.Revision}: " + exception.Message);
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
