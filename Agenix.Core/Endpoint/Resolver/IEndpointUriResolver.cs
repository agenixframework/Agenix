using Agenix.Core.Message;

namespace Agenix.Core.Endpoint.Resolver;

/// <summary>
///     Resolves endpoint uri so we can send messages to dynamic endpoints. Resolver works on request message and chooses
///     the target message endpoint according to message headers or payload.
/// </summary>
public interface IEndpointUriResolver
{
    /// <summary>
    ///     Static header entry name specifying the dynamic endpoint URI.
    /// </summary>
    static readonly string EndpointUriHeaderName = MessageHeaders.Prefix + "endpoint_uri";

    static readonly string RequestPathHeaderName = MessageHeaders.Prefix + "request_path";
    static readonly string QueryParamHeaderName = MessageHeaders.Prefix + "query_params";

    /// <summary>
    ///     Get the dedicated message endpoint URI for this message.
    /// </summary>
    /// <param name="message">The request message to send.</param>
    /// <param name="defaultUri">The fallback URI in case no mapping was found.</param>
    /// <returns>The endpoint URI string representation.</returns>
    string ResolveEndpointUri(IMessage message, string defaultUri);
}