namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Converts objects to double values.
/// </summary>
public class DoubleConverter : IConverter<double>
{
    /// <summary>
    /// Converts the provided value to a double.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The double representation of the value</returns>
    public double Convert(object value)
    {
        return double.Parse(value.ToString());
    }
}
