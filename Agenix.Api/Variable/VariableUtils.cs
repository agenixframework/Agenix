using System.Text;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;

namespace Agenix.Api.Variable;

/// <summary>
///     Utility class manipulating test variables.
/// </summary>
public sealed class VariableUtils
{
    /// <summary>
    ///     Prevent instantiation.
    /// </summary>
    private VariableUtils()
    {
    }

    /// <summary>
    /// Replaces variables in the given string based on the provided context and quoting configuration.
    /// </summary>
    /// <param name="str">The input string containing variables to be replaced.</param>
    /// <param name="context">The context used to resolve variable values.</param>
    /// <param name="enableQuoting">Determines whether the resolved variable values should be quoted.</param>
    /// <returns>The processed string with variables replaced by their resolved values.</returns>
    /// <exception cref="NoSuchVariableException">Thrown when a variable referenced in the input string cannot be resolved in the provided context.</exception>
    public static string ReplaceVariablesInString(string str, TestContext context, bool enableQuoting)
    {
        var newStr = new StringBuilder();

        bool isVarComplete;
        var variableNameBuf = new StringBuilder();

        var startIndex = 0;
        int curIndex;
        int searchIndex;

        while ((searchIndex = str.IndexOf(AgenixSettings.VariablePrefix, startIndex, StringComparison.Ordinal)) != -1)
        {
            var control = 0;
            isVarComplete = false;

            curIndex = searchIndex + AgenixSettings.VariablePrefix.Length;

            while (curIndex < str.Length && !isVarComplete)
            {
                if (str.IndexOf(AgenixSettings.VariablePrefix, curIndex, StringComparison.Ordinal) ==
                    curIndex) control++;

                if (str[curIndex] == AgenixSettings.VariableSuffix[0] || curIndex + 1 == str.Length)
                {
                    if (control == 0)
                        isVarComplete = true;
                    else
                        control--;
                }

                if (!isVarComplete) variableNameBuf.Append(str[curIndex]);

                ++curIndex;
            }

            var value = context.GetVariable(variableNameBuf.ToString());
            if (value == null)
                throw new NoSuchVariableException("Variable: " + variableNameBuf + " could not be found");

            newStr.Append(str.Substring(startIndex, searchIndex - startIndex));

            if (enableQuoting)
                newStr.Append("'" + value + "'");
            else
                newStr.Append(value);

            startIndex = curIndex;

            variableNameBuf = new StringBuilder();
            isVarComplete = false;
        }

        newStr.Append(str.Substring(startIndex));

        return newStr.ToString();
    }

    /// <summary>
    /// Removes the prefix and suffix from a variable name if they are defined by the application settings.
    /// </summary>
    /// <param name="variable">The variable name from which to remove the prefix and suffix.</param>
    /// <returns>
    /// The variable name without the prefix and suffix if both are present; otherwise, returns the original variable name unchanged.
    /// </returns>
    public static string CutOffVariablesPrefix(string variable)
    {
        if (variable.StartsWith(AgenixSettings.VariablePrefix) && variable.EndsWith(AgenixSettings.VariableSuffix))
            return variable.Substring(AgenixSettings.VariablePrefix.Length,
                variable.Length - AgenixSettings.VariableSuffix.Length - 2);

        return variable;
    }

    /// <summary>
    /// Removes the variable escape sequences defined in <see cref="AgenixSettings.VariableEscape"/>
    /// from the beginning and end of the provided variable name, if present.
    /// </summary>
    /// <param name="variable">The variable name to process.</param>
    /// <returns>
    /// The variable name with the escape sequences removed if they are present;
    /// otherwise, returns the original variable name unchanged.
    /// </returns>
    public static string CutOffVariablesEscaping(string variable)
    {
        if (variable.StartsWith(AgenixSettings.VariableEscape) && variable.EndsWith(AgenixSettings.VariableEscape))
            return variable.Substring(AgenixSettings.VariableEscape.Length,
                variable.Length - (2 * AgenixSettings.VariableEscape.Length));

        return variable;
    }


    /// <summary>
    ///     Checks whether a given expression is a variable name.
    /// </summary>
    /// <param name="expression">expression</param>
    /// <returns>flag true/false</returns>
    public static bool IsVariableName(string expression)
    {
        if (string.IsNullOrEmpty(expression)) return false;

        return expression.StartsWith(AgenixSettings.VariablePrefix) &&
               expression.EndsWith(AgenixSettings.VariableSuffix);
    }
    
    /// <summary>
    /// Cut off single quotes prefix and suffix.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string CutOffSingleQuotes(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        if (input.Length >= 2 && input[0] == '\'' && input[^1] == '\'')
        {
            return input.Substring(1, input.Length - 2);
        }

        return input;
    }
    
    /// <summary>
    /// Cut off double quotes prefix and suffix.
    /// </summary>
    /// <param name="variable"></param>
    /// <returns></returns>
    public static string CutOffDoubleQuotes(string variable)
    {
        if (!string.IsNullOrWhiteSpace(variable) &&
            variable.Length > 1 && variable[0] == '"' && variable[^1] == '"')
        {
            return variable.Substring(1, variable.Length - 2);
        }

        return variable;
    }
}