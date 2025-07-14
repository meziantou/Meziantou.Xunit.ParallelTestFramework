using Xunit.Sdk;
using Xunit.v3;

namespace Meziantou.Xunit.v3;

public class ParallelTestCollectionRunner : XunitTestCollectionRunnerBase<XunitTestCollectionRunnerContext, IXunitTestCollection, IXunitTestClass, IXunitTestCase>
{
    public static ParallelTestCollectionRunner Instance { get; } = new();
    
    public async ValueTask<RunSummary> Run(
        IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases,
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        FixtureMappingManager assemblyFixtureMappings)
    {
        var ctxt = new XunitTestCollectionRunnerContext(testCollection, testCases, explicitOption, messageBus, DefaultTestCaseOrderer.Instance, aggregator, cancellationTokenSource, assemblyFixtureMappings);
        await using (ctxt.ConfigureAwait(false))
        {
            await ctxt.InitializeAsync().ConfigureAwait(false);

            return await Run(ctxt).ConfigureAwait(false);
        }
    }
    
    protected override async ValueTask<RunSummary> RunTestClass(XunitTestCollectionRunnerContext ctxt, IXunitTestClass? testClass,
        IReadOnlyCollection<IXunitTestCase> testCases)
    {
        if (ctxt is null)
            throw new ArgumentNullException(nameof(ctxt));
        
        return await ParallelTestClassRunner.Instance.Run(testClass ?? throw new ArgumentNullException(nameof(testClass)), testCases, ctxt.ExplicitOption, ctxt.MessageBus, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource, ctxt.CollectionFixtureMappings).ConfigureAwait(false);
    }

    protected override async ValueTask<RunSummary> RunTestClasses(XunitTestCollectionRunnerContext ctxt,
        Exception? exception)
    {
        if (ctxt is null)
            throw new ArgumentNullException(nameof(ctxt));
        
        if (ctxt.TestCollection.CollectionDefinition != null)
        {
            var enableParallelizationAttribute = ctxt.TestCollection.CollectionDefinition
                .GetCustomAttributes(typeof(EnableParallelizationAttribute), inherit: true).Any();
            if (enableParallelizationAttribute)
            {
                var summary = new RunSummary();

                var classTasks = ctxt.TestCases.GroupBy(tc => tc.TestMethod.TestClass, TestClassComparer.Instance)
                    .Select(tc => RunTestClass(ctxt, tc.Key as IXunitTestClass, tc.ToArray()).AsTask());

                var classSummaries = await Task.WhenAll(classTasks)
#if !NETSTANDARD
                    .WaitAsync(ctxt.CancellationTokenSource.Token)
#endif
                    .ConfigureAwait(false);
                
                foreach (var classSummary in classSummaries)
                {
                    summary.Aggregate(classSummary);
                }

                return summary;
            }
        }

        // Fall back to default behavior
        return await base.RunTestClasses(ctxt, exception).ConfigureAwait(false);
    }
}
