namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Converts objects to integer values.
/// </summary>
public class IntegerConverter : IConverter<int>
{
    /// <summary>
    /// Converts the provided value to an integer.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The integer representation of the value</returns>
    public int Convert(object value)
    {
        return int.Parse(value.ToString());
    }
}
