using Enigma.D3.MemoryModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enigma.D3.Bootloader
{
    internal class WatcherThread : IDisposable
    {
        private MemoryContext _ctx;
        private Thread _thread;
        private const int _defaultUpdateInterval = 10;
        private int _updateInterval;
        private List<Action<MemoryContext>> _tasks = new List<Action<MemoryContext>>();
        private readonly object _tasksLock = new object();
        private ManualResetEvent _stopSignal = new ManualResetEvent(false);
        private Stopwatch _timeSpent;

        public WatcherThread(MemoryContext ctx)
            : this(ctx, _defaultUpdateInterval) { }

        public WatcherThread(MemoryContext ctx, int updateInterval)
        {
            _ctx = ctx;
            _updateInterval = updateInterval;
            _thread = new Thread(Run) { IsBackground = true };
        }

        public void Start()
        {
            lock (_thread)
            {
                _thread.Start();
            }
        }

        private DateTime _lastUpdate;
        private void Run()
        {
            _timeSpent = new Stopwatch();
            while (Continue())
            {
                _timeSpent.Restart();
                lock (_tasksLock)
                {
                    var now = DateTime.UtcNow;
                    //System.Diagnostics.Trace.WriteLine("DeltaTick: " + (now - _lastUpdate).TotalMilliseconds + "ms");
                    _lastUpdate = now;

                    foreach (var task in _tasks)
                    {
                        task.Invoke(_ctx);
                    }
                }
                _timeSpent.Stop();
                _stopSignal.WaitOne(Math.Max(1, _updateInterval - (int)_timeSpent.ElapsedMilliseconds));
            }
        }

        private bool Continue() => _stopSignal.WaitOne(0) == false;

        public void Stop()
        {
            _stopSignal.Set();
        }

        public void Dispose()
        {
            Stop();
            _thread.Join(5000);
        }

        public void AddTask(Action<MemoryContext> task)
        {
            lock (_tasksLock)
            {
                _tasks.Add(task);
            }
        }
    }
}
