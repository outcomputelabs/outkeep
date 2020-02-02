using System;

namespace Outkeep
{
    /// <summary>
    /// Abstract calls to the system clock to ease testing.
    /// </summary>
    public interface ISystemClock
    {
        /// <summary>
        /// Gets the current date and time in UTC.
        /// </summary>
        public DateTimeOffset UtcNow { get; }
    }
}