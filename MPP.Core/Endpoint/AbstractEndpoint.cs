namespace FleetPay.Core.Endpoint
{
    /// <summary>
    ///     Abstract message endpoint handles send/receive timeout setting
    /// </summary>
    public abstract class AbstractEndpoint : IEndpoint
    {
        /// <summary>
        ///     Default constructor using endpoint configuration.
        /// </summary>
        /// <param name="endpointConfiguration">the endpoint configuration</param>
        public AbstractEndpoint(IEndpointConfiguration endpointConfiguration)
        {
            EndpointConfiguration = endpointConfiguration;
        }

        /// <summary>
        ///     Gets the endpoints consumer name.
        /// </summary>
        public string ConsumerName => Name + ":consumer";

        /// <summary>
        ///     Gets the endpoints producer name.
        /// </summary>
        public string ProducerName => Name + ":producer";

        public IEndpointConfiguration EndpointConfiguration { get; }

        /// <summary>
        ///     Gets/ Sets the endpoints producer name.
        /// </summary>
        public string Name { get; set; } = nameof(AbstractEndpoint);
    }
}