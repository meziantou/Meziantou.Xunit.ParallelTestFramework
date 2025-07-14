using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class BlockingCollectionTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
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