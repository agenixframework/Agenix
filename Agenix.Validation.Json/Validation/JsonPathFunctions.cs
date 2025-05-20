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