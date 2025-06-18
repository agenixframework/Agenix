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
public class TestContext : ITestActionListenerAware, IReferenceResolverAware
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
    protected readonly IDictionary<string, object> _variables;

    /// <summary>
    ///     List of actions to run after each test.
    /// </summary>
    private List<IAfterTest> _afterTest = [];

    /// <summary>
    ///     List of actions to run before each test.
    /// </summary>
    private List<IBeforeTest> _beforeTest = [];

    /// <summary>
    ///     Endpoint factory creates endpoint instances
    /// </summary>
    private IEndpointFactory _endpointFactory;

    /// <summary>
    ///     Function registry holding all available functions
    /// </summary>
    private FunctionRegistry _functionRegistry = new();

    /// <summary>
    ///     Set global variables.
    /// </summary>
    private GlobalVariables _globalVariables;

    /// <summary>
    ///     Log modifier.
    /// </summary>
    private ILogModifier _logModifier;

    /// <summary>
    ///     List of message listeners to be informed on inbound and outbound message exchange
    /// </summary>
    private MessageListeners _messageListeners = new();

    /// <summary>
    ///     List of global message processors
    /// </summary>
    private MessageProcessors _messageProcessors = new();

    /// <summary>
    ///     Message store
    /// </summary>
    private IMessageStore _messageStore = new DefaultMessageStore();

    /// <summary>
    ///     Registered validation matchers
    /// </summary>
    private MessageValidatorRegistry _messageValidatorRegistry = new();

    /// <summary>
    ///     POCO reference resolver.
    /// </summary>
    private IReferenceResolver _referenceResolver;

    /// <summary>
    ///     List of test action listeners to be informed on test action events.
    /// </summary>
    private TestActionListeners _testActionListeners = new();

    /// <summary>
    ///     List of test listeners to be informed on test events.
    /// </summary>
    private TestListeners _testListeners = new();

    /// <summary>
    ///     Type converter instance used for converting values between different types.
    /// </summary>
    private ITypeConverter _typeConverter = ITypeConverter.LookupDefault();

    /// <summary>
    ///     Registered validation matchers
    /// </summary>
    private ValidationMatcherRegistry _validationMatcherRegistry = new();


    /// <summary>
    ///     A collection of active timers used within the test context.
    /// </summary>
    protected ConcurrentDictionary<string, IStopTimer> Timers = new();

    /// <summary>
    ///     Default constructor.
    /// </summary>
    public TestContext()
    {
        _variables = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    ///     Provides access to the registry for managing segment variable extractors.
    /// </summary>
    public SegmentVariableExtractorRegistry SegmentVariableExtractorRegistry { get; set; } = new();

    /// <summary>
    ///     Function registry holding all available functions
    /// </summary>
    public FunctionRegistry FunctionRegistry
    {
        get => _functionRegistry;
        set => _functionRegistry = value;
    }

    /// <summary>
    ///     Gets or sets the log modifier.
    /// </summary>
    public ILogModifier LogModifier
    {
        get => _logModifier;
        set => _logModifier = value;
    }

    /// <summary>
    ///     Gets or sets the validation matcher registry.
    /// </summary>
    public ValidationMatcherRegistry ValidationMatcherRegistry
    {
        get => _validationMatcherRegistry;
        set => _validationMatcherRegistry = value;
    }

    /// <summary>
    ///     Manages the collection of message processing strategies.
    /// </summary>
    public MessageProcessors MessageProcessors
    {
        get => _messageProcessors;
        set => _messageProcessors = value;
    }

    /// <summary>
    ///     Represents a builder for configuring and managing namespace contexts in XML structures.
    /// </summary>
    public NamespaceContextBuilder NamespaceContextBuilder { get; set; } = new();

    /// <summary>
    ///     Factory for creating and managing endpoints.
    /// </summary>
    public IEndpointFactory EndpointFactory
    {
        get => _endpointFactory;
        set => _endpointFactory = value;
    }

    /// <summary>
    ///     Provides a registry for message validators.
    /// </summary>
    public virtual MessageValidatorRegistry MessageValidatorRegistry
    {
        get => _messageValidatorRegistry;
        set => _messageValidatorRegistry = value;
    }

    /// <summary>
    ///     Gets or sets the message store.
    /// </summary>
    public IMessageStore MessageStore
    {
        get => _messageStore;
        set => _messageStore = value;
    }

    /// <summary>
    ///     Type converter.
    /// </summary>
    public ITypeConverter TypeConverter
    {
        get => _typeConverter;
        set => _typeConverter = value;
    }

    /// <summary>
    ///     Manages test event listeners and propagates test events to them.
    /// </summary>
    public TestListeners TestListeners
    {
        get => _testListeners;
        set => _testListeners = value;
    }

    /// <summary>
    ///     List of actions to be executed before each test.
    /// </summary>
    public List<IBeforeTest> BeforeTest
    {
        get => _beforeTest;
        set => _beforeTest = value;
    }

    /// <summary>
    ///     Gets or sets the collection of actions to be executed after the test.
    /// </summary>
    public List<IAfterTest> AfterTest
    {
        get => _afterTest;
        set => _afterTest = value;
    }

    /// <summary>
    ///     Manages and interacts with test action listeners.
    /// </summary>
    public TestActionListeners TestActionListeners
    {
        get => _testActionListeners;
        set => _testActionListeners = value;
    }

    /// <summary>
    ///     Manages the collection of message listeners.
    /// </summary>
    public MessageListeners MessageListeners
    {
        get => _messageListeners;
        set => _messageListeners = value;
    }

    /// <summary>
    ///     Retrieves the current reference resolver instance.
    /// </summary>
    /// <returns>The current instance of IReferenceResolver.</returns>
    public virtual IReferenceResolver? ReferenceResolver => _referenceResolver;

    /// <summary>
    ///     Sets the reference resolver to be used by the TestContext.
    /// </summary>
    /// <param name="referenceResolver">The reference resolver to set.</param>
    public void SetReferenceResolver(IReferenceResolver referenceResolver)
    {
        _referenceResolver = referenceResolver;
    }

    /// <summary>
    ///     Adds a test action listener to the context.
    /// </summary>
    /// <param name="listener">The test action listener to be added.</param>
    public void AddTestActionListener(ITestActionListener listener)
    {
        _testActionListeners.AddTestActionListener(listener);
    }


    /// <summary>
    ///     Retrieves a list of message processors that match the specified message direction.
    /// </summary>
    /// <param name="direction">The direction of the message processors to retrieve (INBOUND, OUTBOUND, or UNBOUND).</param>
    /// <returns>A list of message processors that are either unbound or match the specified direction.</returns>
    public List<IMessageProcessor> GetMessageProcessors(MessageDirection direction)
    {
        return _messageProcessors.GetMessageProcessors().Where(processor =>
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
    public bool StopTimer(string timerId)
    {
        if (!Timers.TryGetValue(timerId, out var timer))
        {
            return false;
        }

        timer.StopTimer();
        return true;
    }

    /// <summary>
    ///     Stops all active timers in the current context.
    /// </summary>
    public void StopTimers()
    {
        foreach (var timerId in Timers.Keys)
        {
            StopTimer(timerId);
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
    public bool IsSuccess(TestResult testResult)
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
        return _variables is { Count: > 0 };
    }

    /// <summary>
    ///     Clears variables in this test context.
    /// </summary>
    public void Clear()
    {
        _variables.Clear();
        foreach (var entry in _globalVariables.GetVariables())
        {
            _variables[entry.Key] = entry.Value;
        }
    }

    /// <summary>
    ///     Getter for test variables in this context.
    /// </summary>
    /// <returns>The test variables for this test context.</returns>
    public IDictionary<string, object> GetVariables()
    {
        return _variables;
    }

    /// <summary>
    ///     Informs message listeners that an inbound message was received.
    /// </summary>
    /// <param name="receivedMessage">The received inbound message.</param>
    public virtual void OnInboundMessage(IMessage receivedMessage)
    {
        LogMessage("Receive", receivedMessage, MessageDirection.INBOUND);
    }

    /// <summary>
    ///     Informs message listeners, if present, that a new outbound message is about to be sent.
    /// </summary>
    /// <param name="message">
    ///     The outbound message that is about to be sent.
    /// </param>
    public virtual void OnOutboundMessage(IMessage message)
    {
        LogMessage("Send", message, MessageDirection.OUTBOUND);
    }

    /// <summary>
    ///     Logs the specified message operation with the given message and direction.
    /// </summary>
    /// <param name="operation">The operation being logged.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="direction">The direction of the message.</param>
    private void LogMessage(string operation, IMessage message, MessageDirection direction)
    {
        if (_messageListeners != null && _messageListeners.IsEmpty())
        {
            switch (direction)
            {
                case MessageDirection.OUTBOUND:
                    _messageListeners.OnOutboundMessage(message, this);
                    break;
                case MessageDirection.INBOUND:
                    _messageListeners.OnInboundMessage(message, this);
                    break;
                case MessageDirection.UNBOUND:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        else if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"{operation} message:\n{message?.ToString() ?? ""}");
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
    public AgenixSystemException HandleError(string testName, string packageName, string message, Exception cause)
    {
        // Create an empty fake test case for logging purpose
        ITestCase dummyTest = new EmptyTestCase(testName, packageName);

        var exception = new AgenixSystemException(message, cause);

        // inform test listeners with failed test
        try
        {
            _testListeners.OnTestStart(dummyTest);
            _testListeners.OnTestFailure(dummyTest, exception);
            _testListeners.OnTestFinish(dummyTest);
        }
        catch (Exception e)
        {
            Log.LogWarning("Executing error handler listener failed!", e);
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

        return _variables.TryGetValue(variableName, out var o)
            ? o
            : VariableExpressionIterator.GetLastExpressionValue(variableName, this,
                SegmentVariableExtractorRegistry.SegmentValueExtractors);
    }

    /// <summary>
    ///     Gets the value for the given variable expression. Expression usually is the simple variable name, with optional
    ///     expression prefix/suffix.
    ///     In case the the variable is not known to the context, throw a runtime exception.
    /// </summary>
    /// <param name="variableExpression">expression to search for.</param>
    /// <returns>value of the variable</returns>
    public string GetVariable(string variableExpression)
    {
        return GetVariable<string>(variableExpression, typeof(string));
    }

    /// <summary>
    ///     Gets typed variable value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="variableExpression"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public T GetVariable<T>(string variableExpression, Type type)
    {
        return _typeConverter.ConvertIfNecessary<T>(GetVariableObject(variableExpression), type);
    }

    /// <summary>
    ///     Creates a new variable in this test context with the respective value. In case a variable already exists, the
    ///     variable is
    ///     overwritten.
    /// </summary>
    /// <param name="variableName">The name of the new variable</param>
    /// <param name="value">The new variable value</param>
    public void SetVariable(string variableName, object value)
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
                $"Setting variable: {VariableUtils.CutOffVariablesPrefix(variableName)} with value: '{value}'");
        }

        _variables[VariableUtils.CutOffVariablesPrefix(variableName)] = value;
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
        if (str != null)
        {
            result = VariableUtils.ReplaceVariablesInString(str, this, enableQuoting);
            result = FunctionUtils.ReplaceFunctionsInString(result, this, enableQuoting);
        }

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

        return _functionRegistry.IsFunction(expression)
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
            adaptedValue = (T)(object)ReplaceDynamicContentInString(strValue);
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
    ///     Add several new variables to test context. Existing variables will be overwritten.
    /// </summary>
    /// <param name="variablesToSet">The list of variables to set.</param>
    public void AddVariables(Dictionary<string, object> variablesToSet)
    {
        foreach (var (key, value) in variablesToSet)
        {
            SetVariable(key, value ?? "");
        }
    }

    /// <summary>
    ///     Add variables to context.
    /// </summary>
    /// <param name="variableNames">the variable names to set</param>
    /// <param name="variableValues">the variable values to set</param>
    /// <exception cref="AgenixSystemException"></exception>
    public void AddVariables(string[] variableNames, object[] variableValues)
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
            _variables.Add(adaptedKey, adaptedValue);
            builder.WithVariable(adaptedKey, adaptedValue);
        }

        _globalVariables = builder.Build();
    }

    /// <summary>
    ///     Empty test case implementation used as a test result when tests fail before execution.
    /// </summary>
    /// <param name="testName"></param>
    /// <param name="packageName"></param>
    public sealed class EmptyTestCase(string testName, string packageName) : ITestCase
    {
        public void Execute(TestContext context)
        {
            // do nothing
        }

        public ITestActionContainer SetActions(List<ITestAction> actions)
        {
            return this;
        }

        public List<ITestAction> GetActions()
        {
            return [];
        }

        public long GetActionCount()
        {
            return 0;
        }

        public ITestActionContainer AddTestActions(params ITestAction[] action)
        {
            return this;
        }

        public ITestActionContainer AddTestAction(ITestAction action)
        {
            return this;
        }

        public int GetActionIndex(ITestAction action)
        {
            return 0;
        }

        public void SetActiveAction(ITestAction action)
        {
            // do nothing
        }

        public void SetExecutedAction(ITestAction action)
        {
            // do nothing
        }

        public ITestAction GetActiveAction()
        {
            return null;
        }

        public List<ITestAction> GetExecutedActions()
        {
            return [];
        }

        public ITestAction GetTestAction(int index)
        {
            return null;
        }

        public void SetName(string name)
        {
            // do nothing
        }

        public ITestAction SetDescription(string description)
        {
            return this;
        }

        public void Start(TestContext context)
        {
            // do nothing
        }

        public void ExecuteAction(ITestAction action, TestContext context)
        {
            // do nothing
        }

        public void Finish(TestContext context)
        {
            // do nothing
        }

        public TestCaseMetaInfo GetMetaInfo()
        {
            return new TestCaseMetaInfo();
        }

        public Type GetTestClass()
        {
            return GetType();
        }

        public void SetTestClass(Type type)
        {
            // do nothing
        }

        public string GetNamespaceName()
        {
            return packageName;
        }

        public void SetNamespaceName(string packageName)
        {
            // do nothing
        }

        public void SetTestResult(TestResult testResult)
        {
            // do nothing
        }

        public TestResult GetTestResult()
        {
            return null;
        }

        public bool IsIncremental()
        {
            return true;
        }

        public void SetIncremental(bool incremental)
        {
            // do nothing
        }

        public Dictionary<string, object> GetVariableDefinitions()
        {
            return new Dictionary<string, object>();
        }

        public void AddFinalAction(ITestActionBuilder<ITestAction> builder)
        {
            // do nothing
        }

        public List<ITestActionBuilder<ITestAction>> GetActionBuilders()
        {
            return [];
        }

        public void Fail(Exception throwable)
        {
            // do nothing
        }

        public string Name()
        {
            return testName;
        }
    }
}
