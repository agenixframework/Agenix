using System;
using System.Text;
using MPP.Core.Exceptions;

namespace MPP.Core.Variable
{
    /// <summary>
    /// Utility class manipulating test variables.
    /// </summary>
    public sealed class VariableUtils
    {
        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        private VariableUtils()
        {
        }

        public static string ReplaceVariablesInString(string str, TestContext context, bool enableQuoting)
        {
            var newStr = new StringBuilder();

            bool isVarComplete;
            var variableNameBuf = new StringBuilder();

            int startIndex = 0;
            int curIndex;
            int searchIndex;

            while ((searchIndex = str.IndexOf(CoreSettings.VariablePrefix, startIndex, StringComparison.Ordinal)) != -1)
            {
                int control = 0;
                isVarComplete = false;

                curIndex = searchIndex + CoreSettings.VariablePrefix.Length;

                while (curIndex < str.Length && !isVarComplete)
                {
                    if (str.IndexOf(CoreSettings.VariablePrefix, curIndex, StringComparison.Ordinal) == curIndex)
                    {
                        control++;
                    }

                    if ((str[curIndex] == CoreSettings.VariablePrefix[0]) || (curIndex + 1 == str.Length))
                    {
                        if (control == 0)
                        {
                            isVarComplete = true;
                        }
                        else
                        {
                            control--;
                        }
                    }

                    if (!isVarComplete)
                    {
                        variableNameBuf.Append(str[curIndex]);
                    }

                    ++curIndex;
                }

                var value = context.GetVariable(variableNameBuf.ToString());
                if (value == null)
                {
                    throw new NoSuchVariableException("Variable: " + variableNameBuf + " could not be found");
                }

                newStr.Append(str.Substring(startIndex, searchIndex));

                if (enableQuoting)
                {
                    newStr.Append("'" + value + "'");
                }
                else
                {
                    newStr.Append(value);
                }

                startIndex = curIndex;

                variableNameBuf = new StringBuilder();
                isVarComplete = false;
            }

            newStr.Append(str.Substring(startIndex));

            return newStr.ToString();
        }

        public static string CutOffVariablesPrefix(string variable)
        {
            if (variable.StartsWith(CoreSettings.VariablePrefix) && variable.EndsWith(CoreSettings.VariableSuffix))
            {
                return variable.Substring(CoreSettings.VariablePrefix.Length,
                    variable.Length - CoreSettings.VariableSuffix.Length);
            }

            return variable;
        }

        public static string CutOffVariablesEscaping(string variable)
        {
            if (variable.StartsWith(CoreSettings.VariableEscape) && variable.EndsWith(CoreSettings.VariableEscape))
            {
                return variable.Substring(CoreSettings.VariableEscape.Length,
                    variable.Length - CoreSettings.VariableEscape.Length);
            }

            return variable;
        }

        /// <summary>
        /// Checks whether a given expression is a variable name.
        /// </summary>
        /// <param name="expression">expression</param>
        /// <returns>flag true/false</returns>
        public static bool IsVariableName(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            return expression.StartsWith(CoreSettings.VariablePrefix) &&
                   expression.EndsWith(CoreSettings.VariableSuffix);
        }
    }
}