namespace Tennisi.Xunit;

/// <summary>
/// An assembly-level attribute that enables <c>parallelizeTestCollections</c> and <c>preEnumerateTheories</c> in the xUnit framework. 
/// Alternatively, you can set the <c>FullTestParallelization</c> property in your project file.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class FullTestParallelizationAttribute : Attribute
{
}