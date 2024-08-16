using Xunit;
using Xunit.Abstractions;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

public class ThreadTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    public ThreadTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task Test1()
    {
        _testOutputHelper.WriteLine("Test1");
        var tr = new Thread(async () =>
        {
            _testOutputHelper.WriteLine("Test");
            await Task.Delay(1000);
        });
        tr.Start();
        tr.Join();
    }
    
    [Fact]
    public async Task Test2()
    {
        var tr = Task.Factory.StartNew( async () =>
        {
            _testOutputHelper.WriteLine("Test");
            await Task.Delay(1000);
        });
        await tr;
    }
}