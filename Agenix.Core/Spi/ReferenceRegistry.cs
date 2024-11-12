namespace Agenix.Core.Spi;

/**
 * Bind objects to registry for later reference. Objects declared in registry can be injected in various ways (e.g. annotations).
 */
public interface IReferenceRegistry
{
    void Bind(string name, object value);
}

/// ReferenceRegistry is responsible for binding objects to a registry for later reference.
/// Declared objects can be injected in multiple ways, such as through annotations.
/// /
public class ReferenceRegistry : IReferenceRegistry
{
    public void Bind(string name, object value)
    {
        //implement the bind logic here
    }

    /// Get proper bean name for future bind operation on registry.
    /// @param bindAnnotation The annotation containing binding information.
    /// @param defaultName The default name to use if the annotation does not provide a name.
    /// @return The name to use for binding in the registry.
    /// /
    public static string GetName(BindToRegistry bindAnnotation, string defaultName)
    {
        return !string.IsNullOrEmpty(bindAnnotation.Name) ? bindAnnotation.Name : defaultName;
    }
}