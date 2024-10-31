using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Core.Util;

public class JsonPathUtils
{
    /// <summary>
    ///     Evaluate JsonPath expression on given payload string and return result as object.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="jsonPathExpression"></param>
    /// <returns></returns>
    public static object Evaluate(string payload, string jsonPathExpression)
    {
        try
        {
            var parser = JToken.Parse(payload);
            return Evaluate(parser, jsonPathExpression);
        }
        catch (JsonReaderException e)
        {
            throw new CoreSystemException("Failed to parse JSON text", e);
        }
    }

    /// <summary>
    ///     Evaluate JsonPath expression on given payload string and return result as object.
    /// </summary>
    /// <param name="readerContext"></param>
    /// <param name="jsonPathExpression"></param>
    /// <returns></returns>
    public static object Evaluate(JToken readerContext, string jsonPathExpression)
    {
        var expression = jsonPathExpression;
        string jsonPathFunction = null;
        foreach (var name in JsonPathFunctions.GetSupportedFunctions())
        {
            if (!expression.EndsWith($".{name}()")) continue;
            jsonPathFunction = name;
            expression = expression.Substring(0, expression.Length - $".{name}()".Length);
            break;
        }

        var jsonPathResults = new List<object>();

        try
        {
            var jTokens = readerContext.SelectTokens(expression, true);

            foreach (var jToken in jTokens)
            {
                object jsonPathResult = jToken;

                if (!string.IsNullOrEmpty(jsonPathFunction))
                    jsonPathResult = JsonPathFunctions.Evaluate(jToken, jsonPathFunction);

                jsonPathResults.Add(jsonPathResult);
            }
        }
        catch (JsonException e)
        {
            throw new CoreSystemException(
                $"Failed to evaluate JSON path expression: {jsonPathExpression}", e);
        }

        return jsonPathResults;
    }

    /// <summary>
    ///     Evaluate JsonPath expression on given payload string and return result as string.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="jsonPathExpression"></param>
    /// <returns></returns>
    public static string EvaluateAsString(string payload, string jsonPathExpression)
    {
        try
        {
            var parser = JToken.Parse(payload);
            return EvaluateAsString(parser, jsonPathExpression);
        }
        catch (JsonReaderException e)
        {
            throw new CoreSystemException("Failed to parse JSON text", e);
        }
    }

    /// <summary>
    ///     Evaluate JsonPath expression on given payload string and return result as string.
    /// </summary>
    /// <param name="readerContext"></param>
    /// <param name="jsonPathExpression"></param>
    /// <returns></returns>
    public static string EvaluateAsString(JToken readerContext, string jsonPathExpression)
    {
        var jsonPathResults = Evaluate(readerContext, jsonPathExpression);
        var resultOfStrings = new List<string>();

        foreach (var jsonPathResult in (List<object>)jsonPathResults)
            if (jsonPathResult.GetType() == typeof(JObject))
                resultOfStrings.Add(((JObject)jsonPathResult).ToString(Formatting.None));
            else if (jsonPathResult.GetType() == typeof(JArray))
                resultOfStrings.Add(((JArray)jsonPathResult).ToString(Formatting.None));
            else
                resultOfStrings.Add(string.Join(", ", jsonPathResult));

        return string.Join(", ", resultOfStrings);
    }
}