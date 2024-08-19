using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal class ParallelTestHelper: ITestOutputHelper
{
    public void WriteLine(string message)
    {
        // Custom logging logic
        Console.WriteLine($"[Custom] {message}");
    }

    public void WriteLine(string format, params object[] args)
    {
        // Custom logging logic
        Console.WriteLine($"[Custom] {format}", args);
    }
}