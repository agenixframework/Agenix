#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Collections.Concurrent;
using Agenix.Api.Container;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Report;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Matcher;
using Agenix.Api.Variable;
using Agenix.Api.Xml.Namespace;
using Agenix.Core;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Context;

/// <summary>
///     The test context provides utility methods for replacing dynamic content(variables and functions) in string
/// </summary>
public class TestContext : IAsyncTestActionListenerAware, IReferenceResolverAware
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(TestContext));

    /// <summary>
    ///     List of exceptions that actions raised during execution of forked operations.
    /// </summary>
    private readonly List<AgenixSystemException> _exceptions = [];

    /// <summary>
    ///     Local variables
    /// </summary>
    protected readonly IDictionary<string, object> Variables;

    /// <summary>
    ///     Set global variables.
    /// </summary>
    private GlobalVariables _globalVariables;

    /// <summary>
    ///     POCO reference resolver.
    /// </summary>
    private IReferenceResolver _referenceResolver;

    /// <summary>
    ///     A collection of active timers used within the test context.
    /// </summary>
    protected ConcurrentDictionary<string, IStopTimer> Timers = new();

    /// <summary>
    ///     Default constructor.
    /// </summary>
    public TestContext()
    {
        Variables = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    ///     Provides access to the registry for managing segment variable extractors.
    /// </summary>
    public SegmentVariableExtractorRegistry SegmentVariableExtractorRegistry { get; set; } = new();

    /// <summary>
    ///     Function registry holding all available functions
    /// </summary>
    public FunctionRegistry FunctionRegistry { get; set; } = new();

    /// <summary>
    ///     Gets or sets the log modifier.
    /// </summary>
    public ILogModifier LogModifier { get; set; }

    /// <summary>
    ///     Gets or sets the validation matcher registry.
    /// </summary>
    public ValidationMatcherRegistry ValidationMatcherRegistry { get; set; } = new();

    /// <summary>
    ///     Manages the collection of message processing strategies.
    /// </summary>
    public MessageProcessors MessageProcessors { get; set; } = new();

    /// <summary>
    ///     Represents a builder for configuring and managing namespace contexts in XML structures.
    /// </summary>
    public NamespaceContextBuilder NamespaceContextBuilder { get; set; } = new();

    /// <summary>
    ///     Factory for creating and managing endpoints.
    /// </summary>
    public IEndpointFactory EndpointFactory { get; set; }

    /// <summary>
    ///     Provides a registry for message validators.
    /// </summary>
    public virtual MessageValidatorRegistry MessageValidatorRegistry { get; set; } = new();

    /// <summary>
    ///     Gets or sets the message store.
    /// </summary>
    public IMessageStore MessageStore { get; set; } = new DefaultMessageStore();

    /// <summary>
    ///     Type converter.
    /// </summary>
    public ITypeConverter TypeConverter { get; set; } = ITypeConverter.LookupDefault();

    /// <summary>
    ///     Manages test event listeners and propagates test events to them.
    /// </summary>
    public AsyncTestListeners TestListeners { get; set; } = new();

    /// <summary>
    ///     List of actions to be executed before each test.
    /// </summary>
    public List<IBeforeTest> BeforeTest { get; set; } = [];

    /// <summary>
    ///     Gets or sets the collection of actions to be executed after the test.
    /// </summary>
    public List<IAfterTest> AfterTest { get; set; } = [];

    /// <summary>
    ///     Manages and interacts with test action listeners.
    /// </summary>
    public AsyncTestActionListeners TestActionListeners { get; set; } = new();

    /// <summary>
    ///     Manages the collection of message listeners.
    /// </summary>
    public AsyncMessageListeners? MessageListeners { get; set; } = new();

    /// <summary>
    ///     Retrieves the current reference resolver instance.
    /// </summary>
    /// <returns>The current instance of IReferenceResolver.</returns>
    public virtual IReferenceResolver ReferenceResolver => _referenceResolver;

    /// <summary>
    ///     Adds a test action listener to the context.
    /// </summary>
    /// <param name="listener">The test action listener to be added.</param>
    public void AddTestActionListener(IAsyncTestActionListener listener)
    {
        TestActionListeners.AddTestActionListener(listener);
    }

    /// <summary>
    ///     Sets the reference resolver to be used by the TestContext.
    /// </summary>
    /// <param name="referenceResolver">The reference resolver to set.</param>
    public void SetReferenceResolver(IReferenceResolver referenceResolver)
    {
        _referenceResolver = referenceResolver;
    }


    /// <summary>
    ///     Retrieves a list of message processors that match the specified message direction.
    /// </summary>
    /// <param name="direction">The direction of the message processors to retrieve (INBOUND, OUTBOUND, or UNBOUND).</param>
    /// <returns>A list of message processors that are either unbound or match the specified direction.</returns>
    public List<IMessageProcessor> GetMessageProcessors(MessageDirection direction)
    {
        return MessageProcessors.GetMessageProcessors().Where(processor =>
            {
                var processorDirection = MessageDirection.UNBOUND;

                if (processor is IMessageDirectionAware awareProcessor)
                {
                    processorDirection = awareProcessor.GetDirection();
                }

                return processorDirection == direction || processorDirection == MessageDirection.UNBOUND;
            })
            .ToList();
    }

    /// <summary>
    ///     Add a new exception to the context marking the test as failed. This
    ///     is usually used by actions to mark exceptions during forked operations.
    /// </summary>
    /// <param name="exception">The exception to add.</param>
    public void AddException(AgenixSystemException exception)
    {
        _exceptions.Add(exception);
    }

    /// <summary>
    ///     Gets the value of the exception property.
    /// </summary>
    /// <returns>The list of exceptions.</returns>
    public List<AgenixSystemException> GetExceptions()
    {
        return _exceptions;
    }

    /// <summary>
    ///     Gets exception collection state.
    /// </summary>
    /// <returns>True if there are exceptions, false otherwise.</returns>
    public bool HasExceptions()
    {
        return _exceptions.Count != 0;
    }

    /// <summary>
    ///     Registers a timer with the specified identifier.
    /// </summary>
    /// <param name="timerId">The unique identifier for the timer.</param>
    /// <param name="timer">The timer instance to be registered.</param>
    /// <exception cref="InvalidOperationException">Thrown when a timer with the specified identifier is already registered.</exception>
    public void RegisterTimer(string timerId, IStopTimer timer)
    {
        if (!Timers.TryAdd(timerId, timer))
        {
            throw new InvalidOperationException("Timer already registered with this id");
        }
    }

    /// <summary>
    ///     Stops the timer associated with the specified timer ID.
    /// </summary>
    /// <param name="timerId">The ID of the timer to be stopped.</param>
    /// <returns>True if the timer was successfully stopped; otherwise, false.</returns>
    public async Task<bool> StopTimer(string timerId)
    {
        if (!Timers.TryGetValue(timerId, out var timer))
        {
            return false;
        }

        await timer.StopTimer();
        return true;
    }

    /// <summary>
    ///     Stops all active timers in the current context.
    /// </summary>
    public async Task StopTimers()
    {
        foreach (var timerId in Timers.Keys)
        {
            await StopTimer(timerId);
        }
    }

    /// <summary>
    ///     Determines if the test result indicates success and no exceptions have occurred.
    /// </summary>
    /// <param name="testResult">
    ///     The test result to evaluate.
    /// </param>
    /// <returns>
    ///     True if the test result indicates success and there are no exceptions, otherwise false.
    /// </returns>
    public bool IsSuccess(TestResult? testResult)
    {
        return !HasExceptions() &&
               (testResult?.IsSuccess() ?? false);
    }

    /// <summary>
    ///     Checks if variables are present right now.
    /// </summary>
    /// <returns>boolean flag to mark existence</returns>
    public bool HasVariables()
    {
        return Variables is { Count: > 0 };
    }

    /// <summary>
    ///     Clears variables and exceptions in this test context.
    /// </summary>
    public void Clear()
    {
        Variables.Clear();
        foreach (var entry in _globalVariables.GetVariables())
        {
            Variables[entry.Key] = entry.Value;
        }

        _exceptions.Clear();
    }

    /// <summary>
    ///     Getter for test variables in this context.
    /// </summary>
    /// <returns>The test variables for this test context.</returns>
    public IDictionary<string, object> GetVariables()
    {
        return Variables;
    }

    /// <summary>
    ///     Informs message listeners that an inbound message was received.
    /// </summary>
    /// <param name="receivedMessage">The received inbound message.</param>
    public virtual async Task OnInboundMessage(IMessage receivedMessage)
    {
        await LogMessage("Receive", receivedMessage, MessageDirection.INBOUND);
    }

    /// <summary>
    ///     Informs message listeners, if present, that a new outbound message is about to be sent.
    /// </summary>
    /// <param name="message">
    ///     The outbound message that is about to be sent.
    /// </param>
    public virtual async Task OnOutboundMessage(IMessage message)
    {
        await LogMessage("Send", message, MessageDirection.OUTBOUND);
    }

    /// <summary>
    ///     Logs the specified message operation with the given message and direction.
    /// </summary>
    /// <param name="operation">The operation being logged.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="direction">The direction of the message.</param>
    private async Task LogMessage(string operation, IMessage message, MessageDirection direction)
    {
        if (MessageListeners != null && !MessageListeners.IsEmpty())
        {
            switch (direction)
            {
                case MessageDirection.OUTBOUND:
                    await MessageListeners.OnOutboundMessage(message, this);
                    break;
                case MessageDirection.INBOUND:
                    await MessageListeners.OnInboundMessage(message, this);
                    break;
                case MessageDirection.UNBOUND:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        else if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("{Operation} message:\n{Empty}", operation, message.ToString() ?? "");
        }
    }

    /// <summary>
    ///     Handles an error that occurs during test execution, logs it, and informs test listeners.
    /// </summary>
    /// <param name="testName">The name of the test where the error occurred.</param>
    /// <param name="packageName">The package name of the test where the error occurred.</param>
    /// <param name="message">The error message to be logged.</param>
    /// <param name="cause">The exception that caused the error.</param>
    /// <returns>A CoreSystemException representing the error.</returns>
    public async Task<AgenixSystemException> HandleError(string testName, string packageName, string message,
        Exception cause)
    {
        // Create an empty fake test case for logging purpose
        IAsyncTestCase dummyTest = new EmptyTestCase(testName, packageName);

        var exception = new AgenixSystemException(message, cause);

        // inform test listeners with failed test
        try
        {
            await TestListeners.OnTestStart(dummyTest);
            await TestListeners.OnTestFailure(dummyTest, exception);
            await TestListeners.OnTestFinish(dummyTest);
        }
        catch (Exception e)
        {
            Log.LogWarning(e, "Executing error handler listener failed!");
        }

        return exception;
    }

    /// <summary>
    ///     Gets the value for the given variable as object representation. Use this method if you seek for test objects stored
    ///     in the context.
    /// </summary>
    /// <param name="variableExpression">expression to search for.</param>
    /// <returns>the value of the variable as an object</returns>
    public object GetVariableObject(string variableExpression)
    {
        var variableName = VariableUtils.CutOffVariablesPrefix(variableExpression);

        if (variableName.StartsWith(AgenixSettings.VariableEscape) &&
            variableName.EndsWith(AgenixSettings.VariableEscape))
        {
            return AgenixSettings.VariablePrefix + VariableUtils.CutOffVariablesEscaping(variableName) +
                   AgenixSettings.VariableSuffix;
        }

        return Variables.TryGetValue(variableName, out var o)
            ? o
            : VariableExpressionIterator.GetLastExpressionValue(variableName, this,
                SegmentVariableExtractorRegistry.SegmentValueExtractors);
    }

    /// <summary>
    ///     Gets the value for the given variable expression. Expression usually is the simple variable name, with optional
    ///     expression prefix/suffix.
    ///     In case the variable is not known to the context, throw a runtime exception.
    /// </summary>
    /// <param name="variableExpression">expression to search for.</param>
    /// <returns>value of the variable</returns>
    public string GetVariable(string variableExpression)
    {
        return GetVariable<string>(variableExpression);
    }

    /// <summary>
    ///     Gets typed variable value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="variableExpression"></param>
    /// <returns></returns>
    public T GetVariable<T>(string variableExpression)
    {
        return TypeConverter.ConvertIfNecessary<T>(GetVariableObject(variableExpression), typeof(T));
    }

    /// <summary>
    ///     Creates a new variable in this test context with the respective value. In case a variable already exists, the
    ///     variable is
    ///     overwritten.
    /// </summary>
    /// <param name="variableName">The name of the new variable</param>
    /// <param name="value">The new variable value</param>
    public void SetVariable(string variableName, object? value)
    {
        if (string.IsNullOrEmpty(variableName) || VariableUtils.CutOffVariablesPrefix(variableName).Length == 0)
        {
            throw new AgenixSystemException("Can not create variable '" + variableName +
                                            "', please define proper variable name");
        }

        if (value == null)
        {
            throw new VariableNullValueException(
                "Trying to set variable: " + VariableUtils.CutOffVariablesPrefix(variableName) +
                ", but variable value is null");
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug(
                "Setting variable: {CutOffVariablesPrefix} with value: '{Value}'",
                VariableUtils.CutOffVariablesPrefix(variableName), value);
        }

        Variables[VariableUtils.CutOffVariablesPrefix(variableName)] = value;
    }

    /// <summary>
    ///     Method replacing variable declarations and functions in a string, optionally the variable values get surrounded
    ///     with single quotes.
    /// </summary>
    /// <param name="str">The string to parse for variable placeholders.</param>
    /// <param name="enableQuoting">flag marking surrounding quotes should be added or not.</param>
    /// <returns>resulting string without any variable placeholders.</returns>
    public virtual string ReplaceDynamicContentInString(string? str, bool enableQuoting = false)
    {
        string? result = null;
        if (str == null)
        {
            return result;
        }

        result = VariableUtils.ReplaceVariablesInString(str, this, enableQuoting);
        result = FunctionUtils.ReplaceFunctionsInString(result, this, enableQuoting);

        return result;
    }

    /// <summary>
    ///     Checks weather the given expression is a variable or function and resolves the value accordingly
    /// </summary>
    /// <param name="expression">The expression to resolve</param>
    /// <returns>the resolved expression value</returns>
    public string ResolveDynamicValue(string expression)
    {
        if (VariableUtils.IsVariableName(expression))
        {
            return GetVariable(expression);
        }

        return FunctionRegistry.IsFunction(expression)
            ? FunctionUtils.ResolveFunction(expression, this)
            : expression;
    }

    /// <summary>
    ///     Checks for and resolves the dynamic content in the the supplied value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">the value, optionally containing dynamic content</param>
    /// <returns>the original value or the value with the resolved dynamic content</returns>
    private T ResolveDynamicContentIfRequired<T>(T value)
    {
        T adaptedValue;
        if (value is string strValue)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            adaptedValue = (T)(object)ReplaceDynamicContentInString(strValue);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        else
        {
            adaptedValue = value;
        }

        return adaptedValue;
    }

    /// <summary>
    ///     Replaces variables and functions in array with respective values and returns the new array representation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">having optional variable entries.</param>
    /// <returns>the constructed list without variable entries.</returns>
    public T[] ResolveDynamicValuesInArray<T>(T[] array)
    {
        return ResolveDynamicValuesInList(array.ToList()).ToArray();
    }

    /// <summary>
    ///     Replaces variables and functions inside a map with respective values and returns a new map representation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="map">optionally having variable entries.</param>
    /// <returns>the constructed map without variable entries.</returns>
    public Dictionary<string, T> ResolveDynamicValuesInMap<T>(Dictionary<string, T> map)
    {
        Dictionary<string, T> target = new(map.Count);

        foreach (var entry in map)
        {
            var adaptedKey = ResolveDynamicContentIfRequired(entry.Key);
            var adaptedValue = ResolveDynamicContentIfRequired(entry.Value);
            target[adaptedKey] = adaptedValue;
        }

        return target;
    }

    /// <summary>
    ///     Replaces variables and functions in a list with respective values and returns the new list representation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">having optional variable entries.</param>
    /// <returns>the constructed list without variable entries.</returns>
    public List<T> ResolveDynamicValuesInList<T>(List<T> list)
    {
        List<T> variableFreeList = new(list.Count);

        foreach (var value in list)
        {
            if (value is string strValue)
            // Add new value after check if it is variable or function
            {
                variableFreeList.Add((T)(object)ReplaceDynamicContentInString(strValue));
            }
            else
            {
                variableFreeList.Add(value); // Added to preserve non-string values in the list
            }
        }

        return variableFreeList;
    }

    /// <summary>
    ///     Add several new variables to the test context. Existing variables will be overwritten.
    /// </summary>
    /// <param name="variablesToSet">The list of variables to set.</param>
    public void AddVariables(Dictionary<string, object> variablesToSet)
    {
        foreach (var (key, value) in variablesToSet)
        {
            SetVariable(key, value);
        }
    }

    /// <summary>
    ///     Add variables to context.
    /// </summary>
    /// <param name="variableNames">the variable names to set</param>
    /// <param name="variableValues">the variable values to set</param>
    /// <exception cref="AgenixSystemException"></exception>
    public void AddVariables(string[] variableNames, object?[] variableValues)
    {
        if (variableNames.Length != variableValues.Length)
        {
            throw new AgenixSystemException(
                $"Invalid context variable usage - received '{variableNames.Length}' variables with '{variableValues.Length}' values");
        }

        for (var i = 0; i < variableNames.Length; i++)
        {
            if (variableValues[i] != null)
            {
                SetVariable(variableNames[i], variableValues[i]);
            }
        }
    }

    /// <summary>
    ///     Gets global variables.
    /// </summary>
    /// <returns>the globalVariables</returns>
    public Dictionary<string, object> GetGlobalVariables()
    {
        return _globalVariables.GetVariables();
    }

    /// <summary>
    ///     Copies the passed globalVariables and adds them to the test context.
    ///     If any of the copied global variables contain dynamic content (references to other global variables or functions)
    ///     then this is resolved now.
    ///     As a result it is important setFunctionRegistry(FunctionRegistry) is called first before calling this method.
    /// </summary>
    /// <param name="globalVariables"></param>
    public void SetGlobalVariables(GlobalVariables globalVariables)
    {
        var builder = new GlobalVariables.Builder();
        foreach (var entry in globalVariables.GetVariables())
        {
            var adaptedKey = ResolveDynamicContentIfRequired(entry.Key);
            var adaptedValue = ResolveDynamicContentIfRequired(entry.Value);
            Variables.Add(adaptedKey, adaptedValue);
            builder.WithVariable(adaptedKey, adaptedValue);
        }

        _globalVariables = builder.Build();
    }

    /// <summary>
    ///     Empty test case implementation used as a test result when tests fail before execution.
    /// </summary>
    /// <param name="testName"></param>
    /// <param name="packageName"></param>
    public sealed class EmptyTestCase(string testName, string packageName) : IAsyncTestCase
    {
        /// <summary>
        ///     Executes the asynchronous operation for the test case within the specified context.
        /// </summary>
        /// <param name="context">The <see cref="TestContext" /> object providing the context for the test execution.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> used to observe cancellation requests.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public Task ExecuteAsync(TestContext context, CancellationToken cancellationToken = default)
        {
            // do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Sets the list of actions to be executed within the test case.
        /// </summary>
        /// <param name="actions">A list of actions to be set for execution in the test case.</param>
        /// <returns>An instance of <see cref="IAsyncTestActionContainer" /> representing the container of the configured actions.</returns>
        public IAsyncTestActionContainer SetActions(List<IAsyncTestAction> actions)
        {
            return this;
        }

        /// <summary>
        ///     Retrieves a list of asynchronous test actions associated with the test case.
        /// </summary>
        /// <returns>
        ///     A list of <see cref="IAsyncTestAction" /> representing the asynchronous test actions.
        /// </returns>
        public List<IAsyncTestAction> GetActions()
        {
            return [];
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public long GetActionCount()
        {
            return 0;
        }

        /// <summary>
        ///     Adds one or more test actions to the container.
        /// </summary>
        /// <param name="action">An array of test actions to be added.</param>
        /// <returns>The current instance of <c>IAsyncTestActionContainer</c> to allow method chaining.</returns>
        public IAsyncTestActionContainer AddTestActions(params IAsyncTestAction[] action)
        {
            return this;
        }

        /// <summary>
        ///     Adds a test action to the container.
        /// </summary>
        /// <param name="action">The test action to be added.</param>
        /// <returns>The current instance of <see cref="IAsyncTestActionContainer" /> after the action is added.</returns>
        public IAsyncTestActionContainer AddTestAction(IAsyncTestAction action)
        {
            return this;
        }

        /// <summary>
        ///     Retrieves the index of the specified test action within the collection of test actions.
        /// </summary>
        /// <param name="action">The test action to locate in the collection.</param>
        /// <returns>The zero-based index of the specified test action if found; otherwise, -1.</returns>
        public int GetActionIndex(IAsyncTestAction action)
        {
            return 0;
        }

        /// <summary>
        ///     Sets the specified test action as the active action.
        /// </summary>
        /// <param name="action">The test action to be marked as active.</param>
        public void SetActiveAction(IAsyncTestAction action)
        {
            // do nothing
        }

        /// <summary>
        ///     Sets the action as executed in the test context.
        /// </summary>
        /// <param name="action">
        ///     The action to mark as executed.
        /// </param>
        public void SetExecutedAction(IAsyncTestAction action)
        {
            // do nothing
        }

        /// <summary>
        ///     Retrieves the currently active asynchronous test action.
        /// </summary>
        /// <returns>
        ///     The active asynchronous test action, or null if no active action is set.
        /// </returns>
        public IAsyncTestAction GetActiveAction()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        ///     Retrieves a list of executed asynchronous test actions.
        /// </summary>
        /// <returns>
        ///     A list of executed <see cref="IAsyncTestAction" /> objects.
        /// </returns>
        public List<IAsyncTestAction> GetExecutedActions()
        {
            return [];
        }

        /// <summary>
        ///     Retrieves the test action located at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the test action to retrieve.</param>
        /// <returns>The test action at the specified index. If no action is found, returns null.</returns>
        public IAsyncTestAction GetTestAction(int index)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        ///     Sets the name for the current test case.
        /// </summary>
        /// <param name="name">The name to assign to the test case.</param>
        public void SetName(string name)
        {
            // do nothing
        }

        /// <summary>
        ///     Sets a description for the test action.
        /// </summary>
        /// <param name="description">The description to assign to the test action.</param>
        /// <returns>
        ///     The current instance of <see cref="IAsyncTestAction" /> to allow method chaining.
        /// </returns>
        public IAsyncTestAction SetDescription(string description)
        {
            return this;
        }

        /// <summary>
        ///     Executes the start operation for the given test context asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="TestContext" /> instance that provides context for the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task Start(TestContext context)
        {
            // do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Executes a specified asynchronous test action within the given test context.
        /// </summary>
        /// <param name="action">The asynchronous test action to be executed.</param>
        /// <param name="context">The test context in which the action will be executed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task ExecuteAction(IAsyncTestAction action, TestContext context)
        {
            // do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Completes the test execution for the provided test context asynchronously.
        /// </summary>
        /// <param name="context">The test context to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Finish(TestContext context)
        {
            // do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Provides metadata information about the test case.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="TestCaseMetaInfo" /> representing the metadata of the test case.
        /// </returns>
        public TestCaseMetaInfo GetMetaInfo()
        {
            return new TestCaseMetaInfo();
        }

        /// <summary>
        ///     Retrieves the type of the test class associated with this instance.
        /// </summary>
        /// <returns>The type of the test class.</returns>
        public Type GetTestClass()
        {
            return GetType();
        }

        /// <summary>
        ///     Assigns the specified type as the test class.
        /// </summary>
        /// <param name="type">The type to be set as the test class.</param>
        public void SetTestClass(Type? type)
        {
            // do nothing
        }

        /// <summary>
        ///     Retrieves the namespace name associated with the test case.
        /// </summary>
        /// <returns>
        ///     The namespace name as a string.
        /// </returns>
        public string GetNamespaceName()
        {
            return packageName;
        }

        /// <summary>
        ///     Sets the namespace name for the test case.
        /// </summary>
        /// <param name="packageName">The namespace name to be set.</param>
        // ReSharper disable once ParameterHidesPrimaryConstructorParameter
        public void SetNamespaceName(string packageName)
        {
            // do nothing
        }

        /// <summary>
        ///     Sets the test result for the test case.
        /// </summary>
        /// <param name="testResult">The test result to be associated with the test case.</param>
        public void SetTestResult(TestResult testResult)
        {
            // do nothing
        }

        /// <summary>
        ///     Retrieves the result of the test execution.
        /// </summary>
        /// <returns>The result of the test as a <see cref="TestResult" />.</returns>
        public TestResult GetTestResult()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        ///     Indicates whether the test execution is incremental.
        /// </summary>
        /// <returns>
        ///     A boolean value indicating if the test is executed incrementally.
        /// </returns>
        public bool IsIncremental()
        {
            return true;
        }

        /// <summary>
        ///     Sets the incremental execution mode for the test case.
        /// </summary>
        /// <param name="incremental">
        ///     A boolean value indicating whether the test case should be executed incrementally.
        /// </param>
        public void SetIncremental(bool incremental)
        {
            // do nothing
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetVariableDefinitions()
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        ///     Adds a final action to the current test case.
        /// </summary>
        /// <param name="builder">The builder used to create and configure the action to be added.</param>
        public void AddFinalAction(IAsyncTestActionBuilder<IAsyncTestAction> builder)
        {
            // do nothing
        }

        /// <summary>
        ///     Retrieves a list of action builders capable of constructing asynchronous test actions.
        /// </summary>
        /// <returns>
        ///     A list containing instances of <see cref="IAsyncTestActionBuilder{T}" />, where T is of type
        ///     <see cref="IAsyncTestAction" />.
        /// </returns>
        public List<IAsyncTestActionBuilder<IAsyncTestAction>> GetActionBuilders()
        {
            return [];
        }

        /// <summary>
        ///     Marks the test case as failed by providing an exception that caused the failure.
        /// </summary>
        /// <param name="throwable">
        ///     The exception representing the cause of the test failure.
        /// </param>
        public void Fail(Exception throwable)
        {
            // do nothing
        }

        /// <summary>
        ///     Retrieves the name associated with the test case.
        /// </summary>
        /// <returns>
        ///     The name of the test case as a string.
        /// </returns>
        public string Name()
        {
            return testName;
        }
    }
}
