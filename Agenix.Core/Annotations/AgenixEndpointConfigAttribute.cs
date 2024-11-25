using System;

namespace Agenix.Core.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class AgenixEndpointConfigAttribute(string qualifier) : Attribute
{
    /// <summary>
    ///     Endpoint configuration qualifier.
    /// </summary>
    public string Qualifier { get; set; } = qualifier;
}