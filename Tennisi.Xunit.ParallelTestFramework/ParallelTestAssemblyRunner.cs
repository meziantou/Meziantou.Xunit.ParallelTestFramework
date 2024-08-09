using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public sealed class ParallelTestAssemblyRunner : XunitTestAssemblyRunner
{
    public ParallelTestAssemblyRunner(ITestAssembly testAssembly,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
        : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
    {
    }
    
    private static readonly ConcurrentDictionary<string, bool> AttributeCache = new ConcurrentDictionary<string, bool>();

    private bool ShouldForceParallelize(ITestCollection testCollection)
    {
        var assemblyName = testCollection.TestAssembly.Assembly.Name;

        return AttributeCache.GetOrAdd(assemblyName, name =>
        {
            var assembly = Assembly.Load(new AssemblyName(name));
            var attributeType = typeof(ParallelizeTestCollectionsAttribute);
            var attributes = assembly.GetCustomAttributes(attributeType, false);
            return attributes.Any();
        });
    }

    protected override async Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        CancellationTokenSource cancellationTokenSource)
    {
        var shouldParallelize = ShouldForceParallelize(testCollection);

        if (!shouldParallelize)
            return await (new ParallelTestTestCollectionRunner(
                testCollection,
                testCases,
                DiagnosticMessageSink,
                messageBus,
                TestCaseOrderer,
                new ExceptionAggregator(Aggregator),
                cancellationTokenSource).RunAsync());
        
        var testCasesByCollection = TestCases.GroupBy(tc => tc.TestMethod.TestClass.TestCollection).ToList();
            
        var tasks = testCasesByCollection.Select(testCaseGroup =>
        {
            var testCollectionX = testCaseGroup.Key;
            return Task.Run(() => (new ParallelTestTestCollectionRunner(
                testCollectionX,
                testCaseGroup.ToList(),
                DiagnosticMessageSink,
                messageBus,
                TestCaseOrderer,
                new ExceptionAggregator(Aggregator),
                cancellationTokenSource).RunAsync()));
        }).ToArray();

        var summaries = await Task.WhenAll(tasks);
        var summary = new RunSummary();
        foreach (var runSummary in summaries)
        {
            summary.Aggregate(runSummary);
        }

        return summary;
    }
}