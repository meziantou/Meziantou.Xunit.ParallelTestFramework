By default, xUnit runs all test cases in a test class synchronously.
This package extends the default test framework to execute tests in parallel.

```shell
dotnet add package Meziantou.Xunit.ParallelTestFramework
```

By default, all tests will run in parallel.
Selectively disable parallelization by adding the `DisableParallelization` attribute to a collection or theory.

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

    // This test runs in parallel with other tests
    // However, its test cases are run sequentially
    [Theory]
    [DisableParallelization]
    [InlineData(0), InlineData(1), InlineData(2)]
    public void Test4(int value) => Thread.Sleep(2000);
}

// This collection runs in parallel with other collections
// However, its tests cases are run sequentially
[DisableParallelization]
public class SequentialTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);
}
````

Previous versions of this package relied on built in xUnit attributes instead of exposing a dedicated `DisableParallelization` attribute.
For backwards compatibility, parallelization can also be disabled by adding an explicit `Collection` attribute or `Theory` attribute with `DisableDiscoveryEnumeration` enabled.

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

    // This test runs in parallel with other tests
    // However, its test cases are run sequentially because of DisableDiscoveryEnumeration
    [Theory]
    [MemberData(nameof(GetData), DisableDiscoveryEnumeration = true)]
    public void Test4(int value) => Thread.Sleep(2000);

    public static TheoryData<int> GetData() =>  new() { { 0 }, { 1 } };
}

// This collection runs in parallel with other collections
// However, its tests cases are run sequentially because the Collection is explicit
[Collection("Sequential")]
public class SequentialTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);
}
````

The code is greatly inspired by the sample from [Travis Mortimer](https://github.com/tmort93): <https://github.com/xunit/xunit/issues/1986#issuecomment-831322722>
