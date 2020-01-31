using Orleans;
using Orleans.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Governance
{
    /// <summary>
    /// Default implementation of <see cref="IWeakActivationState{T}"/>.
    /// </summary>
    internal sealed class WeakActivationState<TState> : IWeakActivationState<TState>, ILifecycleParticipant<IGrainLifecycle>, ILifecycleObserver
        where TState : IWeakActivationFactor, new()
    {
        private readonly IGrainActivationContext _context;
        private readonly IResourceGovernor _governor;
        private bool _enlisted;

        public WeakActivationState(IGrainActivationContext context, IResourceGovernor governor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _governor = governor ?? throw new ArgumentNullException(nameof(governor));

            State = new TState();
        }

        public TState State { get; }

        public Task EnlistAsync()
        {
            _enlisted = true;
            return _governor.EnlistAsync(_context.GrainInstance.AsReference<IWeakActivationExtension>(), State);
        }

        public void Participate(IGrainLifecycle lifecycle)
        {
            lifecycle.Subscribe(GrainLifecycleStage.SetupState, this);
        }

        public Task OnStart(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task OnStop(CancellationToken ct)
        {
            if (!_enlisted) return Task.CompletedTask;

            return _governor.LeaveAsync(_context.GrainInstance.AsReference<IWeakActivationExtension>());
        }
    }
}