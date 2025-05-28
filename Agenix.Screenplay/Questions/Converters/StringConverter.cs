namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Converts objects to string values.
/// </summary>
public class StringConverter : IConverter<string>
{
    /// <summary>
    /// Converts the provided value to a string.
    /// Returns an empty string if the value is null.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The string representation of the value, or empty string if null</returns>
    public string Convert(object value)
    {
        return value?.ToString() ?? string.Empty;
    }
}
