using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public class ParallelTestFrameworkDiscoverer: XunitTestFrameworkDiscoverer
{
    private IAssemblyInfo _assemblyInfo;
    public ParallelTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, IXunitTestCollectionFactory collectionFactory = null) : base(assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory)
    {
        _assemblyInfo = assemblyInfo;
    }

    protected override bool FindTestsForMethod(ITestMethod testMethod, bool includeSourceInformation, IMessageBus messageBus,
        ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        ParallelSettings.RefineParallelSetting(_assemblyInfo.Name, discoveryOptions, "xunit.discovery.PreEnumerateTheories", true);
        return base.FindTestsForMethod(testMethod, includeSourceInformation, messageBus, discoveryOptions);
    }

    protected override bool FindTestsForType(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus,
        ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        ParallelSettings.RefineParallelSetting(_assemblyInfo.Name, discoveryOptions, "xunit.discovery.PreEnumerateTheories", true);
        return base.FindTestsForType(testClass, includeSourceInformation, messageBus, discoveryOptions);
    }
}