using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Enigma.D3.MapHack.Markers;

namespace Enigma.D3.MapHack
{
    public partial class Shell : Window, INotifyPropertyChanged
    {
        private static readonly Lazy<Shell> _lazyInstance = new Lazy<Shell>(() => new Shell());

        public static Shell Instance { get { return _lazyInstance.Value; } }

        private bool _isAttached;
        private ShellTraceListener _tracer;

        public MapMarkerOptions Options { get; private set; }
        public bool IsAttached { get { return _isAttached; } set { if (_isAttached != value) { _isAttached = value; Refresh("IsAttached"); } } }
        public string Version => new MemoryModel.SymbolTable(MemoryModel.Platform.X64).Version.ToString();

        public Shell()
        {
            Options = MapMarkerOptions.Instance;
            DataContext = this;
            InitializeComponent();
            Trace.Listeners.Add(_tracer = new ShellTraceListener(Dispatcher, Log));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void Refresh(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private class ShellTraceListener : TraceListener
        {
            private readonly Dispatcher _dispatcher;
            private readonly TextBox _log;

            public ShellTraceListener(Dispatcher dispatcher, TextBox log)
            {
                _dispatcher = dispatcher;
                _log = log;
            }

            public override void Write(string message) => WriteLine(message);

            public override void WriteLine(string message) => _dispatcher.BeginInvoke((Action)(() =>
            {
                WriteLineSync(message);
            }));

            internal void WriteLineSync(string message)
            {
                if (_log.Tag == null)
                    return;

                var scroll = _log.GetLastVisibleLineIndex() <= _log.LineCount - 1;
                _log.AppendText(DateTime.Now.ToString("HH:mm:ss.ffffff") + ": " + message + Environment.NewLine);
                if (scroll)
                    _log.ScrollToEnd();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button.Content.Equals("Start"))
            {
                Log.Tag = new object();
                Log.Foreground = Brushes.Black;
                button.Content = "Stop";
                Trace.WriteLine("Logging started.");
            }
            else
            {
                _tracer.WriteLineSync("Logging stopped.");
                Log.Tag = null;
                Log.Foreground = Brushes.Gray;
                button.Content = "Start";
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Log.Clear();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start((sender as Hyperlink).NavigateUri.AbsoluteUri);
        }

        private Random _rand = new Random();
        private DispatcherTimer _timer;

        private void Animate(object sender, EventArgs e)
        {
            var paths = LogoPathGrid.Children.Cast<Path>().ToArray();
            foreach (var path in paths)
            {
                var transform = (RotateTransform)((path.RenderTransform as RotateTransform) ?? (path.RenderTransform = new RotateTransform(0, 75, 75)));
                transform.Angle += (_rand.NextDouble() - 0.5);
            }
        }

        private void TabItem_Selected(object sender, RoutedEventArgs e)
        {
            _timer?.Start();
        }

        private void TabItem_Unselected(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
        }

        private int _i;
        private void LogoPathGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_i == 0)
                    LogoPathGrid.ToolTip += " Stop!!!";
                if (_i++ == 5)
                {
                    _timer = _timer ?? new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Render, Animate, Dispatcher.CurrentDispatcher);
                    _timer.Start();
                    LogoPathGrid.ToolTip = "You broke it :(";
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var ctx = MemoryModel.MemoryContext.Current;
            var symbols = MemoryModel.SymbolTable.Current;
            if (ctx == null || symbols == null)
            {
                DebugInfo.Text = "No process attached. Unable to generate debug info.";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[ctx]");
            sb.AppendLine($"{nameof(ctx.MainModuleVersion)} = {ctx.MainModuleVersion}");
            sb.AppendLine($"{nameof(ctx.ImageBase)} = {ctx.ImageBase}");
            sb.AppendLine();
            sb.AppendLine("[symbols]");
            sb.AppendLine($"{nameof(symbols.Version)} = {symbols.Version}");
            foreach (var f in symbols.CryptoKeys.GetType().GetFields())
                sb.AppendLine($"{nameof(symbols.CryptoKeys)}.{f.Name} = {f.GetValue(symbols.CryptoKeys):X}");
            sb.AppendLine();
            sb.AppendLine("[pd]");
            var pd = ctx.DataSegment.ObjectManager.PlayerDataManager.First(x => x.HeroClass == Enums.HeroClass.None);
            sb.AppendLine($"{nameof(pd.ACDID)} = {pd.ACDID:X}");
            sb.AppendLine($"{nameof(pd.ActorID)} = {pd.ActorID:X}");
            sb.AppendLine($"{nameof(pd.LevelAreaSNO)} = {pd.LevelAreaSNO:X}");
            sb.AppendLine();
            sb.AppendLine("[asm]");
            var dump = ctx.Memory.Reader.ReadBytes(ctx.ImageBase + 0xAD984, 0x400);
            sb.AppendLine(Convert.ToBase64String(dump));
            DebugInfo.Text = sb.ToString();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(DebugInfo.Text);
        }
    }
}
