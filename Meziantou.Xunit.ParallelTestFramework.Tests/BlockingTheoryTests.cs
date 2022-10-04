using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class BlockingTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public BlockingTheoryTests(ConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Theory(int value)
    {
        Assert.Equal(2, fixture.CheckConcurrencyAsync().Result);
    }
}