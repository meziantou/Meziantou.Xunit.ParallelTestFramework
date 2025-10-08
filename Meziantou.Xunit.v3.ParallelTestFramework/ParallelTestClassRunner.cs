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
        FixtureMappingManager collectionFixtureMappings)
    {
        var ctxt = new XunitTestClassRunnerContext(testClass, testCases, explicitOption, messageBus, DefaultTestCaseOrderer.Instance, aggregator, cancellationTokenSource, collectionFixtureMappings);
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
            orderedTestCases = OrderTestCases(ctxt);
            constructorArguments = await CreateTestClassConstructorArguments(ctxt).ConfigureAwait(false);
            exception = ctxt.Aggregator.ToException();
            ctxt.Aggregator.Clear();
        }
        else
        {
            orderedTestCases = ctxt.TestCases;
            constructorArguments = Array.Empty<object?>();
        }

        var methodGroups = orderedTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance);
        var methodTasks = methodGroups.Select(m =>
            exception switch
            {
                null => RunTestMethod(ctxt, m.Key as IXunitTestMethod, m.ToArray(), constructorArguments).AsTask(),
                not null => FailTestMethod(ctxt, m.Key as IXunitTestMethod, m.ToArray(), constructorArguments, exception).AsTask(),
            });

        var methodSummaries = await Task.WhenAll(methodTasks).ConfigureAwait(false);
        foreach (var methodSummary in methodSummaries)
        {
            summary.Aggregate(methodSummary);
        }

        return summary;
    }

    protected override async ValueTask<RunSummary> RunTestMethod(XunitTestClassRunnerContext ctxt, IXunitTestMethod? testMethod, IReadOnlyCollection<IXunitTestCase> testCases,
        object?[] constructorArguments)
    {
        if (ctxt is null)
            throw new ArgumentNullException(nameof(ctxt));

        return await new ParallelTestMethodRunner(_parallelTestExecutionContext).Run(testMethod ?? throw new ArgumentNullException(nameof(testMethod)), testCases, ctxt.ExplicitOption, ctxt.MessageBus, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource, constructorArguments).ConfigureAwait(false);
    }
}
