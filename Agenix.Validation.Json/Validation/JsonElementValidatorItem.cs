using System.Text.RegularExpressions;
using Agenix.Api.Exceptions;
using Agenix.Core.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     Wraps all needed data to validate an actual json with an expected json-template.
/// </summary>
/// <typeparam name="T">the type of the actual and expected json</typeparam>
public class JsonElementValidatorItem<T>
{
    public readonly T _actual;
    public readonly T _expected;
    private readonly int? _index;
    private readonly string _name;
    private object _parent;

    public JsonElementValidatorItem(int index, T actual, T expected)
    {
        _name = null;
        _index = index;
        _actual = actual;
        _expected = expected;
    }

    public JsonElementValidatorItem(string name, T actual, T expected)
    {
        _name = name;
        _index = null;
        _actual = actual;
        _expected = expected;
    }

    /// <summary>
    ///     Parses the provided actual and expected JSON strings into JsonElementValidatorItem objects.
    /// </summary>
    /// <param name="actualJson">The actual JSON string to parse.</param>
    /// <param name="expectedJson">The expected JSON string to parse.</param>
    /// <returns>
    ///     A JsonElementValidatorItem instance containing the parsed JSON objects from both actual and expected JSON strings.
    /// </returns>
    /// <exception cref="JsonReaderException">Thrown when parsing the JSON string fails.</exception>
    public static JsonElementValidatorItem<object> ParseJson(string actualJson, string expectedJson)
    {
        try
        {
            var actualJToken = JToken.Parse(actualJson);
            var expectedJToken = JToken.Parse(expectedJson);

            return new JsonElementValidatorItem<object>(null, actualJToken, expectedJToken);
        }
        catch (JsonReaderException e)
        {
            throw new AgenixSystemException("Failed to parse JSON text", e);
        }
    }

    /// <summary>
    ///     Retrieves all JSON paths that match the provided JSON path expression in the given JSON object.
    /// </summary>
    /// <param name="jsonPathExpression">The JSON path expression used to find matching paths.</param>
    /// <param name="json">The JSON object to search in.</param>
    /// <returns>A collection of strings representing the matched JSON paths.</returns>
    private static IEnumerable<string> GetAllMatchedPathsInJson(string jsonPathExpression, object json)
    {
        IList<string> foundJsonPaths;
        try
        {
            var jToken = JToken.FromObject(json);

            foundJsonPaths = jToken.SelectTokens(jsonPathExpression)
                .Select(token => NormalizePath(token.Path))
                .ToList();
        }
        catch (Exception e) when (e is JsonException or ArgumentException)
        {
            return [];
        }

        return foundJsonPaths;
    }

    private static string NormalizePath(string path)
    {
        if (!path.StartsWith("$")) path = "$" + path;

        // Replace dot notation with bracket notation for properties
        path = Regex.Replace(path, @"\.?([a-zA-Z_]\w*)", @"['$1']");

        // Ensure array indices remain properly formatted and unaffected
        return path;
    }

    /// <summary>
    ///     Ensures that the actual and expected properties are of the specified type <c>TU</c>.
    /// </summary>
    /// <typeparam name="TU">The expected type for the actual and expected properties.</typeparam>
    /// <returns>
    ///     A new instance of <c>JsonElementValidatorItem&lt;TU&gt;</c> if the types are as expected.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the actual or expected properties are not of the specified type <c>TU</c>.
    /// </exception>
    public JsonElementValidatorItem<TU> EnsureType<TU>() where TU : JToken
    {
        if (_expected is not TU expectedObj || _actual is not TU actualObj)
            throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                "Type mismatch for JSON entry '" + _name + "'",
                typeof(TU).Name,
                _actual == null ? null : _actual.GetType().Name
            ));
        var type = new JsonElementValidatorItem<TU>(_name, actualObj, expectedObj);
        type.Parent(this);
        return type;
    }

    /// <summary>
    ///     Creates a new JsonElementValidatorItem instance for a specified child element of the expected JSON array.
    /// </summary>
    /// <param name="expectedIndex">The index of the expected element in the JSON array.</param>
    /// <param name="other">The object to be used for the actual value of the new JsonElementValidatorItem instance.</param>
    /// <returns>
    ///     A new instance of JsonElementValidatorItem containing the specified child element.
    /// </returns>
    /// <exception cref="ValidationException">
    ///     Thrown when the types of the actual and expected values do not match the specified type parameter.
    /// </exception>
    public JsonElementValidatorItem<object> Child(int expectedIndex, object other)
    {
        var arrayControl = EnsureType<JArray>();

        var type = new JsonElementValidatorItem<object>(
            _name,
            other,
            arrayControl._expected[expectedIndex]);

        type.Parent(arrayControl);

        return type;
    }

    /// <summary>
    ///     Converts the actual value to a string if it is not null.
    /// </summary>
    /// <returns>
    ///     A string representation of the actual value, or null if the actual value is null.
    /// </returns>
    public string? ActualAsStringOrNull()
    {
        return _actual == null ? null : _actual.ToString();
    }

    /// <summary>
    ///     Converts the expected value to a string or returns null if the expected value is null.
    /// </summary>
    /// <returns>
    ///     The expected value as a string, or null if the expected value is null.
    /// </returns>
    public string? ExpectedAsStringOrNull()
    {
        return _expected == null ? null : _expected.ToString();
    }

    public JsonElementValidatorItem<T> Parent(JsonElementValidatorItem<T> parent)
    {
        _parent = parent;
        return this;
    }

    public JsonElementValidatorItem<T> Parent<U>(JsonElementValidatorItem<U> parent)
    {
        _parent = parent;
        return this;
    }

    /// <summary>
    ///     Returns the name or index of the current JsonElementValidatorItem.
    /// </summary>
    /// <returns>
    ///     A string representing the name if specified, the index if specified, or the default root symbol "$".
    /// </returns>
    public string GetName()
    {
        if (_index != null) return $"[{_index}]";
        return _name ?? "$";
    }


    /// <summary>
    ///     Retrieves the root JsonElementValidatorItem object in the hierarchy.
    /// </summary>
    /// <returns>
    ///     The root JsonElementValidatorItem object if the current object has a parent;
    ///     otherwise, returns the current JsonElementValidatorItem object.
    /// </returns>
    public JsonElementValidatorItem<T> GetRoot()
    {
        return _parent is not JsonElementValidatorItem<T> parent ? this : parent.GetRoot();
    }

    /// <summary>
    ///     The JSON path as a string from the root to this item, i.e., $['books'][1]['name']
    /// </summary>
    /// <returns></returns>
    public string GetJsonPath()
    {
        var parentPath = _parent == null ? "$" : ((dynamic)_parent).GetJsonPath();
        if (_index != null) return $"{parentPath}[{_index}]";
        return _name != null ? $"{parentPath}['{_name}']" : parentPath;
    }

    /// <summary>
    ///     Determines whether the specified JSON path is ignored by the current JsonElementValidatorItem object.
    /// </summary>
    /// <param name="jsonPathExpression">The JSON path expression to evaluate.</param>
    /// <returns>
    ///     true if the specified JSON path is ignored; otherwise, false.
    /// </returns>
    public bool IsPathIgnoredBy(string jsonPathExpression)
    {
        return IsPathIgnoredBy(jsonPathExpression, GetJsonPath(), GetRoot()._actual) ||
               IsPathIgnoredBy(jsonPathExpression, GetJsonPath(), GetRoot()._expected);
    }

    /// <summary>
    ///     Determines if the specified JSON path expression should ignore the given current path in the JSON object.
    /// </summary>
    /// <param name="jsonPathExpression">The JSON path expression to evaluate.</param>
    /// <param name="currentPath">The current JSON path to check against the expression.</param>
    /// <param name="json">The JSON object to evaluate the expression within.</param>
    /// <returns>
    ///     True if the current path matches any paths derived from the JSON path expression within the JSON object; otherwise,
    ///     false.
    /// </returns>
    public static bool IsPathIgnoredBy(string jsonPathExpression, string currentPath, object json)
    {
        return GetAllMatchedPathsInJson(jsonPathExpression, json).Any(path => path.Equals(currentPath));
    }
}