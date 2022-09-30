using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class ParallelTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public ParallelTheoryTests(ConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Theory(int value)
    {
        Assert.Equal(2, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }
}