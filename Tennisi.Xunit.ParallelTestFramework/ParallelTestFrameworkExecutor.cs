using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public class ParallelTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    private readonly AssemblyName _assemblyName;
    public ParallelTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider,
        IMessageSink diagnosticMessageSink)
        : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
        _assemblyName = assemblyName;
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "By external requirement")]
    protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        ParallelSettings.RefineParallelSetting(_assemblyName, executionOptions);
        using var assemblyRunner = new ParallelTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink,
            executionMessageSink, executionOptions);
        await assemblyRunner.RunAsync();
    }
}