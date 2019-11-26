using System;

namespace Outkeep.Core
{
    /// <summary>
    /// Helps abstract calls to the system clock to ease testing.
    /// </summary>
    public interface ISystemClock
    {
        public DateTimeOffset UtcNow { get; }
    }
}