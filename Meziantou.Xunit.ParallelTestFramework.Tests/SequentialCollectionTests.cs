using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

[DisableParallelization]
public class SequentialCollectionTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Fact]
    public async Task Fact1()
    {
        Assert.Equal(1, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task Fact2()
    {
        Assert.Equal(1, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }
}