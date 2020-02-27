using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Registry
{
    internal sealed class RegistryState<TState> : IRegistryState<TState>, ILifecycleParticipant<IGrainLifecycle>, ILifecycleObserver
        where TState : class, new()
    {
        private readonly IRegistryGrainStorage<TState> _storage;
        private readonly IGrainActivationContext _context;
        private readonly string _fullStateName;

        public RegistryState(IGrainActivationContext context, IRegistryStateConfiguration config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            if (config is null) throw new ArgumentNullException(nameof(config));

            // select the appropriate storage provider
            var storageName = config.StorageName ?? OutkeepProviderNames.DefaultStorageName;
            _storage = _context.ActivationServices.GetRequiredServiceByName<IRegistryGrainStorage<TState>>(storageName);

            // construct the full state name
            var containerName = config.ContainerName ?? _context.GrainType.FullName;
            var registryName = config.RegistryName ?? OutkeepProviderNames.DefaultRegistryName;
            _fullStateName = string.Create(containerName.Length + 1 + registryName.Length, (containerName, registryName), (span, arg) =>
            {
                arg.containerName.AsSpan().CopyTo(span);
                span = span.Slice(arg.containerName.Length);

                span[0] = '.';
                span = span.Slice(1);

                arg.registryName.AsSpan().CopyTo(span);
            });
        }

        public Task OnStart(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task OnStop(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public void Participate(IGrainLifecycle lifecycle)
        {
            lifecycle.Subscribe(GrainLifecycleStage.SetupState, this);
        }

        public IQueryable<IRegistryEntryState<TState>> CreateQuery()
        {
            var provider = _context.ActivationServices.GetRequiredService<IRegistryQueryProvider<TState>>();
            return new RegistryQuery<TState>(provider);
        }
    }
}