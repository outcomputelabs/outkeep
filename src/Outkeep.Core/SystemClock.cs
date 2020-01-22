using System;

namespace Outkeep.Core
{
    /// <summary>
    /// Default implementation of <see cref="ISystemClock"/> using <see cref="DateTimeOffset.UtcNow"/>.
    /// </summary>
    public sealed class SystemClock : ISystemClock
    {
        /// <inheritdoc />
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}