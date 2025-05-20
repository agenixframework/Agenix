namespace Agenix.Api.Common;

/// <summary>
/// Represents an interface for assigning and managing a name for a component.
/// This interface is intended to provide a mechanism to set the name
/// of a class or object implementing it and serves as a common abstraction
/// for named entities across the system.
/// </summary>
public interface INamed
{
    /// Sets the name of the component.
    /// @param name The name to be assigned to the component.
    /// /
    void SetName(string name);
}