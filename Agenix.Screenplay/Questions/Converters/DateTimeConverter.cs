namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Converts objects to DateTime values.
/// </summary>
public class DateTimeConverter : IConverter<DateTime>
{
    /// <summary>
    /// Converts the provided value to a DateTime.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The DateTime representation of the value</returns>
    public DateTime Convert(object value)
    {
        return DateTime.Parse(value.ToString());
    }
}
