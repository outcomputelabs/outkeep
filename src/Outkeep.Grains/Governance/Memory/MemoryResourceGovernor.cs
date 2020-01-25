using Microsoft.Extensions.Logging;
using Orleans.Core;
using Orleans.Runtime;
using Outkeep.Core.Governance;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains.Governance.Memory
{
    public class MemoryResourceGovernor : GrainService
    {
        private readonly IMemoryPressureMonitor _monitor;

        public MemoryResourceGovernor(IGrainIdentity grainId, Silo silo, ILoggerFactory loggerFactory, IMemoryPressureMonitor monitor)
            : base(grainId, silo, loggerFactory)
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        }

        public override Task Start()
        {
            RegisterTimer(TickGovern, this, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            return base.Start();
        }

        private static Task TickGovern(object state)
        {
            // todo: do maintenance here

            return Task.CompletedTask;
        }
    }
}