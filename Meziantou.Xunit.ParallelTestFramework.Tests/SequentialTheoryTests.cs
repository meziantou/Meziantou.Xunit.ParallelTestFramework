using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class SequentialTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public SequentialTheoryTests(ConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [DisableParallelization]
    public async Task Theory(int value)
    {
        Assert.Equal(1, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }
}