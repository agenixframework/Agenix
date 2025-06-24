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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     Custom JsonPath function support for size(), keySet() and toString() operations on Json objects and arrays.
/// </summary>
public abstract class JsonPathFunctions
{
    private static readonly string[] FunctionNames = ["KeySet", "Size", "Values", "ToString", "Exists"];

    /// <summary>
    ///     Gets names of supported functions.
    /// </summary>
    /// <returns></returns>
    public static string[] GetSupportedFunctions()
    {
        var copy = new string[FunctionNames.Length];
        Array.Copy(FunctionNames, copy, FunctionNames.Length);
        return copy;
    }

    /// <summary>
    ///     Evaluates function on a result. Supported functions are size(), keySet(), values() and toString().
    /// </summary>
    /// <param name="jsonPathResult"></param>
    /// <param name="jsonPathFunction"></param>
    /// <returns></returns>
    public static object Evaluate(object jsonPathResult, string jsonPathFunction)
    {
        switch (jsonPathFunction)
        {
            case "Exists":
                return jsonPathResult != null;
            case "Size":
                if (jsonPathResult.GetType() == typeof(JArray))
                {
                    return ((JArray)jsonPathResult).Count;
                }

                if (jsonPathResult.GetType() == typeof(JObject))
                {
                    return ((JObject)jsonPathResult).Count;
                }

                return jsonPathResult != null ? 1 : 0;
            case "KeySet":
                if (jsonPathResult.GetType() == typeof(JObject))
                {
                    return string.Join(", ",
                        ((JObject)jsonPathResult).Properties().Select(p => p.Name).ToHashSet());
                }

                return new HashSet<string>();
            case "Values":
                if (jsonPathResult.GetType() == typeof(JObject))
                {
                    object[] valueObjects = ((JObject)jsonPathResult).Values().ToArray();
                    var values = new List<string>(valueObjects.Length);
                    foreach (var value in valueObjects)
                    {
                        if (value.GetType() == typeof(JObject))
                        {
                            values.Add(((JObject)value).ToString(Formatting.None));
                        }
                        else if (value.GetType() == typeof(JArray))
                        {
                            values.Add(((JArray)value).ToString(Formatting.None));
                        }
                        else
                        {
                            values.Add(Convert.ToString(value));
                        }
                    }

                    return string.Join(", ", values);
                }

                return Array.Empty<object>();
            case "ToString":
                if (jsonPathResult.GetType() == typeof(JArray))
                {
                    return ((JArray)jsonPathResult).ToString(Formatting.None);
                }

                if (jsonPathResult.GetType() ==
                    typeof(JObject))
                {
                    return ((JObject)jsonPathResult).ToString(Formatting.None);
                }

                return jsonPathResult.ToString();
        }

        return jsonPathResult;
    }
}
