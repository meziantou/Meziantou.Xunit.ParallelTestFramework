namespace Tennisi.Xunit;

/// <summary>
/// Assembly-level attribute that disables <c>xunit.discovery.PreEnumerateTheories</c>, <c>xunit.parallelizeTestCollections</c>, <c>xunit.parallelizeAssembly</c>
/// and enables xunit.execution.DisableParallelization in the xUnit framework.
/// Alternatively, the <c>DisbaleTestParallelization</c> property can be set in the project file to achieve the same effect.
/// </summary>
/// <remarks>
/// <para>
/// When applied, it reverts the behavior of test execution to the standard xUnit execution model.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class DisableTestParallelizationAttribute : Attribute
{
}