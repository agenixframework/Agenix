using System;
using System.Collections.Generic;
using System.Reflection;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Functions;
using FleetPay.Core.Session;
using FleetPay.Core.Validation.Matcher;
using FleetPay.Core.Variable;

namespace FleetPay.Core
{
    /// <summary>
    ///     The test context provides utility methods for replacing dynamic content(variables and functions) in string
    /// </summary>
    public class TestContext
    {
        /// <summary>
        ///     Function registry holding all available functions
        /// </summary>
        private FunctionRegistry _functionRegistry = new();

        /// <summary>
        ///     Function registry holding all available functions
        /// </summary>
        public FunctionRegistry FunctionRegistry
        {
            get => _functionRegistry;
            set => _functionRegistry = value;
        }

        /// <summary>
        ///     ValidationMatcher Registry holding all available matchers
        /// </summary>
        public ValidationMatcherRegistry ValidationMatcherRegistry { get; set; } = new();

        /// <summary>
        ///     Checks if variables are present right now.
        /// </summary>
        /// <returns>boolean flag to mark existence</returns>
        public bool HasVariables()
        {
            return ObjectBag.GetCurrentSession() != null && ObjectBag.GetCurrentSession().Count > 0;
        }

        /// <summary>
        ///     Clears variables in this test context.
        /// </summary>
        public void Clear()
        {
            ObjectBag.ClearCurrentSession();
        }

        /// <summary>
        ///     Getter for test variables in this context.
        /// </summary>
        /// <returns>The test variables for this test context.</returns>
        public IDictionary<object, object> GetVariables()
        {
            return ObjectBag.GetCurrentSession();
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

            if (variableName.StartsWith(CoreSettings.VariableEscape) &&
                variableName.EndsWith(CoreSettings.VariableEscape))
                return CoreSettings.VariablePrefix + VariableUtils.CutOffVariablesEscaping(variableName) +
                       CoreSettings.VariableSuffix;

            if (ObjectBag.GetCurrentSession().ContainsKey(variableName))
                return ObjectBag.SessionVariableCalled<object>(variableName);

            if (variableName.Contains("."))
            {
                var objectName = variableName.Substring(0, variableName.IndexOf(".", StringComparison.Ordinal));
                if (ObjectBag.GetCurrentSession().ContainsKey(objectName))
                    return GetVariable(ObjectBag.SessionVariableCalled<object>(objectName),
                        variableName.Substring(variableName.IndexOf(".", StringComparison.Ordinal) + 1));
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
            if (pathExpression.Contains("."))
            {
                fieldName = pathExpression.Substring(0, pathExpression.IndexOf(".", StringComparison.Ordinal));
                leftOver = pathExpression.Substring(pathExpression.IndexOf(".", StringComparison.Ordinal) + 1);
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
            return (T) GetVariableObject(variableExpression);
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

            ObjectBag.SetSessionVariable(VariableUtils.CutOffVariablesPrefix(variableName)).To(value);
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
            string result = null;

            if (str != null)
            {
                result = VariableUtils.ReplaceVariablesInString(str, this, enableQuoting);
                result = FunctionUtils.ReplaceFunctionsInString(result, this, enableQuoting);
            }

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
        ///     Add several new variables to test context. Existing variables will be overwritten.
        /// </summary>
        /// <param name="variablesToSet">The list of variables to set.</param>
        public void AddVariables(Dictionary<string, object> variablesToSet)
        {
            foreach (var (key, value) in variablesToSet) SetVariable(key, value ?? "");
        }
    }
}