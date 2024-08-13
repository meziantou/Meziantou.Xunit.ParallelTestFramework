using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public sealed class DiagnosticTestMethodRunner : XunitTestMethodRunner
{
    private readonly IMessageSink _diagnosticMessageSink;
    private readonly object[] _constructorArguments;
    private readonly bool _disableParallelization;

    public DiagnosticTestMethodRunner(ITestMethod testMethod,
        IReflectionTypeInfo @class,
        IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        object[] constructorArguments, bool disableParallelization)
        : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator,
            cancellationTokenSource, constructorArguments)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
        _constructorArguments = constructorArguments;
        _disableParallelization = disableParallelization;
    }
    
    protected override async Task<RunSummary> RunTestCasesAsync()
    {
        var disableParallelization =
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute)).Any(a =>
                a.GetNamedArgument<bool>(nameof(MemberDataAttribute.DisableDiscoveryEnumeration)));

        if (disableParallelization || _disableParallelization)
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
        var disableParallelization =
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute)).Any(a =>
                a.GetNamedArgument<bool>(nameof(MemberDataAttribute.DisableDiscoveryEnumeration)));
        
        var parameters = string.Empty;

        if (testCase.TestMethodArguments != null)
        {
            parameters = string.Join(", ", testCase.TestMethodArguments.Select(a => a?.ToString() ?? "null"));
        }

        var test = $"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}({parameters})";

#if DEBUG
        _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"STARTED: {test}"));
#endif

        try
        {
            const int deadlineMinutes = 2;
            using var timer = new Timer(
                _ => _diagnosticMessageSink.OnMessage(
                    new DiagnosticMessage($"WARNING: {test} has been running for more than {deadlineMinutes} minutes")),
                null,
                TimeSpan.FromMinutes(deadlineMinutes),
                Timeout.InfiniteTimeSpan);

            RunSummary result;

            if (!_disableParallelization && !disableParallelization)
            {
                var args = _constructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a)
                    .ToArray();

                var action = () => testCase.RunAsync(_diagnosticMessageSink, MessageBus, args,
                    new ExceptionAggregator(Aggregator), CancellationTokenSource);

                if (SynchronizationContext.Current != null)
                {
                    var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    result = await Task.Factory
                        .StartNew(action, CancellationTokenSource.Token,
                            TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap()
                        .ConfigureAwait(false);
                }
                else
                {
                    result = await Task.Run(action, CancellationTokenSource.Token).ConfigureAwait(false);
                }

            }
            else
            {
                result = await base.RunTestCaseAsync(testCase);
            }

#if DEBUG
            string status;
            if (result.Failed > 0)
                status = "FAILURE";
            else if (result.Skipped > 0)
                status = "SKIPPED";
            else
                status = "SUCCESS";

            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"{status}: {test} ({result.Time}s)"));
#endif

            return result;
        }
        catch (Exception ex)
        {
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"ERROR: {test} ({ex.Message})"));
            throw;
        }
    }
}