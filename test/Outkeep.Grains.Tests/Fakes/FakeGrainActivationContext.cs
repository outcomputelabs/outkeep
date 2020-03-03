using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using System;
using System.Collections.Generic;

namespace Outkeep.Grains.Tests.Fakes
{
    public class FakeGrainActivationContext : IGrainActivationContext
    {
        public Type? GrainType { get; set; }

        public IGrainIdentity? GrainIdentity { get; set; }

        public IServiceProvider? ActivationServices { get; set; }

        public Grain? GrainInstance { get; set; }

        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public IGrainLifecycle? ObservableLifecycle { get; set; }
    }
}