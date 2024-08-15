using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit
{
    public sealed class ParallelTestAssemblyRunner : XunitTestAssemblyRunner
    {
        private static readonly ConcurrentDictionary<string, bool> AttributeCache = new ConcurrentDictionary<string, bool>();

        public ParallelTestAssemblyRunner(ITestAssembly testAssembly,
                                          IEnumerable<IXunitTestCase> testCases,
                                          IMessageSink diagnosticMessageSink,
                                          IMessageSink executionMessageSink,
                                          ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
        {
        }

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
            {
                return await RunTestCollectionRunnerAsync(testCollection, testCases, messageBus, cancellationTokenSource);
            }

            var testCasesByCollection = testCases.GroupBy(tc => tc.TestMethod.TestClass.TestCollection).ToList();

            var tasks = testCasesByCollection.Select(testCaseGroup =>
            {
                var testCollectionX = testCaseGroup.Key;
                return Task.Run(() => RunTestCollectionRunnerAsync(testCollectionX, testCaseGroup.ToList(), messageBus, cancellationTokenSource));
            }).ToArray();

            var summaries = await Task.WhenAll(tasks);
            var summary = new RunSummary();
            foreach (var runSummary in summaries)
            {
                summary.Aggregate(runSummary);
            }

            return summary;
        }

        private async Task<RunSummary> RunTestCollectionRunnerAsync(ITestCollection testCollection,
                                                                    IEnumerable<IXunitTestCase> testCases,
                                                                    IMessageBus messageBus,
                                                                    CancellationTokenSource cancellationTokenSource)
        {
            var runner = new ParallelTestTestCollectionRunner(
                testCollection,
                testCases,
                DiagnosticMessageSink,
                messageBus,
                TestCaseOrderer,
                new ExceptionAggregator(Aggregator),
                cancellationTokenSource);

            // Handle SynchronizationContext
            if (SynchronizationContext.Current == null)
            {
                return await Task.Run(() => runner.RunAsync(), cancellationTokenSource.Token).ConfigureAwait(false);
            }

            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return await Task.Factory.StartNew(() => runner.RunAsync(), cancellationTokenSource.Token, 
                                               TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler)
                                     .Unwrap()
                                     .ConfigureAwait(false);
        }
    }
}
