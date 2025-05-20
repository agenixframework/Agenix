using System;
using Agenix.Api.Annotations;

namespace Agenix.Core.Endpoint.Direct.Annotation;

/// <summary>
///     An attribute to configure the properties of a direct sync endpoint for
///     use within the Agenix.Core.Config.Annotation namespace.
/// </summary>
/// <remarks>
///     This attribute is used to configure a direct sync endpoint with properties
///     such as the qualifier, queue name, polling interval, correlator, and timeout.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
[AgenixEndpointConfig("direct.sync")]
public class DirectSyncEndpointConfigAttribute : Attribute
{
    /// <summary>
    ///     Qualifier for the endpoint configuration.
    /// </summary>
    public string Qualifier { get; set; } = "direct.sync";

    /// <summary>
    ///     Destination name.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    ///     Destination reference.
    /// </summary>
    public string Queue { get; set; } = string.Empty;

    /// <summary>
    ///     Polling interval.
    /// </summary>
    public int PollingInterval { get; set; } = 500;

    /// <summary>
    ///     Message correlator.
    /// </summary>
    public string Correlator { get; set; } = string.Empty;

    /// <summary>
    ///     Timeout.
    /// </summary>
    public long Timeout { get; set; } = 5000L;
}