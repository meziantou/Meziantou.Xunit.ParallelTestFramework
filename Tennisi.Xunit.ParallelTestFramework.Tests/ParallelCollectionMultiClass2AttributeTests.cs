using Xunit;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

[Collection("ParallelMultiClass")]
public class ParallelCollectionMultiClass2AttributeTests
{
    private readonly CollectionConcurrencyFixture fixture;

    public ParallelCollectionMultiClass2AttributeTests(CollectionConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task Fact1()
    {
        Assert.Equal(2, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task Fact2()
    {
        Assert.Equal(2, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }
}