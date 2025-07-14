using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

[Collection("Parallel")]
[EnableParallelization]
public class ParallelCollectionAttributeTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
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