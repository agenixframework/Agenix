namespace Agenix.Core.Endpoint;

/// <summary>
///     Abstract message endpoint handles send/receive timeout setting
/// </summary>
/// <remarks>
///     Default constructor using endpoint configuration.
/// </remarks>
/// <param name="endpointConfiguration">the endpoint configuration</param>
public abstract class AbstractEndpoint(IEndpointConfiguration endpointConfiguration) : IEndpoint
{
    /// <summary>
    ///     Gets the endpoints consumer name.
    /// </summary>
    public string ConsumerName => Name + ":consumer";

    /// <summary>
    ///     Gets the endpoints producer name.
    /// </summary>
    public string ProducerName => Name + ":producer";

    public IEndpointConfiguration EndpointConfiguration { get; } = endpointConfiguration;

    /// <summary>
    ///     Gets/ Sets the endpoints producer name.
    /// </summary>
    public string Name { get; set; } = nameof(AbstractEndpoint);

    public void SetName(string name)
    {
        Name = name;
    }
}