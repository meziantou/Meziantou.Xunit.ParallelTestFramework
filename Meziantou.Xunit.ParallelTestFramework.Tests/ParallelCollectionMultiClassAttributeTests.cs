using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

[CollectionDefinition("ParallelMultiClass")]
[EnableParallelization]
public class ParallelMultiClassCollection : ICollectionFixture<CollectionConcurrencyFixture>
{
}

[Collection("ParallelMultiClass")]
public class ParallelCollectionMultiClass1AttributeTests
{
    private readonly CollectionConcurrencyFixture fixture;

    public ParallelCollectionMultiClass1AttributeTests(CollectionConcurrencyFixture fixture)
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