using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public class ParallelTestFramework : XunitTestFramework
{
    public ITestFrameworkExecutor CreateExecutorShim(AssemblyName assemblyName)
        => CreateExecutor(assemblyName);

    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract", Justification = "By Author")]
    protected ParallelTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
        #if DEBUG
        messageSink?.OnMessage(new DiagnosticMessage("Using CustomTestFramework"));
        #endif
    }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        => new ParallelTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
}
