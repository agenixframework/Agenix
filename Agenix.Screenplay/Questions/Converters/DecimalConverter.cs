namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Converts objects to decimal values.
/// </summary>
public class DecimalConverter : IConverter<decimal>
{
    /// <summary>
    /// Converts the provided value to a decimal.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The decimal representation of the value</returns>
    public decimal Convert(object value)
    {
        return decimal.Parse(value.ToString());
    }
}
