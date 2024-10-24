using System;
using System.Text;
using Agenix.Core.Variable;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Functions
{
    /// <summary>
    ///     Utility class for functions.
    /// </summary>
    public sealed class FunctionUtils
    {
        /// <summary>
        ///     Prevent class instantiation.
        /// </summary>
        private FunctionUtils()
        {
        }

        /// <summary>
        ///     Search for functions in string and replace with respective function result.
        /// </summary>
        /// <param name="str">to parse</param>
        /// <param name="context">parsed string result</param>
        /// <returns></returns>
        public static string ReplaceFunctionsInString(string str, TestContext context)
        {
            return ReplaceFunctionsInString(str, context, false);
        }

        /// <summary>
        ///     Search for functions in string and replace with respective function result.
        /// </summary>
        /// <param name="stringValue">to parse.</param>
        /// <param name="context">The test context object.</param>
        /// <param name="enableQuoting">enables quoting of function results.</param>
        /// <returns>parsed string result.</returns>
        public static string ReplaceFunctionsInString(string stringValue, TestContext context, bool enableQuoting)
        {
            // make sure given string expression meets requirements for having a function
            if (string.IsNullOrEmpty(stringValue) || stringValue.IndexOf(':') < 0 || stringValue.IndexOf('(') < 0 ||
                stringValue.IndexOf(')') < 0)
                // it is not a function, as it is defined as 'prefix:methodName(arguments)'
                return stringValue;

            var newString = stringValue;
            var strBuffer = new StringBuilder();

            var isVarComplete = false;
            var variableNameBuf = new StringBuilder();

            var startIndex = 0;
            int curIndex;
            int searchIndex;

            foreach (var library in context.FunctionRegistry.FunctionLibraries)
            {
                startIndex = 0;
                while ((searchIndex = newString.IndexOf(library.Prefix, startIndex, StringComparison.Ordinal)) != -1)
                {
                    var control = -1;
                    isVarComplete = false;

                    curIndex = searchIndex;

                    while (curIndex < newString.Length && !isVarComplete)
                    {
                        if (newString.IndexOf('(', curIndex) == curIndex) control++;

                        if (newString[curIndex] == ')' || curIndex == newString.Length - 1)
                        {
                            if (control == 0)
                                isVarComplete = true;
                            else
                                control--;
                        }

                        variableNameBuf.Append(newString[curIndex]);
                        curIndex++;
                    }

                    var value = ResolveFunction(variableNameBuf.ToString(), context);

                    strBuffer.Append(newString.Substring(startIndex, searchIndex - startIndex));

                    if (enableQuoting)
                        strBuffer.Append("'" + value + "'");
                    else
                        strBuffer.Append(value);

                    startIndex = curIndex;

                    variableNameBuf = new StringBuilder();
                    isVarComplete = false;
                }

                strBuffer.Append(newString.Substring(startIndex));
                newString = strBuffer.ToString();

                strBuffer = new StringBuilder();
            }

            return newString;
        }

        public static string ResolveFunction(string functionString, TestContext context)
        {
            var functionExpression = VariableUtils.CutOffVariablesPrefix(functionString);

            if (!functionExpression.Contains("(") || !functionExpression.EndsWith(")") ||
                !functionExpression.Contains(":"))
                throw new InvalidFunctionUsageException("Unable to resolve function: " + functionExpression);

            var functionPrefix = functionExpression.Substring(0, functionExpression.IndexOf(':') + 1);
            var startIndexString = functionExpression.IndexOf('(') + 1;
            var parameterString =
                functionExpression.Substring(startIndexString)
                    .Substring(0, functionExpression.Substring(startIndexString).Length - 1);
            var startFunctionString = functionExpression.Substring(functionExpression.IndexOf(':') + 1);
            var function = startFunctionString.Substring(0, startFunctionString.IndexOf('('));

            var library = context.FunctionRegistry.GetLibraryForPrefix(functionPrefix);

            parameterString = VariableUtils.ReplaceVariablesInString(parameterString, context, false);
            parameterString = ReplaceFunctionsInString(parameterString, context);

            var value = library.GetFunction(function)
                .Execute(FunctionParameterHelper.GetParameterList(parameterString), context);

            return value ?? "";
        }
    }
}