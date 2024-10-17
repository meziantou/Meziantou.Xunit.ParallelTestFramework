using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal static class ParallelSettings
{
    private class TestAsm
    {
        public TestAsm(bool force, ITestFrameworkOptions opts)
        {
            Opts = opts;
            Force = force;
        }

        internal bool Force {get; set;}
        internal ITestFrameworkOptions Opts { get; set; }
    }
    private static readonly ConcurrentDictionary<string, TestAsm> TestCollectionsCache = new();

    internal static void RefineParallelSetting(AssemblyName assemblyName, ITestFrameworkOptions opts, string setting, bool value)
    {
        RefineParallelSetting(assemblyName.FullName, opts, setting, value);
    }
    
    
    internal static void RefineParallelSetting(string assemblyName, ITestFrameworkOptions opts, string setting, bool value)
    {
        if (ShouldForceParallelize(assemblyName, opts).Force)
        {
            opts.SetValue(setting, value);
        }
    }

    public static bool GetSetting(string assemblyName, string setting)
    {
        assemblyName = AssemblyInfoExtractor.ExtractNameAndVersion(assemblyName);
        var res = TestCollectionsCache.TryGetValue(assemblyName, out var asm);
        if (!res) throw new InvalidOperationException();
        var val = asm != null && asm.Opts.GetValue<bool>(setting);
        return val;
    }
    
    private static TestAsm ShouldForceParallelize(string assemblyName, ITestFrameworkOptions opts)
    {
        assemblyName = AssemblyInfoExtractor.ExtractNameAndVersion(assemblyName);
        return TestCollectionsCache.GetOrAdd(assemblyName , name =>
        {
            var assembly = Assembly.Load(new AssemblyName(name));
            var attributeType = typeof(FullTestParallelizationAttribute);
            var attributes = assembly.GetCustomAttributes(attributeType, false);
            var result = attributes.Length != 0;
            return new TestAsm(force: result, opts);
        });
    }
}