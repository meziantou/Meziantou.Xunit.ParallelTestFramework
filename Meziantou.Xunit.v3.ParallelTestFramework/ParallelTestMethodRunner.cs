using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

#pragma warning disable IDE1006 // Naming Styles
namespace Meziantou.Xunit.v3;
#pragma warning restore IDE1006 // Naming Styles

public sealed class ParallelTestMethodRunner : XunitTestMethodRunnerBase<XunitTestMethodRunnerContext, IXunitTestMethod, IXunitTestCase>
{
    private readonly ParallelTestExecutionContext _parallelTestExecutionContext;

    internal ParallelTestMethodRunner(ParallelTestExecutionContext parallelTestExecutionContext)
    {
        _parallelTestExecutionContext = parallelTestExecutionContext;
    }

    internal async ValueTask<RunSummary> Run(
        IXunitTestMethod testMethod,
        IReadOnlyCollection<IXunitTestCase> testCases,
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        object?[] constructorArguments)
    {
        var ctxt = new XunitTestMethodRunnerContext(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, constructorArguments);
        await using (ctxt.ConfigureAwait(false))
        {
            await ctxt.InitializeAsync().ConfigureAwait(false);
            return await Run(ctxt).ConfigureAwait(false);
        }
    }

    protected override async ValueTask<RunSummary> RunTestCases(XunitTestMethodRunnerContext ctxt, Exception? exception)
    {
        if (ctxt is null)
            throw new ArgumentNullException(nameof(ctxt));

        var testMethod = ctxt.TestMethod;
        var disableParallelization =
            testMethod.TestClass.Class.GetCustomAttributes<DisableParallelizationAttribute>(inherit: true).Any()
            || testMethod.TestClass.Class.GetCustomAttributes<CollectionAttribute>(inherit: true).Any()
            || testMethod.Method.GetCustomAttributes<DisableParallelizationAttribute>(inherit: true).Any()
            || testMethod.Method.GetCustomAttributes<MemberDataAttribute>(inherit: true).Any(a => a.DisableDiscoveryEnumeration);

        if (disableParallelization)
            return await base.RunTestCases(ctxt, exception).ConfigureAwait(false);

        var summary = new RunSummary();
        var caseTasks = ctxt.TestCases.Select(testCase => RunTestCase(ctxt, testCase).AsTask());
        var caseSummaries = await Task.WhenAll(caseTasks).ConfigureAwait(false);

        foreach (var caseSummary in caseSummaries)
        {
            summary.Aggregate(caseSummary);
        }

        return summary;
    }

    protected override async ValueTask<RunSummary> RunTestCase(XunitTestMethodRunnerContext ctxt, IXunitTestCase testCase)
    {
        if (ctxt is null)
            throw new ArgumentNullException(nameof(ctxt));

        // Handle conservative parallelism
        await _parallelTestExecutionContext.WaitAsync(ctxt.CancellationTokenSource.Token).ConfigureAwait(false);
        try
        {
            // Create a new TestOutputHelper for each test case since they cannot be reused when running in parallel
            var args = ctxt.ConstructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a).ToArray();
            var newCtxt = new XunitTestMethodRunnerContext(ctxt.TestMethod, ctxt.TestCases, ctxt.ExplicitOption, ctxt.MessageBus, ctxt.Aggregator, ctxt.CancellationTokenSource, args);
            await using (newCtxt.ConfigureAwait(false))
            {
                await newCtxt.InitializeAsync().ConfigureAwait(false);

                Task<RunSummary> Action() => base.RunTestCase(newCtxt, testCase).AsTask();

                // Handle aggressive parallelism
                // Respect MaxParallelThreads by using the MaxConcurrencySyncContext if it exists, mimicking how collections are run
                // https://github.com/xunit/xunit/blob/v2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/XunitTestAssemblyRunner.cs#L169-L176
                if (_parallelTestExecutionContext.SynchronizationContext != null)
                {
                    SynchronizationContext.SetSynchronizationContext(_parallelTestExecutionContext.SynchronizationContext);
                    var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    return await Task.Factory.StartNew(Action, ctxt.CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap().ConfigureAwait(false);
                }

                return await Task.Run(Action, ctxt.CancellationTokenSource.Token).ConfigureAwait(false);
            }
        }
        finally
        {
            _parallelTestExecutionContext.Release();
        }
    }
}
