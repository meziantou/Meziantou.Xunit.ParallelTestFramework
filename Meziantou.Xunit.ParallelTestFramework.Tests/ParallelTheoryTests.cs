using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class ParallelTheoryTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Theory(int _)
    {
        Assert.Equal(2, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }
}