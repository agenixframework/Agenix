namespace Agenix.Screenplay;

/// <summary>
/// Interface for entities that contain custom field values.
/// </summary>
public interface IHasCustomFieldValues
{
    /// <summary>
    /// Gets a dictionary of custom field values where the key is the field name and the value is the field value.
    /// </summary>
    /// <returns>A dictionary containing custom field values.</returns>
    IDictionary<string, object> CustomFieldValues { get; }
}
