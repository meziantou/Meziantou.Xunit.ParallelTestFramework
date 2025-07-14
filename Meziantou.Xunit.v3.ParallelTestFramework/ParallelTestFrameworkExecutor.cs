using Xunit.Sdk;
using Xunit.v3;

#pragma warning disable IDE1006 // Naming Styles
namespace Meziantou.Xunit.v3;
#pragma warning restore IDE1006 // Naming Styles

public class ParallelTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    public ParallelTestFrameworkExecutor(XunitTestAssembly xunitTestAssembly) : base(xunitTestAssembly)
    {
    }

    public override async ValueTask RunTestCases(IReadOnlyCollection<IXunitTestCase> testCases, IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions, CancellationToken cancellationToken)
    {
        await ParallelTestAssemblyRunner.Instance.Run(TestAssembly, testCases, executionMessageSink, executionOptions, cancellationToken).ConfigureAwait(false);
    }
}
