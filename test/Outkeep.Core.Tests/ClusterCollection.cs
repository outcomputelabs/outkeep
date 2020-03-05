using Xunit;

namespace Outkeep.Core.Tests
{
    [CollectionDefinition(nameof(ClusterCollection))]
    public class ClusterCollection : ICollectionFixture<ClusterFixture>
    {
    }
}