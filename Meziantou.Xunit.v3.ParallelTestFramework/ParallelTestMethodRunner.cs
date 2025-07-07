using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Meziantou.Xunit.v3;

public class ParallelTestMethodRunner : XunitTestMethodRunnerBase<XunitTestMethodRunnerContext, IXunitTestMethod, IXunitTestCase>
{
    public static ParallelTestMethodRunner Instance { get; } = new();
    
    public async ValueTask<RunSummary> Run(
        IXunitTestMethod testMethod,
        IReadOnlyCollection<IXunitTestCase> testCases,
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        object?[] constructorArguments)
    {
        var ctxt = new XunitTestMethodRunnerContext(testMethod, testCases, explicitOption, messageBus,
            aggregator, cancellationTokenSource, constructorArguments);
        
        await using (ctxt.ConfigureAwait(false))
        {
            await ctxt.InitializeAsync().ConfigureAwait(false);

            return await Run(ctxt).ConfigureAwait(false);
        }
    }
    
    protected override async ValueTask<RunSummary> RunTestCases(XunitTestMethodRunnerContext ctxt, Exception? exception)
    {
        if (ctxt == null) throw new ArgumentNullException(nameof(ctxt));
        
        var testMethod = ctxt.TestMethod;
        var disableParallelization =
            testMethod.TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute), true).Any()
            || testMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute), true).Any()
            || testMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute), true).Any()
            || testMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute), true).Any(a =>
                ((MemberDataAttribute)a).DisableDiscoveryEnumeration);

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
        
        // Create a new TestOutputHelper for each test case since they cannot be reused when running in parallel
        var args = ctxt.ConstructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a).ToArray();
        var newCtxt = new XunitTestMethodRunnerContext(ctxt.TestMethod, ctxt.TestCases, ctxt.ExplicitOption, ctxt.MessageBus, ctxt.Aggregator, ctxt.CancellationTokenSource, args);
        await using (newCtxt.ConfigureAwait(false))
        {
            await newCtxt.InitializeAsync().ConfigureAwait(false);
                
            var action = () => base.RunTestCase(newCtxt, testCase).AsTask();
            
            // Respect MaxParallelThreads by using the MaxConcurrencySyncContext if it exists, mimicking how collections are run
            // https://github.com/xunit/xunit/blob/v2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/XunitTestAssemblyRunner.cs#L169-L176
            if (SynchronizationContext.Current != null)
            {
                var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                return await Task.Factory.StartNew(action, ctxt.CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap().ConfigureAwait(false);
            }
            
            return await Task.Run(action, ctxt.CancellationTokenSource.Token).ConfigureAwait(false);
        }
    }
}
