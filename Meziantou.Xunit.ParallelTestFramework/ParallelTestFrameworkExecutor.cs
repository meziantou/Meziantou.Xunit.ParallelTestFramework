using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Meziantou.Xunit;

public class ParallelTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    public ParallelTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink) : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0155:Do not use async void methods")]
    protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
    {
        using var assemblyRunner = new ParallelTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);
        await assemblyRunner.RunAsync().ConfigureAwait(false);
    }
}
