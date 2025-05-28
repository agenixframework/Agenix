namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Converts objects to float values.
/// </summary>
public class FloatConverter : IConverter<float>
{
    /// <summary>
    /// Converts the provided value to a float.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The float representation of the value</returns>
    public float Convert(object value)
    {
        return float.Parse(value.ToString());
    }
}
