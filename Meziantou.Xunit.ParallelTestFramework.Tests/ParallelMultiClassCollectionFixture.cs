using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

[CollectionDefinition("ParallelMultiClass")]
[EnableParallelization]
public class ParallelMultiClassCollectionFixture : ICollectionFixture<CollectionConcurrencyFixture>
{
}