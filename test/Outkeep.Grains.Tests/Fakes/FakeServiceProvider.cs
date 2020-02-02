using System;
using System.Collections.Generic;

namespace Outkeep.Grains.Tests.Fakes
{
    public class FakeServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, Func<object>> _resolvers = new Dictionary<Type, Func<object>>();

        public FakeServiceProvider()
        {
        }

        public FakeServiceProvider(params (Type, Func<object>)[] resolvers)
        {
            if (resolvers is null) throw new ArgumentNullException(nameof(resolvers));

            foreach (var resolver in resolvers)
            {
                if (resolver.Item1 is null) throw new ArgumentNullException(nameof(resolvers));
                if (resolver.Item2 is null) throw new ArgumentNullException(nameof(resolvers));

                _resolvers[resolver.Item1] = resolver.Item2;
            }
        }

        public object? GetService(Type serviceType)
        {
            if (_resolvers.TryGetValue(serviceType, out var resolver))
            {
                return resolver();
            }

            return null;
        }

        public void SetResolver(Type serviceType, Func<object> factory)
        {
            if (serviceType is null) throw new ArgumentNullException(nameof(serviceType));
            if (factory is null) throw new ArgumentNullException(nameof(factory));

            _resolvers[serviceType] = factory;
        }
    }
}