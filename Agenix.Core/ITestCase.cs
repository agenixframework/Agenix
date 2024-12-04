using System;
using System.Collections.Generic;
using Agenix.Core;
using Agenix.Core.Common;
using Agenix.Core.Container;

/**
 * Test case executing a list of {@link TestAction} in sequence.
 */
public interface ITestCase : ITestActionContainer, INamed, IDescribed
{
    /**
     * Starts the test case.
     */
    void Start(TestContext context);

    /**
     * Sequence of test actions before the test case.
     */
    void BeforeTest(TestContext context)
    {
    }

    /**
     * Sequence of test actions after test case.
     */
    void AfterTest(TestContext context)
    {
    }

    /**
     * Executes a single test action with given test context.
     */
    void ExecuteAction(ITestAction action, TestContext context);

    /**
     * Method that will be executed in any case of test case result (success, error). Usually used for clean up tasks.
     */
    void Finish(TestContext context);

    /**
     * Get the test case meta information.
     */
    TestCaseMetaInfo GetMetaInfo();

    /**
     * Gets the value of the testClass property.
     */
    Type GetTestClass();

    /**
     * Set the test class type.
     */
    void SetTestClass(Type type);

    /**
     * Get the package name.
     */
    string GetPackageName();

    /**
     * Set the package name.
     */
    void SetPackageName(string packageName);

    /**
     * Sets the test result from outside.
     */
    void SetTestResult(TestResult testResult);

    /**
     * Retrieve test result.
     */
    TestResult GetTestResult();

    /**
     * Flags whether test case is incremental.
     */
    bool IsIncremental();

    /**
     * Sets the test runner flag.
     */
    void SetIncremental(bool incremental);

    /**
     * Gets the variables for this test case.
     */
    Dictionary<string, object> GetVariableDefinitions();

    /**
     * Adds action to finally action chain.
     */
    void AddFinalAction(ITestAction action)
    {
    }

    void AddFinalAction(ITestActionBuilder<ITestAction> builder);

    /**
     * Provides access to the raw test action builders used to construct the list of actions in this test case.
     */
    List<ITestActionBuilder<ITestAction>> GetActionBuilders();

    /**
     * Immediately fails the {@link TestCase}, attaching the given {@link Throwable} exception to the {@link TestResult}.
     */
    void Fail(Exception throwable);
}