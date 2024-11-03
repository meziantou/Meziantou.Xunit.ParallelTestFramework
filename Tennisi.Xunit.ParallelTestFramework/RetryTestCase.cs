using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

[Serializable]
internal class RetryTestCase : IXunitTestCase
{
    private int _retryCount;
    private IXunitTestCase _realCase;
    
    public RetryTestCase(IXunitTestCase baseCase, int retryCount)
    {
        _realCase = baseCase;
        _retryCount = retryCount;
    }

    public RetryTestCase(IMessageSink diagnosticMessageSink,
        TestMethodDisplay defaultMethodDisplay,
        TestMethodDisplayOptions defaultMethodDisplayOptions,
        ITestMethod testMethod, object[] testMethodArguments, int retryCount)
    {
        _realCase = new XunitTestCase(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments);
        _retryCount = retryCount;
    }
    
    public async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        var runCount = 0;

        while (true)
        {
            var delayedMessageBus = new DelayedMessageBus(messageBus);

            var summary = await _realCase.RunAsync(diagnosticMessageSink, delayedMessageBus, constructorArguments,
                aggregator, cancellationTokenSource);
            if (aggregator.HasExceptions || summary.Failed == 0 || ++runCount >= _retryCount)
            {
                delayedMessageBus.Dispose();
                return summary;
            }

            diagnosticMessageSink.OnMessage(
                new DiagnosticMessage("Execution of '{0}' failed (attempt #{1}), test retrying...", DisplayName, runCount));

            await Task.Delay(1, cancellationTokenSource.Token);
        }
    }

    public Exception InitializationException => _realCase.InitializationException;
    public IMethodInfo Method => _realCase.Method;
    public int Timeout => _realCase.Timeout;

    public void Serialize(IXunitSerializationInfo data)
    {
        _realCase.Serialize(data);
        data.AddRetryCount(_retryCount);
    }

    public void Deserialize(IXunitSerializationInfo data)
    {
        _realCase.Deserialize(data);
        _retryCount = data.GetRetryCount();
    }

    public string DisplayName => _realCase.DisplayName;
    public string SkipReason => _realCase.SkipReason;

    public ISourceInformation SourceInformation
    {
        get => _realCase.SourceInformation;
        set => _realCase.SourceInformation = value;
    }
    public ITestMethod TestMethod => _realCase.TestMethod;
    public object[] TestMethodArguments => _realCase.TestMethodArguments;
    public Dictionary<string, List<string>> Traits => _realCase.Traits;
    public string UniqueID => _realCase.UniqueID;
}