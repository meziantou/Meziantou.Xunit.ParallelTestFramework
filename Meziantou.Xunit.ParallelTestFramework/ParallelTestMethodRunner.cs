using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Meziantou.Xunit;

public class ParallelTestMethodRunner : XunitTestMethodRunner
{
    private readonly object[] _constructorArguments;
    private readonly IMessageSink _diagnosticMessageSink;

    public ParallelTestMethodRunner(ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments)
        : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, constructorArguments)
    {
        _constructorArguments = constructorArguments;
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    // This method has been slightly modified from the original implementation to run tests in parallel
    // https://github.com/xunit/xunit/blob/2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/TestMethodRunner.cs#L130-L142
    protected override async Task<RunSummary> RunTestCasesAsync()
    {
        var disableParallelization = TestMethod.TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
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
        // Create a new TestOutputHelper for each test case since they cannot be reused when running in parallel
        var args = _constructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a).ToArray();

        Task<RunSummary> Action() => testCase.RunAsync(_diagnosticMessageSink, MessageBus, args, new ExceptionAggregator(Aggregator), CancellationTokenSource);

        // Respect MaxParallelThreads by using the MaxConcurrencySyncContext if it exists, mimicking how collections are run
        // https://github.com/xunit/xunit/blob/2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/XunitTestAssemblyRunner.cs#L169-L176
        if (SynchronizationContext.Current is not null)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return await Task.Factory.StartNew(Action, CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap().ConfigureAwait(false);
        }

        return await Task.Run(Action, CancellationTokenSource.Token).ConfigureAwait(false);
    }
}
