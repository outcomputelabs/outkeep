using System;

namespace Outkeep.Time
{
    /// <summary>
    /// Default implementation of <see cref="ISystemClock"/> using <see cref="DateTimeOffset.UtcNow"/>.
    /// </summary>
    internal sealed class SystemClock : ISystemClock
    {
        /// <inheritdoc />
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}