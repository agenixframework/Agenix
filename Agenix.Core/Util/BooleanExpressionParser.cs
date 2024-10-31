using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agenix.Core.Exceptions;
using log4net;

namespace Agenix.Core.Util;

/**
* Parses boolean expression strings and evaluates to boolean result.
*/
public class BooleanExpressionParser
{
    /**
    * List of known non-boolean operators
    */
    private static readonly List<string> Operators = ["lt", "lt=", "gt", "gt=", "<", "<=", ">", ">="];

    /**
    * List of known boolean operators
    */
    private static readonly List<string> BooleanOperators = ["=", "and", "or"];

    /**
    * List of known boolean values
    */
    private static readonly List<string> BooleanValues = [true.ToString(), false.ToString()];

    /**
    * Logger
    */
    private static readonly ILog _log = LogManager.GetLogger(typeof(BooleanExpressionParser));

    /**
    * Prevent instantiation.
    */
    private BooleanExpressionParser()
    {
    }

    /**
     * Perform evaluation of boolean expression string.
     * 
     * @param expression The expression to evaluate
     * @return boolean result
     * @throws CitrusRuntimeException When unable to parse expression
     */
    public static bool Evaluate(string expression)
    {
        var operators = new Stack<string>();
        var values = new Stack<string>();
        bool result;

        var currentCharacterIndex = 0;

        try
        {
            while (currentCharacterIndex < expression.Length)
            {
                var currentCharacter = expression[currentCharacterIndex];
                switch (currentCharacter)
                {
                    case (char)SeparatorToken.OPEN_PARENTHESIS:
                        operators.Push(((char)SeparatorToken.OPEN_PARENTHESIS).ToString());
                        currentCharacterIndex += MoveCursor(((char)SeparatorToken.OPEN_PARENTHESIS).ToString());
                        break;
                    case (char)SeparatorToken.SPACE:
                        currentCharacterIndex += MoveCursor(((char)SeparatorToken.SPACE).ToString());
                        break;
                    case (char)SeparatorToken.CLOSE_PARENTHESIS:
                        EvaluateSubexpression(operators, values);
                        currentCharacterIndex += MoveCursor(((char)SeparatorToken.CLOSE_PARENTHESIS).ToString());
                        break;
                    default:
                    {
                        if (!char.IsDigit(currentCharacter))
                        {
                            var parsedNonDigit = ParseNonDigits(expression, currentCharacterIndex);
                            if (IsBoolean(parsedNonDigit))
                                values.Push(ReplaceBooleanStringByIntegerRepresentation(parsedNonDigit));
                            else
                                operators.Push(ValidateOperator(parsedNonDigit));
                            currentCharacterIndex += MoveCursor(parsedNonDigit);
                        }
                        else if (char.IsDigit(currentCharacter))
                        {
                            var parsedDigits = ParseDigits(expression, currentCharacterIndex);
                            values.Push(parsedDigits);
                            currentCharacterIndex += MoveCursor(parsedDigits);
                        }

                        break;
                    }
                }
            }

            result = bool.Parse(EvaluateExpressionStack(operators, values));
            if (_log.IsDebugEnabled)
                _log.Debug($"Boolean expression {expression} evaluates to {result}");
        }
        catch (InvalidOperationException e)
        {
            throw new CoreSystemException(
                $"Unable to parse boolean expression '{expression}'. Maybe expression is incomplete!", e);
        }

        return result;
    }

    /**
     * This method takes stacks of operators and values and evaluates possible expressions
     * This is done by popping one operator and two values, applying the operator to the values and pushing the result back onto the value stack
     * 
     * @param operators Operators to apply
     * @param values    Values
     * @return The final result popped of the values stack
     */
    private static string EvaluateExpressionStack(Stack<string> operators, Stack<string> values)
    {
        while (operators.Count != 0)
        {
            var rightOperand = values.Pop();
            var leftOperand = values.Pop();
            var operatorValue = operators.Pop();
            values.Push(GetBooleanResultAsString(operatorValue, rightOperand, leftOperand));
        }

        return ReplaceIntegerStringByBooleanRepresentation(values.Pop());
    }

    /**
     * Evaluates a sub expression within a pair of parentheses and pushes its result onto the stack of values
     * 
     * @param operators Stack of operators
     * @param values    Stack of values
     */
    private static void EvaluateSubexpression(Stack<string> operators, Stack<string> values)
    {
        var operatorValue = operators.Pop();
        while (!operatorValue.Equals(((char)SeparatorToken.OPEN_PARENTHESIS).ToString()))
        {
            var rightOperand = values.Pop();
            var leftOperand = values.Pop();
            values.Push(GetBooleanResultAsString(operatorValue, rightOperand, leftOperand));
            operatorValue = operators.Pop();
        }
    }

    /**
     * This method reads digit characters from a given string, starting at a given index.
     * It will read till the end of the string or up until it encounters a non-digit character
     * 
     * @param expression The string to parse
     * @param startIndex The start index from where to parse
     * @return The parsed substring
     */
    private static string ParseDigits(string expression, int startIndex)
    {
        var digitBuffer = new StringBuilder();
        var currentCharacter = expression[startIndex];
        var subExpressionIndex = startIndex;

        do
        {
            digitBuffer.Append(currentCharacter);
            ++subExpressionIndex;

            if (subExpressionIndex < expression.Length) currentCharacter = expression[subExpressionIndex];
        } while (subExpressionIndex < expression.Length && char.IsDigit(currentCharacter));

        return digitBuffer.ToString();
    }

    /**
     * This method reads non-digit characters from a given string, starting at a given index.
     * It will read till the end of the string or up until it encounters
     * - a digit
     * - a separator token
     * 
     * @param expression The string to parse
     * @param startIndex The start index from where to parse
     * @return The parsed substring
     */
    private static string ParseNonDigits(string expression, int startIndex)
    {
        var operatorBuffer = new StringBuilder();
        var currentCharacter = expression[startIndex];
        var subExpressionIndex = startIndex;
        do
        {
            operatorBuffer.Append(currentCharacter);
            subExpressionIndex++;

            if (subExpressionIndex < expression.Length) currentCharacter = expression[subExpressionIndex];
        } while (subExpressionIndex < expression.Length && !char.IsDigit(currentCharacter) &&
                 !IsSeparatorToken(currentCharacter));

        return operatorBuffer.ToString();
    }

    /**
     * Checks whether a string can be interpreted as a boolean value.
     * 
     * @param possibleBoolean The possible boolean value as string
     * @return Either true or false
     */
    private static bool IsBoolean(string possibleBoolean)
    {
        return BooleanValues.Contains(possibleBoolean, StringComparer.OrdinalIgnoreCase);
    }

    /**
     * Checks whether a String is a Boolean value and replaces it with its Integer representation
     * "true" -> "1"
     * "false" -> "0"
     * 
     * @param possibleBooleanString "true" or "false"
     * @return "1" or "0"
     */
    private static string ReplaceBooleanStringByIntegerRepresentation(string possibleBooleanString)
    {
        if (possibleBooleanString.Equals(true.ToString(), StringComparison.OrdinalIgnoreCase)) return "1";

        return possibleBooleanString.Equals(false.ToString(), StringComparison.OrdinalIgnoreCase)
            ? "0"
            : possibleBooleanString;
    }

    /**
     * Counterpart of {@link #replaceBooleanStringByIntegerRepresentation}
     * Checks whether a String is the Integer representation of a Boolean value and replaces it with its Boolean representation
     * "1" -> "true"
     * "0" -> "false"
     * otherwise -> value
     * 
     * @param value "1", "0" or other string
     * @return "true", "false" or the input value
     */
    private static string ReplaceIntegerStringByBooleanRepresentation(string value)
    {
        return value switch
        {
            "0" => false.ToString(),
            "1" => true.ToString(),
            _ => value
        };
    }

    /**
     * Checks whether a given character is a known separator token or no
     * 
     * @param possibleSeparatorChar The character to check
     * @return True in case its a separator, false otherwise
     */
    private static bool IsSeparatorToken(char possibleSeparatorChar)
    {
        foreach (SeparatorToken token in Enum.GetValues(typeof(SeparatorToken)))
            if ((char)token == possibleSeparatorChar)
                return true;
        return false;
    }

    /**
     * Check if operator is known to this class.
     * 
     * @param operator to validate
     * @return the operator itself.
     * @throws CitrusRuntimeException When encountering an unknown operator
     */
    private static string ValidateOperator(string operatorValue)
    {
        if (!Operators.Contains(operatorValue) && !BooleanOperators.Contains(operatorValue))
            throw new CoreSystemException("Unknown operator '" + operatorValue + "'");
        return operatorValue;
    }

    /**
     * Returns the amount of characters to move the cursor after parsing a token
     * 
     * @param lastToken Last parsed token
     * @return Amount of characters to move forward
     */
    private static int MoveCursor(string lastToken)
    {
        return lastToken.Length;
    }

    /**
     * Evaluates a boolean expression to a String representation (true/false).
     * 
     * @param operatorValue     The operator to apply on operands
     * @param rightOperand The right hand side of the expression
     * @param leftOperand  The left hand side of the expression
     * @return true/false as String
     */
    private static string GetBooleanResultAsString(string operatorValue, string rightOperand, string leftOperand)
    {
        return operatorValue switch
        {
            "lt" or "<" => (int.Parse(leftOperand) < int.Parse(rightOperand)).ToString(),
            "lt=" or "<=" => (int.Parse(leftOperand) <= int.Parse(rightOperand)).ToString(),
            "gt" or ">" => (int.Parse(leftOperand) > int.Parse(rightOperand)).ToString(),
            "gt=" or ">=" => (int.Parse(leftOperand) >= int.Parse(rightOperand)).ToString(),
            "=" => (int.Parse(leftOperand) == int.Parse(rightOperand)).ToString(),
            "and" => (bool.Parse(leftOperand) && bool.Parse(rightOperand)).ToString(),
            "or" => (bool.Parse(leftOperand) || bool.Parse(rightOperand)).ToString(),
            _ => throw new CoreSystemException("Unknown operator '" + operatorValue + "'")
        };
    }

    /**
    * SeparatorToken is an explicit type to identify different kinds of separators.
    */
    private enum SeparatorToken
    {
        SPACE = ' ',
        OPEN_PARENTHESIS = '(',
        CLOSE_PARENTHESIS = ')'
    }
}