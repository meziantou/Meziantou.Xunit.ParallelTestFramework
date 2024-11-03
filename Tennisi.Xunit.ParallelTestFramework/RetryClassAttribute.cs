using Xunit;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <summary>
/// Represents a custom xUnit attribute to specify that all test methods 
/// in the class (both facts and theories) should be retried a specified number of times.
/// </summary>
/// <remarks>
/// This attribute is useful for test classes where tests may occasionally fail 
/// due to transient issues such as network delays, timeouts, or other 
/// external dependencies. When applied, each test method in the class, 
/// including both <see cref="FactAttribute"/> and <see cref="TheoryAttribute"/> methods, 
/// will be retried up to the specified <see cref="RetryCount"/> value upon failure.
/// </remarks>
/// <example>
/// <code>
/// [RetryClass(retryCount: 3)]
/// public class MyTests
/// {
///     [Fact]
///     public void TestMethod1()
///     {
///         // Test code for TestMethod1
///     }
///     
///     [Theory]
///     [InlineData(1)]
///     [InlineData(2)]
///     public void TestMethod2(int input)
///     {
///         // Test code for TestMethod2
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RetryClassAttribute : Attribute
{
    /// <summary>
    /// Gets the number of times the test methods in the class should be retried.
    /// </summary>
    public int RetryCount { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryClassAttribute"/> class 
    /// with the specified retry count.
    /// </summary>
    /// <param name="retryCount">The number of retry attempts for test methods in the class. Must be greater than one.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="retryCount"/> is less than or equal to one.
    /// </exception>
    public RetryClassAttribute(int retryCount)
    {
        RetryCount = RetryHelper.Verify(retryCount);
    }
}