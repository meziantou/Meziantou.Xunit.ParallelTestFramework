using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

[Collection(nameof(ParallelTests))]
[EnableParallelization]
public class ParallelTests
{
    public static TheoryData<int> GetData => new TheoryData<int> { 1, 2 };

    [Theory]
    [MemberData(nameof(GetData))]
    public void Test(int _) => Thread.Sleep(2000);
}
