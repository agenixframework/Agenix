namespace FleetPay.Core.Endpoint
{
    public interface IEndpointBuilder<out T> where T : IEndpoint
    {
        /// <summary>
        ///     Builds the endpoint.
        /// </summary>
        /// <returns></returns>
        T Build();
    }
}