using System.Security.Cryptography;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public sealed class ParallelTestMethodRunner : XunitTestMethodRunner
{
    private readonly IMessageSink _diagnosticMessageSink;
    private readonly object[] _constructorArguments;

    private static readonly TimeSpan TimeLimit = TimeSpan.FromSeconds(120);

    public ParallelTestMethodRunner(ITestMethod testMethod,
        IReflectionTypeInfo @class,
        IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        object[] constructorArguments)
        : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator,
            cancellationTokenSource, constructorArguments)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
        _constructorArguments = constructorArguments;
    }

    protected override async Task<RunSummary> RunTestCasesAsync()
    {
        var disableParallelization =
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute)).Any(a =>
                a.GetNamedArgument<bool>(nameof(MemberDataAttribute.DisableDiscoveryEnumeration)));
        
        var summary = new RunSummary();
        foreach (var testCase in TestCases)
        {
            var runSummary = summary;
            runSummary.Aggregate(await RunTestCaseAsync2(testCase, disableParallelization));
            if (CancellationTokenSource.IsCancellationRequested)
                break;
        }
        return summary;
    }
    
    private async Task<RunSummary> RunTestCaseAsync2(IXunitTestCase testCase, bool disableParallelization)
    {
        if (disableParallelization)
            return await RunDiagnosticTestCaseAsync(testCase, _constructorArguments);
        return await RunTestCaseAsync(testCase);
    }

    protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        var args = _constructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a).ToArray();
        if (args.Length >= 1)
        {
            var lastArg = args[args.Length - 1];
            if (lastArg is null or string)
            {
                args[args.Length - 1] =
                    ParallelTag.ToPositiveInt64Hash(testCase.TestMethod, testCase.TestMethodArguments).ToString();
            }
        }

        var action = () => RunDiagnosticTestCaseAsync(testCase, args);
        if (SynchronizationContext.Current == null)
            return await Task.Run(action, CancellationTokenSource.Token).ConfigureAwait(false);
        var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        return await Task.Factory
                         .StartNew(action, CancellationTokenSource.Token,
                                   TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap()
                         .ConfigureAwait(false);
    }
    
    private async Task<RunSummary> RunDiagnosticTestCaseAsync(IXunitTestCase testCase, object[] args)
    {
        var parameters = testCase.TestMethodArguments != null
            ? string.Join(", ", testCase.TestMethodArguments.Select(a => a?.ToString() ?? "null"))
            : string.Empty;
        var testDetails = $"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}({parameters})";

        try
        {
#if DEBUG
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"STARTED: {testDetails}"));
#endif
            using var timer = new Timer(_ => _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"WARNING: {testDetails} has been running for more than {Math.Round(TimeLimit.TotalMinutes, 2)} minutes")),
                null,
                TimeLimit,
                Timeout.InfiniteTimeSpan);

            var result = await testCase.RunAsync(_diagnosticMessageSink, MessageBus, args, new ExceptionAggregator(Aggregator),
                CancellationTokenSource);

#if DEBUG
            var status = result.Failed > 0 ? "FAILURE" : result.Skipped > 0 ? "SKIPPED" : "SUCCESS";
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"{status}: {testDetails} ({result.Time}s)"));
#endif
            return result;
        }
        catch (Exception ex)
        {
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"ERROR: {testDetails} ({ex.Message})"));
            throw;
        }
    }
}