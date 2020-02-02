using System;

namespace Outkeep
{
    /// <summary>
    /// Implements a system clock that is fixed in time.
    /// </summary>
    public class NullClock : ISystemClock
    {
        /// <summary>
        /// Gets or sets the date and time that this clock will show.
        /// </summary>
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.MinValue;
    }
}