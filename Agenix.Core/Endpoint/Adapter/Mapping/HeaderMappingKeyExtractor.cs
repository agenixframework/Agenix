using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Core.Message;

namespace Agenix.Core.Endpoint.Adapter.Mapping;

/// <summary>
///     This class is responsible for extracting a mapping key from the headers of an incoming message.
/// </summary>
public class HeaderMappingKeyExtractor : AbstractMappingKeyExtractor
{
    /// Represents the header name used for mapping key extraction.
    /// /
    private string _headerName = "";

    /// <summary>
    ///     This class is responsible for extracting a mapping key from the headers of an incoming message.
    /// </summary>
    public HeaderMappingKeyExtractor()
    {
    }

    /// This class is responsible for extracting a mapping key from the headers of an incoming message.
    /// /
    public HeaderMappingKeyExtractor(string headerName)
    {
        _headerName = headerName;
    }

    /// <summary>
    ///     Extracts a mapping key from the headers of the given message.
    /// </summary>
    /// <param name="request">The incoming message from which the header mapping key is to be extracted.</param>
    /// <returns>The extracted mapping key as a string.</returns>
    /// <exception cref="AgenixSystemException">Thrown when the specified header is not found in the request message.</exception>
    protected override string GetMappingKey(IMessage request)
    {
        if (request.GetHeader(_headerName) != null) return request.GetHeader(_headerName)?.ToString();

        throw new AgenixSystemException($"Unable to find header '{_headerName}' in request message");
    }

    /// <summary>
    ///     Sets the header name used for mapping key extraction.
    /// </summary>
    /// <param name="headerName">The name of the header to be used for mapping key extraction.</param>
    public void SetHeaderName(string headerName)
    {
        _headerName = headerName;
    }
}