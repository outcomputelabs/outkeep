using Orleans.Services;

namespace Outkeep.Interfaces
{
    /// <summary>
    /// Client for <see cref="ICacheDirectorGrainService"/>.
    /// </summary>
    public interface ICacheDirectorGrainServiceClient : IGrainServiceClient<ICacheDirectorGrainService>, ICacheDirectorGrainService
    {
    }
}