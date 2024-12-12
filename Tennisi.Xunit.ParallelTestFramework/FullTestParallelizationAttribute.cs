namespace Tennisi.Xunit;

/// <summary>
/// An assembly-level attribute that enables <c>xunit.discovery.PreEnumerateTheories</c>, <c>xunit.parallelizeTestCollections</c>, <c>xunit.parallelizeAssembly</c>
/// and disables xunit.execution.DisableParallelization in the xUnit framework. 
/// Alternatively, the <c>FullTestParallelization</c> property can be set in the project file to achieve the same effect.
/// </summary>
/// <remarks>
/// <para>
/// Note: The use of <see cref="ParallelTag"/> is supported only when <c>FullTestParallelization</c> is enabled, 
/// ensuring facts or theories received this constructor argument are executed under the parallelization strategy.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class FullTestParallelizationAttribute : Attribute
{
}