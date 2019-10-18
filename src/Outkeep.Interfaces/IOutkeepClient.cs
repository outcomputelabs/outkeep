using System;

namespace Outkeep.Interfaces
{
    public interface IOutkeepClient
    {
        IPingGrain GetPingGrain(Guid key);
    }
}