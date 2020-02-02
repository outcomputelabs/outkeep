using Outkeep.Governance;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Outkeep.Grains.Tests.Fakes
{
    public class FakeResourceGovernor : IResourceGovernor
    {
        public ConcurrentDictionary<IWeakActivationExtension, FakeWeakActivationFactor> Registrations { get; } = new ConcurrentDictionary<IWeakActivationExtension, FakeWeakActivationFactor>();

        public Task EnlistAsync(IWeakActivationExtension subject, IWeakActivationFactor factor)
        {
            Registrations[subject] = factor switch
            {
                FakeWeakActivationFactor activity => activity,
                _ => throw new NotSupportedException(),
            };
            return Task.CompletedTask;
        }

        public Task LeaveAsync(IWeakActivationExtension subject)
        {
            Registrations.TryRemove(subject, out _);
            return Task.CompletedTask;
        }
    }
}