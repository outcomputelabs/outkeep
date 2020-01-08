using Orleans.Runtime.Services;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    public class CacheDirectorGrainServiceClient : GrainServiceClient<ICacheDirectorGrainService>, ICacheDirectorGrainServiceClient
    {
        public CacheDirectorGrainServiceClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public Task PingAsync() => GrainService.PingAsync();
    }
}