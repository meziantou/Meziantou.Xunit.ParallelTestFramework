using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

[DisableParallelization]
public class DisabledParallelizationSequentialTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public DisabledParallelizationSequentialTheoryTests(ConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Theory(int value)
    {
        Assert.Equal(1, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }
}
