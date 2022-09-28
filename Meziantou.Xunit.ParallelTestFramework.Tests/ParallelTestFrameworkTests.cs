using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class ParallelTestFrameworkTests : IClassFixture<ParallelTestFrameworkFixture>
{
    private readonly ParallelTestFrameworkFixture fixture;

    public ParallelTestFrameworkTests(ParallelTestFrameworkFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void FactsRunInParallel1()
    {
        Assert.True(fixture.FactBarrier.SignalAndWait(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void FactsRunInParallel2()
    {
        Assert.True(fixture.FactBarrier.SignalAndWait(TimeSpan.FromSeconds(1)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void TheoriesRunInParallel(int value)
    {
        Assert.True(fixture.TheoryBarrier.SignalAndWait(TimeSpan.FromSeconds(1)));
    }
}