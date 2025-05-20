using System.Text;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Validation.Json.Json;

namespace Agenix.Validation.Json.Functions.Core;

/// <summary>
/// Represents a function for evaluating JSONPath expressions on a given JSON source.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
public class JsonPathFunction : IFunction

{
    /// <summary>
    /// Executes a JSONPath query on a JSON string.
    /// </summary>
    /// <param name="parameterList">
    /// A list of strings where the first element is the JSON source and the second element
    /// is the JSONPath expression. If there are more than two elements, they will all be concatenated
    /// (except the last element, which is treated as the JSONPath expression).
    /// </param>
    /// <param name="context">
    /// The <see cref="TestContext"/> instance used for dynamic content replacement in the JSON source.
    /// </param>
    /// <returns>
    /// A string containing the result of evaluating the JSONPath expression against the JSON source.
    /// </returns>
    /// <exception cref="InvalidFunctionUsageException">
    /// Thrown when the parameter list is null, empty, or does not contain at least two elements.
    /// </exception>
    public string Execute(List<string> parameterList, TestContext context)
    {
        if (parameterList == null || parameterList.Count == 0)
            throw new InvalidFunctionUsageException("Function parameters must not be empty");

        if (parameterList.Count < 2)
            throw new InvalidFunctionUsageException(
                "Missing parameter for function - usage JsonPath('jsonSource','expression')");

        string jsonSource;
        string jsonPathExpression;

        if (parameterList.Count > 2)
        {
            var sb = new StringBuilder();
            sb.Append(parameterList[0]);

            for (var i = 1; i < parameterList.Count - 1; i++) sb.Append(", ").Append(parameterList[i]);

            jsonSource = sb.ToString();
            jsonPathExpression = parameterList[^1];
        }
        else
        {
            jsonSource = parameterList[0];
            jsonPathExpression = parameterList[1];
        }

        return JsonPathUtils.EvaluateAsString(context.ReplaceDynamicContentInString(jsonSource),
            jsonPathExpression);
    }
}