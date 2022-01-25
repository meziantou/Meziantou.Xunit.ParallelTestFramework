By default, all test cases in a test class share the same collection instance which ensures they run synchronously.
This package automatically attribute a unique collection to all test cases, so they can run in parallel.

````
dotnet add package Meziantou.Xunit.ParallelTestFramework
````

This package parallelize all test cases, except those in a explicit collection or Theory with non discoverable data.

````c#
// All tests are run in parallel
public class ParallelTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);

    [Theory]
    [InlineData(0), InlineData(1), InlineData(2)]
    public void Test3(int value) => Thread.Sleep(2000);

    // This test runs in parallel of other tests
    // However, its test cases are run sequentially because of DisableDiscoveryEnumeration
    [Theory]
    [MemberData(nameof(GetData), DisableDiscoveryEnumeration = true)]
    public void Test4(int value) => Thread.Sleep(2000);

    public static TheoryData<int> GetData() =>  new() { { 0 }, { 1 } };
}

// The collection is explicit => no parallelization
[Collection("Sequential")]
public class SequentialTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);
}
````


The code is greatly inspired by @tmort93: https://github.com/xunit/xunit/issues/1986#issuecomment-831322722
