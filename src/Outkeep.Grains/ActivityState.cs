using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    /// <summary>
    /// Default implementation of <see cref="IActivityState"/>.
    /// </summary>
    internal class ActivityState : IActivityState
    {
        /// <inheritdoc />
        public bool IsBusy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ActivityPriority Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public long Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task WriteStateAsync()
        {
            throw new NotImplementedException();
        }
    }
}