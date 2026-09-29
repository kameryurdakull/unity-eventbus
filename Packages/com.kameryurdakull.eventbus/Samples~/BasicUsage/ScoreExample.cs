using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace EventSystem.Samples
{
    public sealed class ScoreExample : IStartable, IDisposable
    {
        private readonly IEventBus _eventBus;

        public ScoreExample(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Start()
        {
            _eventBus.Subscribe<ScoreChanged>(OnScoreChanged);
            _eventBus.SubscribeAsync<ScoreChanged>(OnScoreChangedAsync);
            PublishExampleAsync().Forget();
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<ScoreChanged>(OnScoreChanged);
            _eventBus.UnsubscribeAsync<ScoreChanged>(OnScoreChangedAsync);
        }

        private async UniTaskVoid PublishExampleAsync()
        {
            await _eventBus.PublishAsync(new ScoreChanged(100));
        }

        private static void OnScoreChanged(ScoreChanged message)
        {
            Debug.Log($"Score: {message.Value}");
        }

        private static async UniTask OnScoreChangedAsync(ScoreChanged message)
        {
            await UniTask.Yield();
            Debug.Log($"Async score: {message.Value}");
        }
    }

    public readonly struct ScoreChanged
    {
        public int Value { get; }

        public ScoreChanged(int value)
        {
            Value = value;
        }
    }
}
