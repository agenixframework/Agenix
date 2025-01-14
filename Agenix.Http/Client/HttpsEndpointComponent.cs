namespace Agenix.Http.Client;

/// <summary>
/// Component creates proper HTTP client endpoint from endpoint uri resource and parameters.
/// </summary>
public class HttpsEndpointComponent() : HttpEndpointComponent("https")
{
    protected override string Scheme => "https://";
}