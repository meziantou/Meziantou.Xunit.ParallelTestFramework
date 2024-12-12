using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal static class ParallelSettings
{
    private class TestAsm
    {
        public TestAsm(bool disbale, bool force, ITestFrameworkOptions opts)
        {
            Opts = opts;
            Force = force;
            Disable = disbale;
        }
        internal bool Disable {get; set;}
        internal bool Force {get; set;}
        internal ITestFrameworkOptions Opts { get; set; }
    }
    private static readonly ConcurrentDictionary<string, TestAsm> TestCollectionsCache = new();

    internal static void RefineParallelSetting(AssemblyName assemblyName, ITestFrameworkOptions opts)
    {
        RefineParallelSetting(assemblyName.FullName, opts);
    }
    
    internal static void RefineParallelSetting(string assemblyName, ITestFrameworkOptions opts)
    {
        var behaviour = DetectParallelBehaviour(assemblyName, opts);
        
        if (behaviour.Force && !behaviour.Disable)
        {
            opts.SetValue("xunit.discovery.PreEnumerateTheories", true);
            opts.SetValue("xunit.execution.DisableParallelization", false);
            opts.SetValue("xunit.parallelizeTestCollections", true);
            opts.SetValue("xunit.parallelizeAssembly", true);
        }
        else if (behaviour.Disable)
        {
            opts.SetValue("xunit.discovery.PreEnumerateTheories", false);
            opts.SetValue("xunit.execution.DisableParallelization", true);
            opts.SetValue("xunit.parallelizeTestCollections", false);
            opts.SetValue("xunit.parallelizeAssembly", false);
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
    
    private static TestAsm DetectParallelBehaviour(string assemblyName, ITestFrameworkOptions opts)
    {
        assemblyName = AssemblyInfoExtractor.ExtractNameAndVersion(assemblyName);
        return TestCollectionsCache.GetOrAdd(assemblyName , name =>
        {
            var assembly = Assembly.Load(new AssemblyName(name));
            var force = assembly.GetCustomAttributes(typeof(FullTestParallelizationAttribute), false).Length != 0;
            var disable = assembly.GetCustomAttributes(typeof(DisableTestParallelizationAttribute), false).Length != 0;
            return new TestAsm(force: force, disbale:disable, opts: opts);
        });
    }
}