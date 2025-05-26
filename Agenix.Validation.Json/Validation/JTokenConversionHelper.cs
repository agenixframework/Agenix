using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     Provides helper methods for converting JTokens to dynamic objects.
/// </summary>
public static class JTokenConversionHelper
{
    /// <summary>
    ///     Converts a given JToken to a dynamic object.
    /// </summary>
    /// <param name="token">The JToken to be converted.</param>
    /// <returns>A dynamic object that represents the JToken data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the JToken is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the JToken type is unsupported.</exception>
    public static object? ConvertToDynamic(JToken token)
    {
        if (token == null) throw new ArgumentNullException(nameof(token), "JToken cannot be null.");

        switch (token.Type)
        {
            case JTokenType.Object:
                return token.ToObject<Dictionary<string, object>?>();

            case JTokenType.Array:
                var array = token.ToObject<List<JToken>?>() ?? [];
                List<object> convertedList = [];
                convertedList.AddRange(array.Select(ConvertToDynamic)
                    .Where(item => item != null)
                    .Cast<object>());

                return convertedList;

            case JTokenType.Integer:
                return token.ToObject<int>();

            case JTokenType.Float:
                return token.ToObject<double>();

            case JTokenType.String:
                return token.ToObject<string>();

            case JTokenType.Boolean:
                return token.ToObject<bool>();

            case JTokenType.Null:
                return null;

            case JTokenType.Date:
                return token.ToObject<DateTime>();

            case JTokenType.TimeSpan:
                return token.ToObject<TimeSpan>();

            case JTokenType.Uri:
                return token.ToObject<Uri>();

            case JTokenType.Guid:
                return token.ToObject<Guid>();

            case JTokenType.None:
            case JTokenType.Constructor:
            case JTokenType.Property:
            case JTokenType.Comment:
            case JTokenType.Undefined:
            case JTokenType.Raw:
            case JTokenType.Bytes:
            default:
                throw new InvalidOperationException($"Unsupported JToken type: {token.Type}");
        }
    }
}