using Agenix.Core.Message;

namespace Agenix.Core.Endpoint.Adapter.Mapping;

/// <summary>
///     Interface describes mapping extraction along message dispatching endpoint adapter processing steps. Extractor
///     is supposed to read mapping name predicate from request (e.g. via XPath, header value, etc.). Dispatching
///     endpoint adapters
///     may then dispatch message processing according to this mapping name.
/// </summary>
public interface IMappingKeyExtractor
{
    /// Extracts the mapping key from incoming request message.
    /// @param request The incoming request message from which the mapping key is to be extracted.
    /// @return The mapping key extracted from the request message.
    /// /
    string ExtractMappingKey(IMessage request);
}