namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Converts objects to boolean values.
/// </summary>
public class BooleanConverter : IConverter<bool>
{
    /// <summary>
    /// Converts the provided value to a boolean.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The boolean representation of the value</returns>
    public bool Convert(object value)
    {
        return bool.Parse(value.ToString().ToLower());
    }
}
