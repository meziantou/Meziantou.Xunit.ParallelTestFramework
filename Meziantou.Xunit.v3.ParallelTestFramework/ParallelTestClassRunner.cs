using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Meziantou.Xunit.v3;

public class ParallelTestClassRunner : XunitTestClassRunnerBase<XunitTestClassRunnerContext, IXunitTestClass, IXunitTestMethod, IXunitTestCase>
{
    private readonly ParallelTestExecutionContext _parallelTestExecutionContext;

    internal ParallelTestClassRunner(ParallelTestExecutionContext parallelTestExecutionContext)
    {
        _parallelTestExecutionContext = parallelTestExecutionContext;
    }

    public async ValueTask<RunSummary> Run(
        IXunitTestClass testClass,
        IReadOnlyCollection<IXunitTestCase> testCases,
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        FixtureMappingManager collectionFixtureMappings)
    {
        var ctxt = new XunitTestClassRunnerContext(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler, collectionFixtureMappings);
        await using (ctxt.ConfigureAwait(false))
        {
            await ctxt.InitializeAsync().ConfigureAwait(false);
            return await Run(ctxt).ConfigureAwait(false);
        }
    }

    // This method has been slightly modified from the original implementation to run tests in parallel
    // https://github.com/xunit/xunit/blob/main/src/xunit.v3.core/Runners/TestClassRunner.cs#L254-L292
    protected override async ValueTask<RunSummary> RunTestMethods(XunitTestClassRunnerContext ctxt, Exception? exception)
    {
        if (ctxt is null) throw new ArgumentNullException(nameof(ctxt));

        var disableParallelizationAttribute = ctxt.TestClass.Class.GetCustomAttributes<DisableParallelizationAttribute>().Any();

        var disableParallelizationOnCustomCollection = ctxt.TestClass.Class.GetCustomAttributes<CollectionAttribute>().Any()
                                                       && !ctxt.TestClass.Class.GetCustomAttributes<EnableParallelizationAttribute>().Any();

        var disableParallelization = disableParallelizationAttribute || disableParallelizationOnCustomCollection;

        if (disableParallelization)
            return await base.RunTestMethods(ctxt, exception).ConfigureAwait(false);

        var summary = new RunSummary();
        IReadOnlyCollection<IXunitTestCase> orderedTestCases;
        object?[] constructorArguments;

        if (exception is null)
        {
            orderedTestCases = [.. OrderTestMethods(ctxt).SelectMany(x => x.TestCases)];
            constructorArguments = await CreateTestClassConstructorArguments(ctxt).ConfigureAwait(false);
            exception = ctxt.Aggregator.ToException();
            ctxt.Aggregator.Clear();
        }
        else
        {
            orderedTestCases = ctxt.TestCases;
            constructorArguments = Array.Empty<object?>();
        }

        var methodGroups = orderedTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer<IXunitTestMethod>.Instance);
        var methodTasks = methodGroups.Select(m =>
        {
            var testMethod = m.Key;
            var testCases = m.ToArray();

            if (exception is not null)
            {
                return FailTestMethod(
                    ctxt,
                    testMethod,
                    testCases,
                    exception).AsTask();
            }

            return new ParallelTestMethodRunner(_parallelTestExecutionContext)
                .Run(
                    testMethod,
                    testCases,
                    ctxt.ExplicitOption,
                    ctxt.MessageBus,
                    ctxt.Aggregator.Clone(),
                    ctxt.CancellationTokenSource,
                    constructorArguments,
                    ctxt.ParallelMode,
                    ctxt.Scheduler,
                    ctxt.ClassFixtureMappings)
                .AsTask();
        });
        
        var methodSummaries = await Task.WhenAll(methodTasks).ConfigureAwait(false);
        foreach (var methodSummary in methodSummaries)
        {
            summary.Aggregate(methodSummary);
        }

        return summary;
    }
}
