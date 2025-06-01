#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using Agenix.Api;
using Agenix.Core;
using Agenix.Core.Annotations;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ITestAction = NUnit.Framework.ITestAction;
using TestContext = Agenix.Api.Context.TestContext;

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
        if (test.IsSuite)
        {
            AgenixInstanceManager.Reset();
            AgenixInstanceManager.GetOrDefault().AgenixContext.ParseConfiguration(test.Fixture);
            AgenixInstanceManager.GetOrDefault().BeforeSuite(test.Name);
            return;
        }

        var context = AgenixInstanceManager.GetOrDefault().AgenixContext.CreateTestContext();
        _delegate = CreateTestRunner(test.Name, test.FullName, test.Fixture?.GetType()!, context);

        AgenixAnnotations.InjectAll(test.Fixture, AgenixInstanceManager.GetOrDefault(), context);
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
        testCaseRunner.SetNamespaceName(packageName);

        return testCaseRunner;
    }
}
