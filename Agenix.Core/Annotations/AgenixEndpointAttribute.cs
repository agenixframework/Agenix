using System;
using System.Collections.Generic;
using System.Reflection;

namespace Agenix.Core.Annotations;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
public class AgenixEndpointAttribute : Attribute
{
    public AgenixEndpointAttribute()
    {
    }

    public AgenixEndpointAttribute(string name, params string[] properties)
    {
        Name = name;
        Properties = ParseProperties(properties);
    }

    /// <summary>
    ///     Endpoint name usually referencing a Spring bean id.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    ///     Endpoint properties.
    /// </summary>
    public AgenixEndpointPropertyAttribute[] Properties { get; set; } = Array.Empty<AgenixEndpointPropertyAttribute>();

    private static AgenixEndpointPropertyAttribute[] ParseProperties(string[] properties)
    {
        var props = new List<AgenixEndpointPropertyAttribute>();

        foreach (var prop in properties)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var parts = prop.Split([':'], 3);
            var name = parts[0];
            var value = parts[1];
            var resultedType = parts.Length > 2
                ? Type.GetType(parts[2]) != null ? Type.GetType(parts[2]) : executingAssembly.GetType(parts[2])
                : typeof(string);
            props.Add(new AgenixEndpointPropertyAttribute(name, value, resultedType));
        }

        return props.ToArray();
    }
}