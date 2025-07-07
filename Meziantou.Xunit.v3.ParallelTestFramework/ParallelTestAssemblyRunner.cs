using Xunit.Sdk;
using Xunit.v3;

namespace Meziantou.Xunit.v3;

public class ParallelTestAssemblyRunner : XunitTestAssemblyRunnerBase<XunitTestAssemblyRunnerContext, IXunitTestAssembly, IXunitTestCollection, IXunitTestCase>
{
    public static ParallelTestAssemblyRunner Instance { get; } = new();

    protected override async ValueTask<RunSummary> RunTestCollection(XunitTestAssemblyRunnerContext ctxt, IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases)
    {
        if (ctxt is null)
            throw new ArgumentNullException(nameof(ctxt));
        
        return await ParallelTestCollectionRunner.Instance.Run(testCollection, testCases, ctxt.ExplicitOption, ctxt.MessageBus, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource, ctxt.AssemblyFixtureMappings).ConfigureAwait(false);
    }

    public async ValueTask<RunSummary> Run(
        IXunitTestAssembly testAssembly,
        IReadOnlyCollection<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions,
        CancellationToken cancellationToken)
    {
        if (testAssembly is null)
            throw new ArgumentNullException(nameof(testAssembly));
        if (testCases is null)
            throw new ArgumentNullException(nameof(testCases));
        if (executionMessageSink is null)
            throw new ArgumentNullException(nameof(executionMessageSink));
        if (executionOptions is null)
            throw new ArgumentNullException(nameof(executionOptions));
        
        var ctxt = new XunitTestAssemblyRunnerContext(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
        await using (ctxt.ConfigureAwait(false))
        {
            await ctxt.InitializeAsync().ConfigureAwait(false);

            return await Run(ctxt).ConfigureAwait(false);
        }
    }
}
