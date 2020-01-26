using Orleans.Runtime;
using Outkeep.Grains.Governance;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    public class GrainControlExtension : IGrainControlExtension
    {
        private readonly IGrainActivationContext _context;
        private readonly IGrainRuntime _runtime;

        public GrainControlExtension(IGrainActivationContext context, IGrainRuntime runtime)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public Task DeactivateOnIdleAsync()
        {
            _runtime.DeactivateOnIdle(_context.GrainInstance);

            return Task.CompletedTask;
        }
    }
}