namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Converts objects to long values.
/// </summary>
public class LongConverter : IConverter<long>
{
    /// <summary>
    /// Converts the provided value to a long.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The long representation of the value</returns>
    public long Convert(object value)
    {
        return long.Parse(value.ToString());
    }
}
