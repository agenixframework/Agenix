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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core.Container;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core;

/// <summary>
///     Default implementation of a test case that includes actions and supports test grouping, test parameters, and
///     completes test results with duration.
/// </summary>
public class DefaultTestCase : AbstractActionContainer, ITestCase, ITestGroupAware, ITestParameterAware
{
    /// <summary>
    ///     Sets the test result to failed due to the provided exception and completes the test result with the duration.
    ///     This method marks the test as failed and also tracks the test duration.
    /// </summary>
    /// <param name="throwable">The exception that caused the test to fail.</param>
    public void Fail(Exception throwable)
    {
        SetTestResult(TestResult.Failed(Name, TestClass.Name, throwable));
        CompleteTestResultWithDuration();
    }

    /// <summary>
    ///     Executes a sequence of actions defined to run after the test is completed.
    ///     This method evaluates each action in the context's AfterTest list and executes them if they meet the criteria
    ///     specified by the ShouldExecute method.
    ///     Any exceptions encountered during the execution of these actions are logged as warnings.
    /// </summary>
    /// <param name="context">The context that provides information and actions related to the test that has been executed.</param>
    public virtual void AfterTest(TestContext context)
    {
        foreach (var sequenceAfterTest in context.AfterTest)
        {
            try
            {
                if (sequenceAfterTest.ShouldExecute(Name, PackageName, Groups))
                {
                    sequenceAfterTest.Execute(context);
                }
            }
            catch (Exception e)
            {
                Log.LogWarning(e, "After test failed with errors");
            }
        }
    }

    /// <summary>
    ///     Executes the provided test action within the given test context.
    ///     If the context contains exceptions, the first exception is thrown before any action execution.
    /// </summary>
    /// <param name="action">The test action to be executed.</param>
    /// <param name="context">The context in which the test action is executed, containing state and listener information.</param>
    /// <exception cref="TestCaseFailedException">Thrown when an exception occurs during the test action execution.</exception>
    public new void ExecuteAction(ITestAction action, TestContext context)
    {
        if (context.HasExceptions())
        {
            var exception = context.GetExceptions()[0];
            context.GetExceptions().RemoveAt(0);
            throw exception;
        }

        try
        {
            SetActiveAction(action);

            if (!action.IsDisabled(context))
            {
                context.TestActionListeners.OnTestActionStart(this, action);

                action.Execute(context);
                context.TestActionListeners.OnTestActionFinish(this, action);
            }
            else
            {
                context.TestActionListeners.OnTestActionSkipped(this, action);
            }
        }
        catch (Exception e)
        {
            TestResult = GetTestResultInstanceProvider(context).CreateFailed(this, e);
            throw new TestCaseFailedException(e);
        }
        finally
        {
            SetExecutedAction(action);
        }
    }

    /// <summary>
    ///     Finalizes the test case execution, updates the test result based on the context,
    ///     handles any exceptions that occurred, and completes the test result with the duration.
    /// </summary>
    /// <param name="context">The context of the test, containing necessary information for execution and results tracking.</param>
    public void Finish(TestContext context)
    {
        if (GetMetaInfo().GetStatus().Equals(TestCaseMetaInfo.Status.DISABLED))
        {
            return;
        }

        try
        {
            AgenixSystemException contextException = null;
            if (TestResult == null)
            {
                if (context.HasExceptions())
                {
                    var exception = context.GetExceptions()[0];
                    context.GetExceptions().RemoveAt(0);
                    contextException = exception;
                    TestResult = GetTestResultInstanceProvider(context).CreateFailed(this, contextException);
                }
                else
                {
                    TestResult = GetTestResultInstanceProvider(context).CreateSuccess(this);
                }
            }

            if (context.IsSuccess(TestResult))
            {
                WaitUtils.WaitForCompletion(this, context, Timeout).GetAwaiter().GetResult();
            }

            context.TestListeners.OnTestFinish(this);
            ExecuteFinalActions(context);

            if (contextException != null)
            {
                throw new TestCaseFailedException(contextException);
            }
        }
        catch (TestCaseFailedException)
        {
            throw;
        }
        catch (Exception e)
        {
            TestResult = GetTestResultInstanceProvider(context).CreateFailed(this, e);
            throw new TestCaseFailedException(e);
        }
        finally
        {
            if (TestResult != null)
            {
                if (TestResult.IsSuccess())
                {
                    context.TestListeners.OnTestSuccess(this);
                }
                else
                {
                    context.TestListeners.OnTestFailure(this, TestResult.Cause);
                }
            }

            AfterTest(context);

            CompleteTestResultWithDuration();
        }
    }

    /// <summary>
    ///     Executes the sequence of actions defined to run before the test.
    ///     This method initiates the timer and processes each action in the context's BeforeTest list,
    ///     executing them if they meet the criteria specified by the ShouldExecute method.
    ///     Any exceptions that occur during the execution of these actions are caught and wrapped in a CoreSystemException.
    /// </summary>
    /// <param name="context">The context containing information and actions pertinent to the test being executed.</param>
    public virtual void BeforeTest(TestContext context)
    {
        RestartTimer();

        foreach (var sequenceBeforeTest in context.BeforeTest)
        {
            try
            {
                if (sequenceBeforeTest.ShouldExecute(Name, PackageName, Groups))
                {
                    sequenceBeforeTest.Execute(context);
                }
            }
            catch (Exception e)
            {
                throw new AgenixSystemException("Before test failed with errors", e);
            }
        }
    }

    /// <summary>
    ///     Starts the test case execution.
    ///     This method triggers the initialization of the test case, logs debug information,
    ///     initializes test parameters and variables, and calls the BeforeTest method.
    ///     If an exception occurs during initialization, the test result is set to failed
    ///     and a TestCaseFailedException is thrown.
    /// </summary>
    /// <param name="context">The test context that holds information about the test case execution.</param>
    public virtual void Start(TestContext context)
    {
        context.TestListeners.OnTestStart(this);

        try
        {
            Log.LogDebug("Initializing test case");

            DebugVariables("Global", context);
            InitializeTestParameters(_parameters, context);
            InitializeTestVariables(VariableDefinitions, context);
            DebugVariables("Test", context);

            BeforeTest(context);
        }
        catch (Exception e)
        {
            TestResult = GetTestResultInstanceProvider(context).CreateFailed(this, e);
            throw new TestCaseFailedException(e);
        }
    }

    /// <summary>
    ///     Sets the parameters for the test case.
    ///     Ensures that the number of parameter names matches the number of parameter values.
    ///     If the parameter names and values count do not match, an exception is thrown.
    /// </summary>
    /// <param name="parameterNames">Array containing the names of the parameters.</param>
    /// <param name="parameterValues">Array containing the values of the parameters.</param>
    public void SetParameters(string[] parameterNames, object[] parameterValues)
    {
        if (parameterNames.Length != parameterValues.Length)
        {
            throw new AgenixSystemException(
                $"Invalid test parameter usage - received '{parameterNames.Length}' parameters with '{parameterValues.Length}' values");
        }

        for (var i = 0; i < parameterNames.Length; i++)
        {
            if (parameterValues[i] != null)
            {
                _parameters[parameterNames[i]] = parameterValues[i];
            }
        }
    }

    /// <summary>
    ///     Retrieves the dictionary containing the test parameters.
    /// </summary>
    /// <return>A dictionary where the keys are parameter names and the values are parameter values.</return>
    Dictionary<string, object> ITestParameterAware.GetParameters()
    {
        return _parameters;
    }

    /// <summary>
    ///     Completes the test result by stopping the timer and updating the duration of the test.
    ///     This method gracefully stops the timer and sets the duration of the test result
    ///     based on the elapsed time measured by the timer.
    /// </summary>
    private void CompleteTestResultWithDuration()
    {
        GracefullyStopTimer();

        TestResult?.WithDuration(TimeSpan.FromTicks(_timer.ElapsedTicks * (1000000000L / Stopwatch.Frequency)));
    }

    /// <summary>
    ///     Executes all final actions registered to the test case. These actions will be executed at the end of the test,
    ///     regardless of whether the test passed or failed.
    ///     Also updates the test result if any exception occurred during the execution of the final actions.
    /// </summary>
    /// <param name="context">The test context containing information and services available for the test execution.</param>
    private void ExecuteFinalActions(TestContext context)
    {
        if (_finalActions.Count != 0)
        {
            Log.LogDebug("Entering finally block in test case");

            /* walk through the finally-chain and execute the actions in there */
            foreach (var action in _finalActions.Select(actionBuilder => actionBuilder.Build()))
            {
                if (!action.IsDisabled(context))
                {
                    context.TestActionListeners.OnTestActionStart(this, action);
                    action.Execute(context);
                    context.TestActionListeners.OnTestActionFinish(this, action);
                }
                else
                {
                    context.TestActionListeners.OnTestActionSkipped(this, action);
                }
            }
        }

        if (!TestResult.IsSuccess() || !context.HasExceptions())
        {
            return;
        }

        var exception = context.GetExceptions()[0];
        context.GetExceptions().RemoveAt(0);
        TestResult = GetTestResultInstanceProvider(context).CreateFailed(this, exception);
        throw new TestCaseFailedException(exception);
    }

    /// <summary>
    ///     Initializes the test case variables within the provided TestContext.
    ///     Adds global test variables to the TestContext by resolving variable names and their current values.
    ///     If the variable value is a string, it checks if the value needs to be dynamically resolved.
    /// </summary>
    /// <param name="variableDefinitions">Dictionary containing variable names and their values.</param>
    /// <param name="context">The TestContext where the variables will be set.</param>
    private static void InitializeTestVariables(Dictionary<string, object> variableDefinitions, TestContext context)
    {
        // Build up the global test variables in TestContext by
        // getting the names and the current values of all variables
        foreach (var entry in variableDefinitions)
        {
            var key = entry.Key;
            var value = entry.Value;

            if (value is string stringValue)
                // Check if value is a variable or function and resolve it accordingly
            {
                context.SetVariable(key, context.ReplaceDynamicContentInString(stringValue));
            }
            else
            {
                context.SetVariable(key, value);
            }
        }
    }

    /// <summary>
    ///     Executes the test case actions within the provided TestContext.
    ///     Handles the lifecycle of the test case, including starting and stopping timers,
    ///     and creates the appropriate test result based on the execution outcome.
    /// </summary>
    /// <param name="context">The context in which the test case is executed.</param>
    public override void DoExecute(TestContext context)
    {
        if (!GetMetaInfo().GetStatus().Equals(TestCaseMetaInfo.Status.DISABLED))
        {
            try
            {
                Start(context);

                foreach (var actionBuilder in actions)
                {
                    ExecuteAction(actionBuilder.Build(), context);
                }

                TestResult = GetTestResultInstanceProvider(context).CreateSuccess(this);
            }
            catch (TestCaseFailedException)
            {
                GracefullyStopTimer();
                throw;
            }
            catch (Exception e)
            {
                TestResult = GetTestResultInstanceProvider(context).CreateFailed(this, e);
                throw new TestCaseFailedException(e);
            }
        }
        else
        {
            TestResult = GetTestResultInstanceProvider(context).CreateSkipped(this);
            context.TestListeners.OnTestSkipped(this);
        }
    }

    /// <summary>
    ///     Logs the variables present in the TestContext within the specified scope.
    ///     This method provides detailed debug information about the variables during
    ///     the test execution process.
    /// </summary>
    /// <param name="scope">The scope of the variables to be logged (e.g., "Global" or "Test").</param>
    /// <param name="context">The context containing the variables to be debugged.</param>
    private static void DebugVariables(string scope, TestContext context)
    {
        /* Debug print global variables */
        if (!context.HasVariables() || !Log.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        Log.LogDebug("{Scope} variables:", scope);
        foreach (var entry in context.GetVariables())
        {
            Log.LogDebug("{EntryKey} = {EntryValue}", entry.Key, entry.Value);
        }
    }

    /// <summary>
    ///     Initializes the test parameters within the provided TestContext.
    ///     It sets default variables such as the test name and namespace,
    ///     and adds parameters as variables in the context.
    /// </summary>
    /// <param name="parameters">The dictionary containing parameter names and values to be initialized.</param>
    /// <param name="context">The context in which the test parameters are initialized.</param>
    private void InitializeTestParameters(Dictionary<string, object> parameters, TestContext context)
    {
        // Add default variables for test
        context.SetVariable(AgenixSettings.TestNameVariable(), Name);
        context.SetVariable(AgenixSettings.TestNameSpaceVariable(), PackageName);

        foreach (var paramEntry in parameters)
        {
            Log.LogDebug("Initializing test parameter '{ParamEntryKey}' as variable", paramEntry.Key);
            context.SetVariable(paramEntry.Key, paramEntry.Value);
        }
    }

    /// <summary>
    ///     Retrieves the instance of the default ITestResultInstanceProvider.
    /// </summary>
    /// <returns>The default ITestResultInstanceProvider instance.</returns>
    private ITestResultInstanceProvider GetTestResultInstanceProvider(TestContext context)
    {
        return _defaultTestResultInstanceProvider;
    }

    /// <summary>
    ///     Stops the stopwatch if it is currently running.
    /// </summary>
    private void GracefullyStopTimer()
    {
        if (_timer.IsRunning)
        {
            _timer.Stop();
        }
    }

    /// <summary>
    ///     Restarts the internal stopwatch timer.
    ///     If the timer is already running, the method does nothing. Otherwise, the timer is reset and started.
    /// </summary>
    private void RestartTimer()
    {
        if (_timer.IsRunning)
        {
            return;
        }

        _timer.Reset();
        _timer.Start();
    }

    /// <summary>
    ///     Default implementation of TestResultInstanceProvider that provides simple TestResults without any parameters.
    /// </summary>
    private class DefaultTestResultInstanceProvider : ITestResultInstanceProvider
    {
        /// <summary>
        ///     Creates and returns a TestResult instance representing a successful test case.
        /// </summary>
        /// <param name="testCase">The test case that has succeeded.</param>
        /// <returns>A TestResult instance indicating that the test case has succeeded.</returns>
        public TestResult CreateSuccess(ITestCase testCase)
        {
            return TestResult.Success(testCase.Name, testCase.GetType().Name);
        }

        /// <summary>
        ///     Creates and returns a TestResult instance representing a failed test case.
        /// </summary>
        /// <param name="testCase">The test case that has failed.</param>
        /// <param name="exception">The exception that caused the test case to fail.</param>
        /// <returns>A TestResult instance indicating that the test case has failed, along with the relevant exception.</returns>
        public TestResult CreateFailed(ITestCase testCase, Exception exception)
        {
            return TestResult.Failed(testCase.Name, testCase.GetType().Name, exception);
        }

        /// <summary>
        ///     Creates and returns a TestResult instance representing a skipped test case.
        /// </summary>
        /// <param name="testCase">The test case that has been skipped.</param>
        /// <returns>A TestResult instance indicating that the test case was skipped.</returns>
        public TestResult CreateSkipped(ITestCase testCase)
        {
            return TestResult.Skipped(testCase.Name, testCase.GetType().Name);
        }
    }

    #region Members

    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultTestCase));

    private readonly ITestResultInstanceProvider _defaultTestResultInstanceProvider =
        new DefaultTestResultInstanceProvider();

    private readonly List<ITestActionBuilder<ITestAction>> _finalActions = [];
    private Dictionary<string, object> VariableDefinitions { get; set; } = new();
    private readonly TestCaseMetaInfo _metaInfo = new();
    private readonly Dictionary<string, object> _parameters = new();
    public int Timeout { get; set; } = 10000;
    private readonly Stopwatch _timer = new();

    public DefaultTestCase()
    {
        TestClass = GetType();
        PackageName = GetType().Namespace;
    }

    #endregion

    #region Properties

    public string[] Groups { get; set; }

    public bool IsIncremental { get; set; }

    public string PackageName { get; set; }

    public IDictionary<string, object> Parameters => _parameters;

    public Type TestClass { get; set; }

    public TestResult TestResult { get; set; }

    /// <summary>
    ///     Retrieves a list of action builders associated with the current test case.
    /// </summary>
    /// <returns>A list of action builders that are used to build test actions.</returns>
    public List<ITestActionBuilder<ITestAction>> GetActionBuilders()
    {
        return actions;
    }

    /// <summary>
    ///     Retrieves the final actions for the test case by building them using their respective builders.
    /// </summary>
    /// <return>List of final actions for the test case.</return>
    public List<ITestAction> GetFinalActions()
    {
        return _finalActions.Select(builder => builder.Build()).ToList();
    }

    /// <summary>
    ///     Retrieves the meta-information associated with this test case.
    ///     The meta-information includes details about the test status, description, and other relevant metadata.
    /// </summary>
    /// <returns>The <see cref="TestCaseMetaInfo" /> object containing meta-information of the test case.</returns>
    public TestCaseMetaInfo GetMetaInfo()
    {
        return _metaInfo;
    }

    /// <summary>
    ///     Retrieves the Type of the test class associated with the test case.
    /// </summary>
    /// <returns>The Type of the test class.</returns>
    public Type GetTestClass()
    {
        return TestClass;
    }

    /// <summary>
    ///     Sets the type of the test class to be executed.
    ///     This method assigns the specified type to the internal test class field.
    /// </summary>
    /// <param name="type">The Type of the test class.</param>
    public void SetTestClass(Type type)
    {
        TestClass = type;
    }

    /// <summary>
    ///     Retrieves the package name associated with the test case.
    /// </summary>
    /// <returns>The package name as a string.</returns>
    public string GetNamespaceName()
    {
        return PackageName;
    }

    /// <summary>
    ///     Sets the package name for the test case.
    /// </summary>
    /// <param name="packageName">The name of the package to set.</param>
    public void SetNamespaceName(string packageName)
    {
        PackageName = packageName;
    }

    /// <summary>
    ///     Sets the provided test result for the current test case.
    /// </summary>
    /// <param name="testResult">The test result to set for the test case.</param>
    public void SetTestResult(TestResult testResult)
    {
        TestResult = testResult;
    }

    /// <summary>
    ///     Retrieves a dictionary of parameters associated with the test case.
    /// </summary>
    /// <returns>A dictionary where the keys are parameter names and the values are parameter values.</returns>
    public IDictionary<string, object> GetParameters()
    {
        return _parameters;
    }

    /// <summary>
    ///     Retrieves the current TestResult instance.
    ///     This method returns the TestResult associated with the current test case.
    /// </summary>
    /// <return>The TestResult object for the current test case.</return>
    public TestResult GetTestResult()
    {
        return TestResult;
    }

    bool ITestCase.IsIncremental()
    {
        return IsIncremental;
    }

    /// <summary>
    ///     Sets a flag indicating whether the test case is incremental.
    /// </summary>
    /// <param name="incremental">True if the test case should be incremental, otherwise false.</param>
    public void SetIncremental(bool incremental)
    {
        IsIncremental = incremental;
    }

    /// <summary>
    ///     Retrieves the dictionary containing variable definitions for the test case.
    /// </summary>
    /// <returns>A dictionary with variable names as keys and their corresponding values as dictionary values.</returns>
    public Dictionary<string, object> GetVariableDefinitions()
    {
        return VariableDefinitions;
    }

    /// <summary>
    ///     Sets the variable definitions for the test case. This method allows defining variables
    ///     that can be used and manipulated within the test case, enabling more dynamic and flexible test scenarios.
    /// </summary>
    /// <param name="variableDefinitions">A dictionary containing variable names and their corresponding values.</param>
    public void SetVariableDefinitions(Dictionary<string, object> variableDefinitions)
    {
        VariableDefinitions = variableDefinitions;
    }

    /// <summary>
    ///     Retrieves the groups associated with this test case.
    /// </summary>
    /// <returns>An array of strings representing the groups to which this test case belongs.</returns>
    public string[] GetTestGroups()
    {
        return Groups;
    }

    /// <summary>
    ///     Retrieves the groups associated with the test case.
    ///     This method returns the array of group names assigned to the test case.
    /// </summary>
    /// <return>An array of group names.</return>
    public string[] GetGroups()
    {
        return Groups;
    }

    /// <summary>
    ///     Sets the groups associated with the test case.
    ///     This allows the test case to be categorized into multiple groups.
    /// </summary>
    /// <param name="groups">An array of group names to be associated with the test case.</param>
    public void SetGroups(string[] groups)
    {
        Groups = groups;
    }

    /// <summary>
    ///     Adds a final action to the list of final actions for the test case.
    /// </summary>
    /// <param name="testAction">The action builder for the action to be added as a final action.</param>
    public void AddFinalAction(ITestActionBuilder<ITestAction> testAction)
    {
        _finalActions.Add(testAction);
    }

    /// <summary>
    ///     Adds a specified test action to the list of final actions to be executed.
    ///     Final actions are typically run at the end of the test execution process.
    /// </summary>
    /// <param name="testAction">The test action to be added to the final actions list.</param>
    public void AddFinalAction(ITestAction testAction)
    {
        _finalActions.Add(new FuncITestActionBuilder<ITestAction>(() => testAction));
    }

    /// <summary>
    ///     Retrieves the list of action builders associated with the test case.
    /// </summary>
    /// <returns>A list of action builders implementing the ITestActionBuilder interface.</returns>
    List<ITestActionBuilder<ITestAction>> ITestCase.GetActionBuilders()
    {
        return actions;
    }

    #endregion
}
