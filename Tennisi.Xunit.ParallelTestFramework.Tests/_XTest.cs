using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

public class XTest
{
    private readonly int _uniqueTagPerMethod;
    private static readonly ConcurrentDictionary<string, int> _uniqueControl = new ConcurrentDictionary<string, int>();
    private static int _counter = 0;
    
    private readonly ITestOutputHelper _testOutput;
    
    public XTest(ITestOutputHelper testOutputHelper)
    {
        _uniqueTagPerMethod = Interlocked.Increment(ref _counter);
        _testOutput = testOutputHelper;
    }

    private async Task Test(string who)
    {
        var counter = _counter;
        if (_uniqueControl.TryAdd(who, _uniqueTagPerMethod) == false)
        {
            throw new InvalidOperationException("Ne poluchitsa");
        }
        _testOutput.WriteLine($"I'm started when {counter} tests are allready created, My name is {who}, my tag {_uniqueTagPerMethod}");
        await Task.Delay(1000);
        var unqState = string.Join(',',_uniqueControl.Select(a => $"{a.Key}-{a.Value}"));
        _testOutput.WriteLine($"I'm finished. Group Control:{unqState}");
    }

    [Fact]
    public async Task Task1() =>
        await Test(GetTestMethodName());
    
    [Fact]
    public async Task Task2() =>
        await Test(GetTestMethodName());
    
    [Fact]
    public async Task Task3() =>
        await Test(GetTestMethodName());
    
    [Fact]
    public async Task Task4() =>
        await Test(GetTestMethodName());
    
    [Fact]
    public async Task Task5() =>
        await Test(GetTestMethodName());
    
    [Fact]
    public async Task Task6() =>
        await Test(GetTestMethodName());
    
    [Fact]
    public async Task Task7() =>
        await Test(GetTestMethodName());
    
    [Fact]
    public async Task Task8() =>
        await Test(GetTestMethodName());
    
    [Fact]
    public async Task Task9() =>
        await Test(GetTestMethodName());
    
    private string GetTestMethodName()
    {
        // Iterate through the call stack frames to find the method that has the Fact attribute
        var stackTrace = new StackTrace();
        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame.GetMethod() as MethodInfo;
            if (method != null)
            {
                // Check if the method has the [Fact] attribute
                var factAttribute = method.GetCustomAttribute(typeof(FactAttribute), false);
                if (factAttribute != null)
                {
                    return method.Name;
                }
            }
        }
        return null;
    }
}