using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

[Collection("Sequential")]
public class SequentialCollectionAttributeTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public SequentialCollectionAttributeTests(ConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

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