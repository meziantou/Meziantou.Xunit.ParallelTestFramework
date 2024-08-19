using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal static class ParallelSettings
{
    private static readonly ConcurrentDictionary<string, bool> TestCollectionsCache = new();

    public static void RefineParallelSetting(AssemblyName assemblyName, ITestFrameworkOptions opts, string setting, bool value)
    {
        RefineParallelSetting(assemblyName.FullName, opts, setting, value);
    }
    
    public static void RefineParallelSetting(string assemblyName, ITestFrameworkOptions opts, string setting, bool value)
    {
        if (ShouldForceParallelize(assemblyName))
        {
            opts.SetValue(setting, value);
        }
    }
    
    private static bool ShouldForceParallelize(string assemblyName)
    {
        return TestCollectionsCache.GetOrAdd(assemblyName , name =>
        {
            var assembly = Assembly.Load(new AssemblyName(name));
            var attributeType = typeof(FullTestParallelizationAttribute);
            var attributes = assembly.GetCustomAttributes(attributeType, false);
            var result = attributes.Any();
            return result;
        });
    }
}