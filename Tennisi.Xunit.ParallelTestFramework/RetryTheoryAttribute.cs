using Xunit;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <summary>
/// Represents a custom xUnit attribute to retry a theory test a specified number of times.
/// </summary>
/// <remarks>
/// This attribute is useful for theory tests that may occasionally fail due to transient issues.
/// When used, it will retry the test up to the specified <see cref="RetryCount"/> value for each set of inputs.
/// </remarks>
/// <example>
/// <code>
/// [RetryTheory(retryCount: 3)]
/// [InlineData(1)]
/// [InlineData(2)]
/// public void TestMethod(int input)
/// {
///     // Test code here
/// }
/// </code>
/// </example>
[XunitTestCaseDiscoverer("Tennisi.Xunit.RetryTheoryDiscoverer", "Tennisi.Xunit.ParallelTestFramework")]
public sealed class RetryTheoryAttribute : TheoryAttribute
{
    public int RetryCount { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryTheoryAttribute"/> class with the specified retry count.
    /// </summary>
    /// <param name="retryCount">The number of retry attempts for the test. Must be greater than one.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="retryCount"/> is less than or equal to one.
    /// </exception>
    public RetryTheoryAttribute(int retryCount)
    {
        RetryCount = RetryHelper.Verify(retryCount);
    }
}