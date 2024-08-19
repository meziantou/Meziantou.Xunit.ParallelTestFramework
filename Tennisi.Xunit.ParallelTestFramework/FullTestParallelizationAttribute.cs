namespace Tennisi.Xunit;

/// <summary>
/// Форсирует Xunit parallelizeTestCollections & preEnumerateTheories параметры в положение 'вкыл'
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class FullTestParallelizationAttribute : Attribute
{
}