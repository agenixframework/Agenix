using System;

namespace Agenix.Core.Annotations;

/// <summary>
///     Represents an attribute that can be applied to a field to specify
///     additional metadata for Agenix endpoint properties.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class AgenixEndpointPropertyAttribute : Attribute
{
    public AgenixEndpointPropertyAttribute(string name, string value, Type propertyType)
    {
        Name = name;
        Value = value;
        Type = propertyType;
    }

    public AgenixEndpointPropertyAttribute(string name, string value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    ///     Property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Property value.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     Property type.
    /// </summary>
    public Type Type { get; set; } = typeof(string);
}