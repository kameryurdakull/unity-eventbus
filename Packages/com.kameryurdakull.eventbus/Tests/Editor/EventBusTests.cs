using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using VContainer;

namespace EventSystem.Tests
{
    public sealed class EventBusTests
    {
        [Test]
        public void Publish_UsesSubscriptionSnapshot()
        {
            var bus = new EventBus();
            var calls = 0;
            Action<int> second = _ => calls++;
            Action<int> first = _ =>
            {
                calls++;
                bus.Unsubscribe(second);
            };

            bus.Subscribe(first);
            bus.Subscribe(second);

            bus.Publish(1);
            bus.Publish(2);

            Assert.That(calls, Is.EqualTo(3));
        }

        [Test]
        public async Task PublishAsync_AwaitsAsyncSubscribers()
        {
            var bus = new EventBus();
            var completion = new UniTaskCompletionSource();
            var calls = 0;

            bus.Subscribe<int>(_ => calls++);
            bus.SubscribeAsync<int>(async _ =>
            {
                await completion.Task;
                calls++;
            });

            var publish = bus.PublishAsync(1);
            Assert.That(calls, Is.EqualTo(1));

            completion.TrySetResult();
            await publish;

            Assert.That(calls, Is.EqualTo(2));
        }

        [Test]
        public void RegisterEventBus_ResolvesSingleton()
        {
            var builder = new ContainerBuilder();
            builder.RegisterEventBus();

            using (var container = builder.Build())
            {
                var first = container.Resolve<IEventBus>();
                var second = container.Resolve<IEventBus>();

                Assert.That(first, Is.SameAs(second));
            }
        }
    }
}
