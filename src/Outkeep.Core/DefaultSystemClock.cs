using System;

namespace Outkeep.Core
{
    /// <summary>
    /// Default implementation of <see cref="ISystemClock"/> using <see cref="DateTimeOffset.UtcNow"/>.
    /// </summary>
    public class DefaultSystemClock : ISystemClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}