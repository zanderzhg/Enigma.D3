using Enigma.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MapHack
{
    internal class EventBus
    {
        public static readonly EventBus Default = new EventBus();

        class Subscription
        {
            public Type EventType;
            public object Handler;
        }

        private readonly List<Subscription> _subscriptions = new List<Subscription>();

        public event Action<Exception, object> Error;

        public IDisposable On<T>(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var subscription = new Subscription { EventType = typeof(T), Handler = handler };
            lock (_subscriptions)
                _subscriptions.Add(subscription);
            return new DisposeDelegate(() => Unsubscribe(subscription));
        }

        private void Unsubscribe(Subscription subscription)
        {
            lock (_subscriptions)
                _subscriptions.Remove(subscription);
        }

        public void PublishAsync<T>(T appEvent)
        {
            Execute.OnUIThreadAsync(() => PublishCore(appEvent));
        }

        public void Publish<T>(T appEvent)
        {
            Execute.OnUIThread(() => PublishCore(appEvent));
        }

        private void PublishCore<T>(T appEvent)
        {
            var currentSubscriptions = default(Subscription[]);
            lock (_subscriptions)
                currentSubscriptions = _subscriptions.ToArray();

            foreach (var subscription in currentSubscriptions)
            {
                if (subscription.EventType.IsAssignableFrom(typeof(T)))
                {
                    try
                    {
                        (subscription.Handler as Action<T>).Invoke(appEvent);
                    }
                    catch (Exception exception)
                    {
                        Error?.Invoke(exception, appEvent);
                    }
                }
            }
        }
    }

    internal class DisposeDelegate : IDisposable
    {
        private readonly Action _action;

        public DisposeDelegate(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        public void Dispose()
        {
            _action.Invoke();
        }
    }
}
