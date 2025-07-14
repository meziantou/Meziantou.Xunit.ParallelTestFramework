using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class BlockingTheoryTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Theory(int _)
    {
        Assert.Equal(2, fixture.CheckConcurrencyAsync().Result);
    }
}