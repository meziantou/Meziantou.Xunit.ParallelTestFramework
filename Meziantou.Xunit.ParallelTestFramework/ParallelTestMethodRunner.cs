using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Meziantou.Xunit;

public class ParallelTestMethodRunner : XunitTestMethodRunner
{
    readonly object[] constructorArguments;
    readonly IMessageSink diagnosticMessageSink;

    public ParallelTestMethodRunner(ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments)
        : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, constructorArguments)
    {
        this.constructorArguments = constructorArguments;
        this.diagnosticMessageSink = diagnosticMessageSink;
    }

    // This method has been slightly modified from the original implementation to run tests in parallel
    // https://github.com/xunit/xunit/blob/2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/TestMethodRunner.cs#L130-L142
    protected override async Task<RunSummary> RunTestCasesAsync()
    {
        var disableParallelization = TestMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute)).Any(a => a.GetNamedArgument<bool>(nameof(MemberDataAttribute.DisableDiscoveryEnumeration)));

        if (disableParallelization)
            return await base.RunTestCasesAsync().ConfigureAwait(false);

        var summary = new RunSummary();

        var caseTasks = TestCases.Select(RunTestCaseAsync);
        var caseSummaries = await Task.WhenAll(caseTasks).ConfigureAwait(false);

        foreach (var caseSummary in caseSummaries)
        {
            summary.Aggregate(caseSummary);
        }

        return summary;
    }

    protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        var args = constructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a).ToArray();

        return await Task.Run(() => testCase.RunAsync(diagnosticMessageSink, MessageBus, args, new ExceptionAggregator(Aggregator), CancellationTokenSource)).ConfigureAwait(false);
    }
}
