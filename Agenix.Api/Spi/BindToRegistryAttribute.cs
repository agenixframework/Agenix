using System;

namespace Agenix.Core.Spi;

/// Used to bind an object to the Agenix context reference registry for dependency injection reasons.
/// Object is bound with given name. In case no explicit name is given the registry will auto compute the name from given
/// Class name, method name or field name.
/// /
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
public class BindToRegistryAttribute : Attribute
{
    private string _name;

    /// Gets or sets the name used to bind the object to the Agenix context reference registry.
    /// If not explicitly set, the registry will auto compute the name from the class name, method name, or field name.
    public string Name
    {
        get => _name ?? string.Empty;
        set => _name = value;
    }
}