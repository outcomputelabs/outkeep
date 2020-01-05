using System;

namespace Outkeep.Core
{
    public class NullClock : ISystemClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.MinValue;

        public static NullClock Instance => new NullClock();
    }
}