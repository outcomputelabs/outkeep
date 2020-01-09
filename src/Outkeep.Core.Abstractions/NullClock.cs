using System;

namespace Outkeep.Core
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

        public static NullClock Default => new NullClock();
    }
}