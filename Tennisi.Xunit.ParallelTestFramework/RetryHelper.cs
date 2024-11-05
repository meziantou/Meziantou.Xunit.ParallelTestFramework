using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal static class RetryHelper
{
    private const int DefaultRetryCount = 1;
    private const int MaxRetryCount = 10;
    private const string RetryCountName = nameof(RetryTheoryAttribute.RetryCount); 
    
    internal static int GetRetryCount(this IAttributeInfo attributeInfo)
    {
        return attributeInfo.GetNamedArgument<int>(RetryCountName);
    }
    
    internal static bool IsDefaultRetryCount(this int value)
    {
        return value == DefaultRetryCount;
    }
    
    internal static int GetRetryCountOrDefault(this IAttributeInfo? attributeInfo)
    {
        return attributeInfo?.GetRetryCount() ?? DefaultRetryCount;
    }

    internal static void AddRetryCount(this IXunitSerializationInfo serializationInfo, int retryCount)
    {
        serializationInfo.AddValue(RetryCountName, retryCount);
    }
    
    internal static int GetRetryCount(this IXunitSerializationInfo serializationInfo)
    {
        return serializationInfo.GetValue<int>(RetryHelper.RetryCountName);
    }
    
    internal static int Verify(int retryCount)
    {
        if (retryCount is < DefaultRetryCount or > MaxRetryCount)
        {
            throw new ArgumentException($"{nameof(retryCount)} must be between {DefaultRetryCount} and {MaxRetryCount}.", nameof(retryCount));
        }
        return retryCount;
    }

}