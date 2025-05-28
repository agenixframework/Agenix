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

using Agenix.Api.Common;
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Core;

namespace Agenix.Api;

/**
* Test case executing a list of {@link TestAction} in sequence.
*/
public interface ITestCase : ITestActionContainer, INamed, IDescribed
{
    /// Starts the test case execution using the given context.
    /// <param name="context">The context in which the test case is executed.</param>
    /// /
    void Start(TestContext context);

    /// Executes actions that must be performed before the test case execution begins.
    /// <param name="context">The context in which the test case is executed.</param>
    void BeforeTest(TestContext context)
    {
    }

    /// Executes a sequence of test actions after a test case.
    /// <param name="context">The context in which the post-test actions are executed.</param>
    void AfterTest(TestContext context)
    {
    }

    /// Executes a provided test action within a specified test context.
    /// <param name="action">The test action to be executed.</param>
    /// <param name="context">The context in which the test action is executed.</param>
    void ExecuteAction(ITestAction action, TestContext context);

    /// Executes cleanup tasks after the test case, regardless of the test result.
    /// <param name="context">The context in which the test case is executed.</param>
    void Finish(TestContext context);

    /// Retrieves the meta-information associated with the test case.
    /// <return>The meta-information of the test case as a <see cref="TestCaseMetaInfo" /> instance.</return>
    TestCaseMetaInfo GetMetaInfo();

    /// Retrieves the Type of the test class associated with the test case.
    /// <returns>The Type of the test class.</returns>
    Type GetTestClass();

    /// Sets the type of the test class to be executed.
    /// <param name="type">The Type representing the test class.</param>
    void SetTestClass(Type type);

    /// Retrieves the namespace name associated with the test case.
    /// <return>The namespace name as a string.</return>
    string GetNamespaceName();

    /// Sets the namespace name for the test case.
    /// <param name="packageName">The name of the namespace to set.</param>
    void SetNamespaceName(string packageName);

    /// Sets the test result manually from an external source.
    /// <param name="testResult">The result of the test encapsulated in a TestResult object.</param>
    void SetTestResult(TestResult testResult);

    /// Retrieves the result of the test execution.
    /// <returns>The result of the test encapsulated in a TestResult object.</returns>
    TestResult GetTestResult();

    /// Indicates whether the test case should be executed in an incremental fashion, where each test step is executed sequentially and builds upon the previous one.
    /// <returns>True if the test case is incremental; otherwise, false.</returns>
    bool IsIncremental();

    /// Sets the incremental execution flag for the test case.
    /// <param name="incremental">Determines whether the test case should execute incrementally.</param>
    void SetIncremental(bool incremental);

    /// Retrieves a dictionary of variable definitions associated with the test case.
    /// <returns>A dictionary containing the variable names as keys and their corresponding values.</returns>
    Dictionary<string, object> GetVariableDefinitions();

    /// Adds a specified test action to the chain of actions to be executed at the end of the test.
    /// <param name="action">The test action to be added to the final execution chain.</param>
    void AddFinalAction(ITestAction action)
    {
    }

    /// Adds a final action to the test case.
    /// <param name="builder">The builder used to construct the final action to be added.</param>
    void AddFinalAction(ITestActionBuilder<ITestAction> builder);

    /// Retrieves the list of test action builders used to define the sequence of actions in the test case.
    /// <return>Returns a list of action builders that construct the test case actions.</return>
    List<ITestActionBuilder<ITestAction>> GetActionBuilders();

    /// Immediately fails the test case, attaching the given exception to the test result.
    /// <param name="throwable">The exception to attach to the test result.</param>
    void Fail(Exception throwable);
}
