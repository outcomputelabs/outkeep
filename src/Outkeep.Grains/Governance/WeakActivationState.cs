using Orleans;
using Orleans.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Grains.Governance
{
    /// <summary>
    /// Default implementation of <see cref="IWeakActivationState{T}"/>.
    /// </summary>
    internal sealed class WeakActivationState<TState> : IWeakActivationState<TState>, ILifecycleParticipant<IGrainLifecycle>
        where TState : new()
    {
        private readonly IGrainActivationContext _context;
        private readonly IResourceGovernor<TState> _governor;

        public WeakActivationState(IGrainActivationContext context, IResourceGovernor<TState> governor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _governor = governor ?? throw new ArgumentNullException(nameof(governor));

            State = new TState();
        }

        private bool _enlisted;

        public TState State { get; }

        public Task ApplyAsync()
        {
            _enlisted = true;
            return _governor.EnlistAsync(_context.GrainInstance.AsReference<IGrainControlExtension>(), State);
        }

        public void Participate(IGrainLifecycle lifecycle)
        {
            lifecycle.Subscribe(GetType().FullName, GrainLifecycleStage.Last, OnLast);
        }

        private Task OnLast(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            if (_enlisted)
            {
                // todo: add a check to prevent a redundant call when the grain is deactivated from the resource governor
                return _governor.LeaveAsync(_context.GrainInstance.AsReference<IGrainControlExtension>());
            }

            return Task.CompletedTask;
        }
    }
}