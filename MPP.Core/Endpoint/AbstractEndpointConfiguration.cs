namespace FleetPay.Core.Endpoint
{
    /// <summary>
    ///     Abstract endpoint configuration provides basic properties such as message listeners.
    /// </summary>
    public abstract class AbstractEndpointConfiguration : IEndpointConfiguration
    {
        /// <summary>
        ///     Gets/ Sets the timeout for sending and receiving messages. Initial default timeout 5 seconds.
        /// </summary>
        public long Timeout { get; set; } = 5000L;
    }
}