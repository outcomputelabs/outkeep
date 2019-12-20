using Orleans.Services;

namespace Outkeep.Interfaces
{
    public interface ICacheDirectorGrainServiceClient : IGrainServiceClient<ICacheDirectorGrainService>, ICacheDirectorGrainService
    {
    }
}