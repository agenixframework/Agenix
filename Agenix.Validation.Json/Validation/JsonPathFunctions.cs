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
                    return ((JArray)jsonPathResult).Count;
                if (jsonPathResult.GetType() == typeof(JObject))
                    return ((JObject)jsonPathResult).Count;
                return jsonPathResult != null ? 1 : 0;
            case "KeySet":
                if (jsonPathResult.GetType() == typeof(JObject))
                    return string.Join(", ",
                        ((JObject)jsonPathResult).Properties().Select(p => p.Name).ToHashSet());
                return new HashSet<string>();
            case "Values":
                if (jsonPathResult.GetType() == typeof(JObject))
                {
                    object[] valueObjects = ((JObject)jsonPathResult).Values().ToArray();
                    var values = new List<string>(valueObjects.Length);
                    foreach (var value in valueObjects)
                        if (value.GetType() == typeof(JObject))
                            values.Add(((JObject)value).ToString(Formatting.None));
                        else if (value.GetType() == typeof(JArray))
                            values.Add(((JArray)value).ToString(Formatting.None));
                        else
                            values.Add(Convert.ToString(value));

                    return string.Join(", ", values);
                }

                return Array.Empty<object>();
            case "ToString":
                if (jsonPathResult.GetType() == typeof(JArray))
                    return ((JArray)jsonPathResult).ToString(Formatting.None);
                if (jsonPathResult.GetType() ==
                    typeof(JObject))
                    return ((JObject)jsonPathResult).ToString(Formatting.None);
                return jsonPathResult.ToString();
        }

        return jsonPathResult;
    }
}
