using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public class RetryFactDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public RetryFactDiscoverer(IMessageSink diagnosticMessageSink)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        yield return 
            new RetryTestCase(_diagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(), 
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod,null, factAttribute.GetRetryCount());
    }
}