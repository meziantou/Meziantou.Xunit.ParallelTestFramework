namespace Meziantou.Xunit.v3;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class DisableParallelizationAttribute : Attribute
{
}
