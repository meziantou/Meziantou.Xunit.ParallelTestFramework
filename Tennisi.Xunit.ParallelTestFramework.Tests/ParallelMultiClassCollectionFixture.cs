using Xunit;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

[CollectionDefinition("ParallelMultiClass")]
[EnableParallelization]
public class ParallelMultiClassCollectionFixture : ICollectionFixture<CollectionConcurrencyFixture>
{
}