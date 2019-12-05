using Outkeep.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Orleans
{
    public static class GrainFactoryInterfaceExtensions
    {
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static ICacheGrain GetCacheGrain(this IGrainFactory factory, string key)
        {
            if (factory == null) ThrowFactoryNull();
            if (key == null) ThrowKeyNull();

            return factory.GetGrain<ICacheGrain>(key);

            void ThrowFactoryNull() => throw new ArgumentNullException(nameof(factory));
            void ThrowKeyNull() => throw new ArgumentNullException(nameof(key));
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static IEchoGrain GetEchoGrain(this IGrainFactory factory)
        {
            if (factory == null) ThrowFactoryNull();

            return factory.GetGrain<IEchoGrain>(Guid.Empty);

            void ThrowFactoryNull() => throw new ArgumentNullException(nameof(factory));
        }
    }
}