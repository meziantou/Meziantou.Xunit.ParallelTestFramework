using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

[Collection("Sequential")]
public class CollectionAttributeTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public CollectionAttributeTests(ConcurrencyFixture fixture)
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