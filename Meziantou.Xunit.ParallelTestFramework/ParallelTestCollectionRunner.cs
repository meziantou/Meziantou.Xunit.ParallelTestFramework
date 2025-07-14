using Xunit.Abstractions;
using Xunit.Sdk;

namespace Meziantou.Xunit;

public class ParallelTestCollectionRunner : XunitTestCollectionRunner
{
    public ParallelTestCollectionRunner(ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
    {
    }

    protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
        => new ParallelTestClassRunner(testClass, @class, testCases, DiagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings).RunAsync();


    protected override async Task<RunSummary> RunTestClassesAsync()
    {

        if (TestCollection.CollectionDefinition is not null)
        {
            var enableParallelizationAttribute = TestCollection.CollectionDefinition.GetCustomAttributes(typeof(EnableParallelizationAttribute)).Any();
            if (enableParallelizationAttribute)
            {
                var summary = new RunSummary();

                var classTasks = TestCases.GroupBy(tc => tc.TestMethod.TestClass, TestClassComparer.Instance)
                    .Select(tc => RunTestClassAsync(tc.Key, (IReflectionTypeInfo)tc.Key.Class, tc));

                var classSummaries = await Task.WhenAll(classTasks)
#if !NETSTANDARD
                    .WaitAsync(CancellationTokenSource.Token)
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
        return await base.RunTestClassesAsync().ConfigureAwait(false);
    }
}
