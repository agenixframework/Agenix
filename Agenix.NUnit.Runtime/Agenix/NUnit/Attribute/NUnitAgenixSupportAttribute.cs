using Agenix.Core;
using Agenix.Core.Annotations;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ITestAction = NUnit.Framework.ITestAction;
using TestContext = Agenix.Core.TestContext;

namespace Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;

/// An attribute designed to provide custom test actions for NUnit test methods, classes, interfaces, or assemblies.
/// This attribute enables specific actions to be executed before and after test execution, facilitating integration
/// with Agenix test contexts and enhanced configuration capabilities tailored to test scenarios.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class |
                AttributeTargets.Interface | AttributeTargets.Assembly,
    AllowMultiple = true)]
public class NUnitAgenixSupportAttribute : System.Attribute, ITestAction
{
    [ThreadStatic] private static ITestCaseRunner? _delegate;

    /// Executes actions before the test execution begins.
    /// <param name="test">
    ///     An instance of the test about to be executed, providing context and details of the test case or
    ///     suite.
    /// </param>
    public void BeforeTest(ITest test)
    {
        var agenixInstance = AgenixInstanceManager.GetOrDefault();

        if (test.IsSuite)
        {
            agenixInstance.AgenixContext.ParseConfiguration(test.Fixture);
            agenixInstance.BeforeSuite(test.Name);
            return;
        }

        var context = agenixInstance.AgenixContext.CreateTestContext();
        _delegate = CreateTestRunner(test.Name, test.FullName, test.Fixture?.GetType()!, context);

        AgenixAnnotations.InjectAll(test.Fixture, agenixInstance, context);
        AgenixAnnotations.InjectTestRunner(test.Fixture, _delegate);

        _delegate.Start();
    }

    /// Performs actions after the test execution is completed.
    /// <param name="test">The test instance that has been executed, providing context and details of the test case or suite.</param>
    public void AfterTest(ITest test)
    {
        if (test.IsSuite)
        {
            AgenixInstanceManager.GetOrDefault().AfterSuite(test.Name);
            return;
        }

        _delegate?.Stop();
    }

    public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;

    /// Creates an instance of a test runner for executing test cases with the provided details.
    /// <param name="testName">The name of the test case to be executed.</param>
    /// <param name="packageName">The package name associated with the test class.</param>
    /// <param name="testClass">The type of the test class containing the test case.</param>
    /// <param name="context">The test context providing necessary configurations and state.</param>
    /// <return>An instance of ITestCaseRunner configured with the specified test case details.</return>
    private static ITestCaseRunner CreateTestRunner(string testName, string packageName, Type testClass,
        TestContext context)
    {
        var testCaseRunner = TestCaseRunnerFactory.CreateRunner(new DefaultTestCase(), context);
        testCaseRunner.SetTestClass(testClass);
        testCaseRunner.SetName(testName);
        testCaseRunner.SetPackageName(packageName);

        return testCaseRunner;
    }
}