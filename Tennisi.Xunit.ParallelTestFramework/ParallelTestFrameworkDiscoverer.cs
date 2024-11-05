using System.Globalization;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public class ParallelTestFrameworkDiscoverer: XunitTestFrameworkDiscoverer
{
    private readonly IAssemblyInfo _assemblyInfo;

    public ParallelTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, IXunitTestCollectionFactory collectionFactory = null) : base(assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory)
    {
        _assemblyInfo = assemblyInfo;
    }

    protected override bool FindTestsForMethod(ITestMethod testMethod, bool includeSourceInformation,
        IMessageBus messageBus,
        ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        ParallelSettings.RefineParallelSetting(_assemblyInfo.Name, discoveryOptions,
            "xunit.discovery.PreEnumerateTheories", true);
        
        if (!testMethod.ShouldUseClassRetry(out var retryCount))
            return base.FindTestsForMethod(testMethod, includeSourceInformation, messageBus, discoveryOptions);

        return FindTestsForMethod2(testMethod, includeSourceInformation, messageBus, discoveryOptions, retryCount);
    }
    
    protected virtual bool FindTestsForMethod2(ITestMethod testMethod, bool includeSourceInformation,
        IMessageBus messageBus,
        ITestFrameworkDiscoveryOptions discoveryOptions, int retryCount)
    {
        var factAttributesRaw = testMethod.Method.GetCustomAttributes(typeof(FactAttribute));
        var factAttributes = (factAttributesRaw as List<IAttributeInfo>) ?? factAttributesRaw.ToList();
        if (factAttributes.Count > 1)
        {
            var message = string.Format(CultureInfo.CurrentCulture,
                "Test method '{0}.{1}' has multiple [Fact]-derived attributes", testMethod.TestClass.Class.Name,
                testMethod.Method.Name);
            using var testCase = new ExecutionErrorTestCase(DiagnosticMessageSink, TestMethodDisplay.ClassAndMethod,
                TestMethodDisplayOptions.None, testMethod, message);
                return ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus);
        }

        var factAttribute = factAttributes.FirstOrDefault();
        if (factAttribute == null)
            return true;

        var factAttributeType = (factAttribute as IReflectionAttributeInfo)?.Attribute.GetType();

        Type? discovererType = null;
        if (factAttributeType == null || !DiscovererTypeCache.TryGetValue(factAttributeType, out discovererType))
        {
            var testCaseDiscovererAttribute =
                factAttribute.GetCustomAttributes(typeof(XunitTestCaseDiscovererAttribute)).FirstOrDefault();
            if (testCaseDiscovererAttribute != null)
            {
                var args = testCaseDiscovererAttribute.GetConstructorArguments().Cast<string>().ToList();
                discovererType = SerializationHelper.GetType(args[1], args[0]);
            }

            if (factAttributeType != null)
                DiscovererTypeCache[factAttributeType] = discovererType;
        }

        if (discovererType == null)
            return true;

        var discoverer = GetDiscoverer(discovererType);
        if (discoverer == null)
            return true;

        foreach (var testCase in discoverer.Discover(discoveryOptions, testMethod, factAttribute))
        {
            var retryTestCase = new RetryTestCase(testCase, retryCount);
            if (!ReportDiscoveredTestCase(retryTestCase, includeSourceInformation, messageBus))
                return false;
        }

        return true;
    }

    protected override bool FindTestsForType(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus,
        ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        ParallelSettings.RefineParallelSetting(_assemblyInfo.Name, discoveryOptions, "xunit.discovery.PreEnumerateTheories", true);
        return base.FindTestsForType(testClass, includeSourceInformation, messageBus, discoveryOptions);
    }
}