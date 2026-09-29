using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace EventSystem
{
    public sealed class EventBus : IEventBus
    {
        private sealed class Subscribers<T>
        {
            public Action<T>[] Synchronous { get; private set; } = Array.Empty<Action<T>>();
            public Func<T, UniTask>[] Asynchronous { get; private set; } = Array.Empty<Func<T, UniTask>>();

            public void Add(Action<T> callback)
            {
                var current = Synchronous;
                var updated = new Action<T>[current.Length + 1];
                Array.Copy(current, updated, current.Length);
                updated[current.Length] = callback;
                Synchronous = updated;
            }

            public void Remove(Action<T> callback)
            {
                var current = Synchronous;
                var index = Array.IndexOf(current, callback);
                if (index < 0) return;

                if (current.Length == 1)
                {
                    Synchronous = Array.Empty<Action<T>>();
                    return;
                }

                var updated = new Action<T>[current.Length - 1];
                Array.Copy(current, 0, updated, 0, index);
                Array.Copy(current, index + 1, updated, index, current.Length - index - 1);
                Synchronous = updated;
            }

            public void AddAsync(Func<T, UniTask> callback)
            {
                var current = Asynchronous;
                var updated = new Func<T, UniTask>[current.Length + 1];
                Array.Copy(current, updated, current.Length);
                updated[current.Length] = callback;
                Asynchronous = updated;
            }

            public void RemoveAsync(Func<T, UniTask> callback)
            {
                var current = Asynchronous;
                var index = Array.IndexOf(current, callback);
                if (index < 0) return;

                if (current.Length == 1)
                {
                    Asynchronous = Array.Empty<Func<T, UniTask>>();
                    return;
                }

                var updated = new Func<T, UniTask>[current.Length - 1];
                Array.Copy(current, 0, updated, 0, index);
                Array.Copy(current, index + 1, updated, index, current.Length - index - 1);
                Asynchronous = updated;
            }
        }

        private readonly Dictionary<Type, object> _subscribers = new();

        public void Subscribe<T>(Action<T> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            GetOrCreateSubscribers<T>().Add(callback);
        }

        public void Unsubscribe<T>(Action<T> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            if (_subscribers.TryGetValue(typeof(T), out var subscribers))
                ((Subscribers<T>)subscribers).Remove(callback);
        }

        public void SubscribeAsync<T>(Func<T, UniTask> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            GetOrCreateSubscribers<T>().AddAsync(callback);
        }

        public void UnsubscribeAsync<T>(Func<T, UniTask> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            if (_subscribers.TryGetValue(typeof(T), out var subscribers))
                ((Subscribers<T>)subscribers).RemoveAsync(callback);
        }

        public async UniTask PublishAsync<T>(T eventData)
        {
            if (!_subscribers.TryGetValue(typeof(T), out var subscribers)) return;

            var typedSubscribers = (Subscribers<T>)subscribers;
            var synchronous = typedSubscribers.Synchronous;
            var asynchronous = typedSubscribers.Asynchronous;

            for (var index = 0; index < synchronous.Length; index++)
                synchronous[index](eventData);

            for (var index = 0; index < asynchronous.Length; index++)
                await asynchronous[index](eventData);
        }

        public void Publish<T>(T eventMessage)
        {
            if (!_subscribers.TryGetValue(typeof(T), out var subscribers)) return;

            // Keep the current array stable if a callback changes subscriptions during dispatch.
            var snapshot = ((Subscribers<T>)subscribers).Synchronous;
            for (var index = 0; index < snapshot.Length; index++)
            {
                snapshot[index](eventMessage);
            }
        }

        private Subscribers<T> GetOrCreateSubscribers<T>()
        {
            if (_subscribers.TryGetValue(typeof(T), out var subscribers))
                return (Subscribers<T>)subscribers;

            var created = new Subscribers<T>();
            _subscribers.Add(typeof(T), created);
            return created;
        }
    }
}
