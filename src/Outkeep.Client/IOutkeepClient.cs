using Outkeep.Interfaces;
using System;

namespace Outkeep.Client
{
    public interface IOutkeepClient
    {
        IPingGrain GetPingGrain(Guid key);
    }
}