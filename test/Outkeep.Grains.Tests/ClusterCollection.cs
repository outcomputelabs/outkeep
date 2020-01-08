using Xunit;

namespace Outkeep.Grains.Tests
{
    [CollectionDefinition(nameof(ClusterCollection))]
    public class ClusterCollection : ICollectionFixture<ClusterFixture>
    {
    }
}