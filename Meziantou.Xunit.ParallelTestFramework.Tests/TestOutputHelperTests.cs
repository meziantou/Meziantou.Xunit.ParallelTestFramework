using Xunit;
using Xunit.Abstractions;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class TestOutputHelperTests
{
    private readonly ITestOutputHelper output;

    public TestOutputHelperTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Fact1()
    {
        output.WriteLine(nameof(Fact1));
    }

    [Fact]
    public void Fact2()
    {
        output.WriteLine(nameof(Fact2));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Theory(int value)
    {
        output.WriteLine(nameof(Theory) + value);
    }
}