using EventSystem;
using VContainer;
using VContainer.Unity;

namespace EventSystem.Samples
{
    public sealed class ExampleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEventBus();
            builder.RegisterEntryPoint<ScoreExample>();
        }
    }
}
