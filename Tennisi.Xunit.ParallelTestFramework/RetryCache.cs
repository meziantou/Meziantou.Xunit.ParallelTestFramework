using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal static class RetryCache
{
    private static readonly ConcurrentDictionary<(ITestMethod Method, ITypeInfo Class), RetryEntry> Cache = new();

    internal static bool ShouldUseClassRetry(this ITestMethod testMethod, out int retryCount)
    {
        var entry = Cache.GetOrAdd((testMethod, testMethod.TestClass.Class), tuple =>
        {
            var mtd = tuple.Method;
            var hasMethodAttribute = mtd.Method.GetCustomAttributes(typeof(RetryFactAttribute)).Any()
                                     || mtd.Method.GetCustomAttributes(typeof(RetryTheoryAttribute)).Any();
            if (hasMethodAttribute)
            {
                return new RetryEntry(false, int.MinValue);
            }
            var cls = tuple.Class;
            var retryClassAttribute = cls.GetCustomAttributes(typeof(RetryClassAttribute)).FirstOrDefault();
            if (retryClassAttribute == null)
                return new RetryEntry(false, int.MinValue);
            var retryCount = retryClassAttribute.GetRetryCountOrDefault();
            var hasRetry = !retryCount.IsDefaultRetryCount();
            return new RetryEntry(hasRetry, retryCount);
        });
        
        retryCount = entry.RetryCount;
        return entry.HasRetry;
    }

    private readonly struct RetryEntry
    {
        internal bool HasRetry { get; }
        internal int RetryCount { get; }

        internal RetryEntry(bool hasRetry, int retryCount)
        {
            HasRetry = hasRetry;
            RetryCount = retryCount;
        }
    }
}
