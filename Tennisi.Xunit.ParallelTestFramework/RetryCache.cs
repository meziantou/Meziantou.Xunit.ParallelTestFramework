using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal static class RetryCache
{
    private static readonly ConcurrentDictionary<ITestMethod, bool> MethodCache = new();
    
    private static readonly ConcurrentDictionary<ITypeInfo, int> TypeCache = new();
    
    internal static bool ShouldUseClassRetry(this ITestMethod testMethod, out int retryCount)
    {
        var result = MethodCache.GetOrAdd(testMethod, mtd =>
        {
            var isMethodAttribute = mtd.Method.GetCustomAttributes(typeof(RetryFactAttribute)).Any()
                                    || mtd.Method.GetCustomAttributes(typeof(RetryTheoryAttribute)).Any();

            return isMethodAttribute;
        });

        if (!result)
        {
            var cnt = TypeCache.GetOrAdd(testMethod.TestClass.Class, cls =>
            {
                var retryClassAttribute = cls
                    .GetCustomAttributes(typeof(RetryClassAttribute))
                    .FirstOrDefault();
                return retryClassAttribute.GetRetryCountOrDefault();
            });
            if (!cnt.IsDefaultRetryCount())
            {
                retryCount = cnt;
                return true;
            }
        }
        retryCount = int.MinValue;
        return false;
    }
}