using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public sealed class ParallelTestClassRunner : XunitTestClassRunner
{
    public ParallelTestClassRunner(ITestClass testClass,
        IReflectionTypeInfo @class,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ITestCaseOrderer testCaseOrderer,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        IDictionary<Type, object> collectionFixtureMappings)
        : base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
    {
    }

    protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod,
        IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases,
        object[] constructorArguments)
    {
        var inst = new ParallelTestMethodRunner(testMethod, Class, method, testCases, DiagnosticMessageSink,
            MessageBus,
            new ExceptionAggregator(Aggregator),
            CancellationTokenSource, constructorArguments);
         var result = inst.RunAsync();
         return result;
    }

    protected override async Task<RunSummary> RunTestMethodsAsync()
    {
        var disableParallelizationAttribute = TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any();

        var disableParallelizationOnCustomCollection = TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any()
                                                       && !TestClass.Class.GetCustomAttributes(typeof(EnableParallelizationAttribute)).Any();

        var disableParallelization = disableParallelizationAttribute || disableParallelizationOnCustomCollection;

        if (disableParallelization)
            return await base.RunTestMethodsAsync().ConfigureAwait(false);

        var summary = new RunSummary();
        IEnumerable<IXunitTestCase> orderedTestCases;
        try
        {
            orderedTestCases = TestCaseOrderer.OrderTestCases(TestCases);
        }
        catch (Exception ex)
        {
            var innerEx = Unwrap(ex);
            DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Test case orderer '{TestCaseOrderer.GetType().FullName}' threw '{innerEx.GetType().FullName}' during ordering: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}"));
            orderedTestCases = TestCases.ToList();
        }

        var constructorArguments = CreateTestClassConstructorArguments();
        var methodGroups = orderedTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance);
        var methodTasks = methodGroups.Select(m => RunTestMethodAsync(m.Key, (IReflectionMethodInfo)m.Key.Method, m, constructorArguments));
        var methodSummaries = await Task.WhenAll(methodTasks).ConfigureAwait(false);

        foreach (var methodSummary in methodSummaries)
        {
            summary.Aggregate(methodSummary);
        }

        return summary;
    }

    private static Exception Unwrap(Exception ex)
    {
        while (true)
        {
            if (ex is not TargetInvocationException tiex || tiex.InnerException == null)
                return ex;

            ex = tiex.InnerException;
        }
    }
}