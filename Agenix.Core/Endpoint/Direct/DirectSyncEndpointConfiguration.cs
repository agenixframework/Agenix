using Agenix.Core.Message;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     Configuration for Direct Sync Endpoints. Inherits from DirectEndpointConfiguration and implements
///     IPollableEndpointConfiguration.
///     Provides settings for polling interval and message correlator.
/// </summary>
public class DirectSyncEndpointConfiguration : DirectEndpointConfiguration, IPollableEndpointConfiguration
{
    /// Correlates messages for synchronous reply handling
    /// /
    public IMessageCorrelator Correlator { get; set; } = new DefaultMessageCorrelator();

    /// Polling interval when waiting for a synchronous reply message to arrive
    public long PollingInterval { get; set; } = 500;
}