using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public class DiagnosticTestFramework : XunitTestFramework
{
    public ITestFrameworkExecutor CreateExecutorShim(AssemblyName assemblyName)
        => CreateExecutor(assemblyName);
    
    public DiagnosticTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
        #if DEBUG
        messageSink.OnMessage(new DiagnosticMessage("Using CustomTestFramework"));
        #endif
    }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        => new DiagnosticTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
}
