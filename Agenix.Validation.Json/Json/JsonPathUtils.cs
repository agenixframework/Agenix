#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
