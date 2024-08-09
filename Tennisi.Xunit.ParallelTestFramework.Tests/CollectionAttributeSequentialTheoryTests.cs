using Xunit;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

[Collection("CollectionAttribute_SequentialTheoryTests")]
public class CollectionAttributeSequentialTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public CollectionAttributeSequentialTheoryTests(ConcurrencyFixture fixture)
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