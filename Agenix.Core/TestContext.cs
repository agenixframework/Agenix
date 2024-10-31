using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agenix.Core.Container;
using Agenix.Core.Exceptions;
using Agenix.Core.Functions;
using Agenix.Core.Log;
using Agenix.Core.Message;
using Agenix.Core.Validation.Matcher;
using Agenix.Core.Variable;
using log4net;

namespace Agenix.Core;

/// <summary>
///     The test context provides utility methods for replacing dynamic content(variables and functions) in string
/// </summary>
public class TestContext
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog _log = LogManager.GetLogger(typeof(DefaultFunctionLibrary));

    /// <summary>
    ///     Local variables
    /// </summary>
    protected readonly IDictionary<string, object> _variables;

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
    ///     Message store
    /// </summary>
    private IMessageStore _messageStore = new DefaultMessageStore();

    /// <summary>
    ///     Registered validation matchers
    /// </summary>
    private ValidationMatcherRegistry _validationMatcherRegistry = new();

    /// <summary>
    ///     Default constructor.
    /// </summary>
    public TestContext()
    {
        _variables = new ConcurrentDictionary<string, object>();
    }

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
    ///     Gets or sets the message store.
    /// </summary>
    public IMessageStore MessageStore
    {
        get => _messageStore;
        set => _messageStore = value;
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
        foreach (var entry in _globalVariables.GetVariables()) _variables[entry.Key] = entry.Value;
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
    ///     Gets the value for the given variable as object representation. Use this method if you seek for test objects stored
    ///     in the context.
    /// </summary>
    /// <param name="variableExpression">expression to search for.</param>
    /// <returns>the value of the variable as object</returns>
    public object GetVariableObject(string variableExpression)
    {
        var variableName = VariableUtils.CutOffVariablesPrefix(variableExpression);

        if (variableName.StartsWith(CoreSettings.VariableEscape) && variableName.EndsWith(CoreSettings.VariableEscape))
            return CoreSettings.VariablePrefix + VariableUtils.CutOffVariablesEscaping(variableName) +
                   CoreSettings.VariableSuffix;

        if (_variables.ContainsKey(variableName))
            return _variables[variableName];

        if (variableName.Contains('.'))
        {
            var objectName = variableName[..variableName.IndexOf('.')];
            if (_variables.ContainsKey(objectName))
                return GetVariable(_variables[variableName],
                    variableName[(variableName.IndexOf('.') + 1)..]);
        }

        throw new CoreSystemException("Unknown variable '" + variableName + "'");
    }

    /// <summary>
    ///     Gets variable from path expression. Variable paths are translated to reflection fields on object instances. Path
    ///     separators are '.'. Each separator is handled as object hierarchy.
    /// </summary>
    /// <param name="instance">the instance of object</param>
    /// <param name="pathExpression">The path expression to look for</param>
    /// <returns>The found obj.</returns>
    private object GetVariable(object instance, string pathExpression)
    {
        string leftOver = null;
        string fieldName;
        if (pathExpression.Contains('.'))
        {
            fieldName = pathExpression.Substring(0, pathExpression.IndexOf('.'));
            leftOver = pathExpression.Substring(pathExpression.IndexOf('.') + 1);
        }
        else
        {
            fieldName = pathExpression;
        }

        var objectType = instance.GetType();
        var fieldInfo = objectType.GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        if (fieldInfo == null)
            throw new CoreSystemException(
                $"Failed to get variable - unknown field '{fieldName}' on type {instance.GetType().Name}");

        var fieldValue = fieldInfo.GetValue(objectType);
        return !string.IsNullOrEmpty(leftOver) ? GetVariable(fieldValue, leftOver) : fieldValue;
    }

    /// <summary>
    ///     Gets the value for the given variable expression. Expression usually is the simple variable name, with optional
    ///     expression prefix/suffix.
    ///     In case variable is not known to the context throw runtime exception.
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
        return (T)GetVariableObject(variableExpression);
    }

    /// <summary>
    ///     Creates a new variable in this test context with the respective value. In case variable already exists variable is
    ///     overwritten.
    /// </summary>
    /// <param name="variableName">The name of the new variable</param>
    /// <param name="value">The new variable value</param>
    public void SetVariable(string variableName, object value)
    {
        if (string.IsNullOrEmpty(variableName) || VariableUtils.CutOffVariablesPrefix(variableName).Length == 0)
            throw new CoreSystemException("Can not create variable '" + variableName +
                                          "', please define proper variable name");

        if (value == null)
            throw new VariableNullValueException(
                "Trying to set variable: " + VariableUtils.CutOffVariablesPrefix(variableName) +
                ", but variable value is null");

        if (_log.IsDebugEnabled)
            _log.Debug($"Setting variable: {VariableUtils.CutOffVariablesPrefix(variableName)} with value: '{value}'");

        _variables.Add(VariableUtils.CutOffVariablesPrefix(variableName), value);
    }

    /// <summary>
    ///     Method replacing variable declarations and functions in a string, optionally the variable values get surrounded
    ///     with single quotes.
    /// </summary>
    /// <param name="str">The string to parse for variable place holders.</param>
    /// <param name="enableQuoting">flag marking surrounding quotes should be added or not.</param>
    /// <returns>resulting string without any variable place holders.</returns>
    public string ReplaceDynamicContentInString(string str, bool enableQuoting)
    {
        if (str == null) return null;
        var result = VariableUtils.ReplaceVariablesInString(str, this, enableQuoting);
        result = FunctionUtils.ReplaceFunctionsInString(result, this, enableQuoting);

        return result;
    }

    /// <summary>
    ///     Method replacing variable declarations and place holders as well as function expressions in a string
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <returns>resulting string without any variable place holders.</returns>
    public string ReplaceDynamicContentInString(string str)
    {
        return ReplaceDynamicContentInString(str, false);
    }

    /// <summary>
    ///     Checks weather the given expression is a variable or function and resolves the value accordingly
    /// </summary>
    /// <param name="expression">The expression to resolve</param>
    /// <returns>the resolved expression value</returns>
    public string ResolveDynamicValue(string expression)
    {
        if (VariableUtils.IsVariableName(expression)) return GetVariable(expression);

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
            adaptedValue = (T)(object)ReplaceDynamicContentInString(strValue);
        else
            adaptedValue = value;
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
            if (value is string strValue)
                // Add new value after check if it is variable or function
                variableFreeList.Add((T)(object)ReplaceDynamicContentInString(strValue));
            else
                variableFreeList.Add(value); // Added to preserve non-string values in the list
        return variableFreeList;
    }

    /// <summary>
    ///     Add several new variables to test context. Existing variables will be overwritten.
    /// </summary>
    /// <param name="variablesToSet">The list of variables to set.</param>
    public void AddVariables(Dictionary<string, object> variablesToSet)
    {
        foreach (var (key, value) in variablesToSet) SetVariable(key, value ?? "");
    }

    /// <summary>
    ///     Add variables to context.
    /// </summary>
    /// <param name="variableNames">the variable names to set</param>
    /// <param name="variableValues">the variable values to set</param>
    /// <exception cref="CoreSystemException"></exception>
    public void AddVariables(string[] variableNames, object[] variableValues)
    {
        if (variableNames.Length != variableValues.Length)
            throw new CoreSystemException(
                $"Invalid context variable usage - received '{variableNames.Length}' variables with '{variableValues.Length}' values");

        for (var i = 0; i < variableNames.Length; i++)
            if (variableValues[i] != null)
                SetVariable(variableNames[i], variableValues[i]);
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
    ///     Empty test case implementation used as test result when tests fail before execution.
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

        public string GetPackageName()
        {
            return packageName;
        }

        public void SetPackageName(string packageName)
        {
            // do nothing
        }

        public string Name()
        {
            return testName;
        }

        public void SetTestResult(TestResult testResult)
        {
            // do nothing
        }

        public TestResult GetTestResult()
        {
            return null;
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
    }
}