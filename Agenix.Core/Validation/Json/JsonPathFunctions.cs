using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Core.Validation.Json
{
    /// <summary>
    ///     Custom JsonPath function support for size(), keySet() and toString() operations on Json objects and arrays.
    /// </summary>
    public class JsonPathFunctions
    {
        private static readonly string[] FunctionNames = { "KeySet", "Size", "Values", "ToString", "Exists" };

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
        ///     Evaluates function on result. Supported functions are size(), keySet(), values() and toString().
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
                    else if (jsonPathResult.GetType() == typeof(JObject))
                        return ((JObject)jsonPathResult).Count;
                    else
                        return jsonPathResult != null ? 1 : 0;
                case "KeySet":
                    if (jsonPathResult.GetType() == typeof(JObject))
                        return string.Join(", ",
                            ((JObject)jsonPathResult).Properties().Select(p => p.Name).ToHashSet());
                    else
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
                    else
                    {
                        return Array.Empty<object>();
                    }
                case "ToString":
                    if (jsonPathResult.GetType() == typeof(JArray))
                        return ((JArray)jsonPathResult).ToString(Formatting.None);
                    else if (jsonPathResult.GetType() ==
                             typeof(JObject))
                        return ((JObject)jsonPathResult).ToString(Formatting.None);
                    else
                        return jsonPathResult.ToString();
            }

            return jsonPathResult;
        }
    }
}