using System;
using Cysharp.Threading.Tasks;

namespace EventSystem
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> callback);
        void Unsubscribe<T>(Action<T> callback);
        void SubscribeAsync<T>(Func<T, UniTask> callback);
        void UnsubscribeAsync<T>(Func<T, UniTask> callback);
        UniTask PublishAsync<T>(T eventData);
        void Publish<T>(T eventMessage);
    }
}
