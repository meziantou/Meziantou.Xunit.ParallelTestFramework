using Xunit;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

public class ParallelCollectionTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public ParallelCollectionTests(ConcurrencyFixture fixture)
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