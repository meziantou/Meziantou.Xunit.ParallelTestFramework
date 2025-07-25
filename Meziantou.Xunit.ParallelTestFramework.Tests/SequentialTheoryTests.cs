using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class SequentialTheoryTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [DisableParallelization]
    public async Task Theory(int _)
    {
        Assert.Equal(1, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }
}
