using Xunit;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

public class BlockingCollectionTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public BlockingCollectionTests(ConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void Fact1()
    {
        Assert.Equal(2, fixture.CheckConcurrencyAsync().Result);
    }

    [Fact]
    public void Fact2()
    {
        Assert.Equal(2, fixture.CheckConcurrencyAsync().Result);
    }
}