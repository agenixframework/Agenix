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
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Spi;
using Agenix.Core.Actions;
using Agenix.Core.Container;

namespace Agenix.Core;

/// <summary>
///     Default implementation of the ITestCaseRunner interface, providing methods
///     to manage and execute test cases.
/// </summary>
public class DefaultTestCaseRunner : ITestCaseRunner
{
    /**
     * The test case
     */
    private ITestCase _testCase;

    /// Default implementation of the ITestCaseRunner interface, providing methods to manage and execute test cases.
    /// /
    public DefaultTestCaseRunner(TestContext context) : this(new DefaultTestCase(), context)
    {
    }

    /// Class responsible for running test cases with a specified context.
    /// /
    public DefaultTestCaseRunner(ITestCase testCase, TestContext context)
    {
        _testCase = testCase;
        Context = context;

        _testCase.SetIncremental(true);
    }

    /**
     * The test context
     */
    public TestContext Context { get; }

    /// Retrieves the test case associated with this test case runner.
    /// <returns>
    ///     The current instance of ITestCase associated with this runner.
    /// </returns>
    public ITestCase GetTestCase()
    {
        return _testCase;
    }

    /// Sets the test class type for the current test case.
    /// <param name="type">The type representing the test class to be set.</param>
    public void SetTestClass(Type type)
    {
        _testCase.SetTestClass(type);
    }

    /// Sets the name of the test case.
    /// <param name="name">The name to set for the test case.</param>
    public void SetName(string name)
    {
        _testCase.SetName(name);
    }

    /// Sets the description for the test case.
    /// <param name="description">The description to set for the test case.</param>
    public void SetDescription(string description)
    {
        _testCase.SetDescription(description);
    }

    /// Sets the author for the current test case.
    /// <param name="author">The author's name to be associated with the test case.</param>
    public void SetAuthor(string author)
    {
        _testCase.GetMetaInfo().Author = author;
    }

    /// Sets the name of the package for the test case.
    /// <param name="packageName">The name of the package to be set for the test case.</param>
    public void SetNamespaceName(string packageName)
    {
        _testCase.SetNamespaceName(packageName);
    }

    /// Sets the status of the test case.
    /// <param name="status">The new status for the test case.</param>
    public void SetStatus(TestCaseMetaInfo.Status status)
    {
        _testCase.GetMetaInfo().SetStatus(status);
    }

    /// Sets the creation date for the test case.
    /// <param name="date">The date to set as the creation date of the test case.</param>
    public void SetCreationDate(DateTime date)
    {
        _testCase.GetMetaInfo().CreationDate = date;
    }

    /// Sets the groups for the test case. If the test case is group-aware, the specified groups are assigned.
    /// <param name="groups">Array of group names to set for the test case.</param>
    public void SetGroups(string[] groups)
    {
        if (_testCase is ITestGroupAware aware)
        {
            aware.SetGroups(groups);
        }
    }

    /// Sets a variable with the specified name and value.
    /// If the value is a string, any dynamic content within the string is resolved before setting the variable.
    /// Updates the variable definitions within the test case and sets the variable in the test context.
    /// <typeparam name="T">The type of the variable.</typeparam>
    /// <param name="name">The name of the variable to set.</param>
    /// <param name="value">The value of the variable to set.</param>
    /// <return>The value that was set, potentially modified if it was a string value.</return>
    public T SetVariable<T>(string name, T value)
    {
        _testCase.GetVariableDefinitions()[name] = value;

        if (value is string strValue)
        {
            var resolved = Context.ReplaceDynamicContentInString(strValue);
            Context.SetVariable(name, resolved);
            return (T)(object)resolved;
        }

        Context.SetVariable(name, value);
        return value;
    }

    /// Executes the specified test action.
    /// <param name="action">The test action to be executed.</param>
    /// <typeparam name="T">Type of the test action, which implements ITestAction.</typeparam>
    /// <return>Returns the executed test action of type T.</return>
    public T Run<T>(T action) where T : ITestAction
    {
        var builder = new FuncITestActionBuilder<T>(() => action);
        return Run(builder);
    }

    /// Executes the provided test action and handles the required operations before and after the execution.
    /// <typeparam name="T">The type of action that implements the ITestAction interface.</typeparam>
    /// <param name="action">A function that returns an action to be executed.</param>
    /// <returns>The executed action of type T.</returns>
    public T Run<T>(Func<T> action) where T : ITestAction
    {
        return Run(action.Invoke());
    }

    /// Runs a provided test action builder and manages the execution lifecycle.
    /// <param name="builder">The builder responsible for creating the test action.</param>
    /// <typeparam name="T">The type of test action being run.</typeparam>
    /// <returns>The test action that was executed.</returns>
    public T Run<T>(ITestActionBuilder<T> builder) where T : ITestAction
    {
        if (builder is IReferenceResolverAware referenceResolverAwareBuilder)
        {
            referenceResolverAwareBuilder.SetReferenceResolver(Context.ReferenceResolver);
        }

        if (builder is ApplyTestBehaviorAction.Builder applyTestBehaviorBuilder)
        {
            applyTestBehaviorBuilder.On(this);
        }

        var action = builder.Build();

        switch (builder)
        {
            case FinallySequence.Builder finallySequenceBuilder:
            {
                foreach (var finalAction in finallySequenceBuilder.GetActions())
                {
                    _testCase.AddFinalAction(finalAction);
                }

                return action;
            }
            case FuncITestActionBuilder<FinallySequence.Builder> finallySequenceBuilder:
            {
                foreach (var finalAction in finallySequenceBuilder.Build().GetActions())
                {
                    _testCase.AddFinalAction(finalAction);
                }

                return action;
            }
        }

        _testCase.AddTestAction(action);
        _testCase.ExecuteAction(action, Context);
        return action;
    }


    public ITestActionBuilder<ITestAction> ApplyBehavior(ITestBehavior behavior)
    {
        return new ApplyTestBehaviorAction.Builder()
            .Behavior(behavior)
            .On(this);
    }

    /// Applies a specified test behavior to the current test case.
    /// <param name="behavior">The test behavior to be applied.</param>
    /// <returns>A builder for creating an instance of ApplyTestBehaviorAction.</returns>
    public ITestActionBuilder<ITestAction> ApplyBehavior(TestBehavior behavior)
    {
        return new ApplyTestBehaviorAction.Builder()
            .Behavior(new DelegatingTestBehaviour(behavior))
            .On(this);
    }

    /// Starts the execution of the test case within the current test context.
    /// This method initializes the required environment and triggers the start of the test case logic.
    public void Start()
    {
        _testCase.Start(Context);
    }

    /// Stops the execution of the test case and triggers the Finish method on the test case instance to perform any necessary cleanup tasks using the provided test context.
    public void Stop()
    {
        _testCase.Finish(Context);
    }

    /// Sets the test case to be used by the test runner.
    /// <param name="testCase">The test case to be used for executing test actions.</param>
    public void SetTestCase(ITestCase testCase)
    {
        _testCase = testCase;
        _testCase.SetIncremental(true);
    }

    /// <summary>
    ///     Provides methods to create instances of ITestCaseRunner, specifically DefaultTestCaseRunner,
    ///     with different configurations including test context and test case.
    /// </summary>
    public class DefaultTestCaseRunnerProvider : ITestCaseRunnerProvider
    {
        /// Creates a new instance of a test case runner with the given test context.
        /// <param name="context">The context for the test case runner which provides runtime information and dependencies.</param>
        /// <return>Returns an instance of ITestCaseRunner, specifically DefaultTestCaseRunner.</return>
        public ITestCaseRunner CreateTestCaseRunner(TestContext context)
        {
            return new DefaultTestCaseRunner(context);
        }

        /// Creates an instance of DefaultTestCaseRunner with the specified test case and context.
        /// <param name="testCase">An implementation of the ITestCase interface representing the test case to be run.</param>
        /// <param name="context">A TestContext instance providing the execution context for the test case.</param>
        /// <returns>An instance of ITestCaseRunner configured with the specified test case and context.</returns>
        public ITestCaseRunner CreateTestCaseRunner(ITestCase testCase, TestContext context)
        {
            return new DefaultTestCaseRunner(testCase, context);
        }
    }
}
