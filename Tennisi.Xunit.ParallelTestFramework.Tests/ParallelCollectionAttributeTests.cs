using Xunit;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

[Collection("Parallel")]
[EnableParallelization]
public class ParallelCollectionAttributeTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public ParallelCollectionAttributeTests(ConcurrencyFixture fixture)
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