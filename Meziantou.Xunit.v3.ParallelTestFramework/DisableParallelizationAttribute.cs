#pragma warning disable IDE1006 // Naming Styles
namespace Meziantou.Xunit.v3;
#pragma warning restore IDE1006 // Naming Styles

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class DisableParallelizationAttribute : Attribute
{
}
