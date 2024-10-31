namespace Agenix.Core.Endpoint;

/// <summary>
///     Abstract pollable endpoint configuration adds polling interval settings.
/// </summary>
public abstract class AbstractPollableEndpointConfiguration : AbstractEndpointConfiguration,
    IPollableEndpointConfiguration
{
    /// <summary>
    ///     Polling interval when waiting for synchronous reply message to arrive
    /// </summary>
    public long PollingInterval { get; set; } = 500L;
}