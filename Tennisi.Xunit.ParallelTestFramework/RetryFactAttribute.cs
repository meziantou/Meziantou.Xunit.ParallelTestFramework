using Xunit;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <summary>
/// Represents a custom xUnit attribute to retry a test a specified number of times.
/// </summary>
/// <remarks>
/// This attribute is useful for tests that may occasionally fail due to transient issues.
/// When used, it will retry the test up to the specified <see cref="RetryCount"/> value.
/// </remarks>
/// <example>
/// <code>
/// [RetryFact(retryCount: 3)]
/// public void TestMethod()
/// {
///     // Test code here
/// }
/// </code>
/// </example>
[XunitTestCaseDiscoverer("Tennisi.Xunit.RetryFactDiscoverer", "Tennisi.Xunit.ParallelTestFramework")]
public sealed class RetryFactAttribute : FactAttribute
{
    public int RetryCount { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryFactAttribute"/> class with the specified retry count.
    /// </summary>
    /// <param name="retryCount">The number of retry attempts for the test. Must be greater than one.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="retryCount"/> is less than or equal to one.
    /// </exception>
    public RetryFactAttribute(int retryCount)
    {
        RetryCount = RetryHelper.Verify(retryCount);
    }
}