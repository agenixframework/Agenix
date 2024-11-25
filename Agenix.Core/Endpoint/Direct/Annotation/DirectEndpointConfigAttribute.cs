using System;
using Agenix.Core.Annotations;

namespace Agenix.Core.Endpoint.Direct.Annotation;

/// <summary>
///     The DirectEndpointConfigAttribute is used to configure direct endpoint settings for a specific field.
///     This attribute is non-inheritable and targets field declarations.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
[AgenixEndpointConfig("direct.async")]
public class DirectEndpointConfigAttribute : Attribute
{
    /// <summary>
    ///     Destination name.
    /// </summary>
    public string QueueName { get; set; } = "";

    /// <summary>
    ///     Destination reference.
    /// </summary>
    public string Queue { get; set; } = "";

    /// <summary>
    ///     Timeout.
    /// </summary>
    public long Timeout { get; set; } = 5000L;
}