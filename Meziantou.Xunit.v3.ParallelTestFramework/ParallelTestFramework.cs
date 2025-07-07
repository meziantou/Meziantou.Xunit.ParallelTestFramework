using System.Reflection;
using Xunit.v3;

namespace Meziantou.Xunit.v3;

public class ParallelTestFramework : XunitTestFramework
{
    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
    {
        return new ParallelTestFrameworkExecutor(new XunitTestAssembly(assembly));
    }
}
