namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Defines a converter that can convert an object to a specified type.
/// </summary>
/// <typeparam name="TO">The target type to convert to</typeparam>
public interface IConverter<TO>
{
    /// <summary>
    /// Converts the provided value to the target type.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The converted value</returns>
    TO Convert(object value);
}
