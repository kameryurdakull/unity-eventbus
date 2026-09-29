using System;
using VContainer;

namespace EventSystem
{
    public static class VContainerEventBusExtensions
    {
        public static void RegisterEventBus(this IContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);
        }
    }
}
