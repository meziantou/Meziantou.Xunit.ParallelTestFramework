using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class MemberDataAttributeTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public static TheoryData<int> GetData() => new() { { 1 }, { 2 } };

    public MemberDataAttributeTests(ConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [MemberData(nameof(GetData), DisableDiscoveryEnumeration = true)]
    public async Task Theory(int value)
    {
        Assert.Equal(1, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }
}