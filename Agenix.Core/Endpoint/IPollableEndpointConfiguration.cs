namespace Agenix.Core.Endpoint
{
    /// <summary>
    ///     Extends endpoint configuration by adding polling interval settings.
    /// </summary>
    public interface IPollableEndpointConfiguration : IEndpointConfiguration
    {
        /// <summary>
        ///     Gets/ Sets the polling interval used on this endpoint configuration.
        /// </summary>
        long PollingInterval { get; set; }
    }
}