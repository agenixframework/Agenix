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

using Agenix.Api.Exceptions;
using Agenix.Validation.Json.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Json;

public static class JsonPathUtils
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
            throw new AgenixSystemException("Failed to parse JSON text", e);
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
            if (!expression.EndsWith($".{name}()"))
            {
                continue;
            }

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
                {
                    jsonPathResult = JsonPathFunctions.Evaluate(jToken, jsonPathFunction);
                }

                jsonPathResults.Add(jsonPathResult);
            }
        }
        catch (JsonException e)
        {
            throw new AgenixSystemException(
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
            throw new AgenixSystemException("Failed to parse JSON text", e);
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
        {
            if (jsonPathResult.GetType() == typeof(JObject))
            {
                resultOfStrings.Add(((JObject)jsonPathResult).ToString(Formatting.None));
            }
            else if (jsonPathResult.GetType() == typeof(JArray))
            {
                resultOfStrings.Add(((JArray)jsonPathResult).ToString(Formatting.None));
            }
            else
            {
                resultOfStrings.Add(string.Join(", ", jsonPathResult));
            }
        }

        return string.Join(", ", resultOfStrings);
    }
}
