using Agenix.Api.Endpoint.Adapter.Mapping;
using Agenix.Api.Message;
using Agenix.Core.Message;

namespace Agenix.Core.Endpoint.Adapter.Mapping;

/// <summary>
///     Abstract mapping key extractor adds common mapping prefix and suffix added to evaluated mapping key. Subclasses do
///     evaluate mapping key from incoming request message and optional prefix and/ or suffix are automatically added to
///     resulting mapping key.
/// </summary>
public abstract class AbstractMappingKeyExtractor : IMappingKeyExtractor
{
    /// Represents an optional prefix that is automatically added to the extracted mapping key.
    /// /
    private string _mappingKeyPrefix = "";

    private string _mappingKeySuffix = "";

    /// Extracts the mapping key from the incoming request message, incorporating optional prefixes and suffixes.
    /// @param request The incoming request message.
    /// @return The extracted mapping key with optional prefix and suffix.
    /// /
    public string ExtractMappingKey(IMessage request)
    {
        return _mappingKeyPrefix + GetMappingKey(request) + _mappingKeySuffix;
    }

    /// Provides the mapping key from an incoming request message. Subclasses must implement this method.
    /// <param name="request">The incoming request message.</param>
    /// <return>The mapping key extracted from the request.</return>
    protected abstract string GetMappingKey(IMessage request);

    /// Sets the static mapping key prefix that will automatically be added to the extracted mapping key.
    /// @param mappingKeyPrefix The prefix to be added to the mapping key.
    /// /
    public void SetMappingKeyPrefix(string mappingKeyPrefix)
    {
        _mappingKeyPrefix = mappingKeyPrefix;
    }

    /// Sets the static mapping key suffix that will automatically be added to the extracted mapping key.
    /// <param name="mappingKeySuffix">The suffix to be added to the mapping key.</param>
    public void SetMappingKeySuffix(string mappingKeySuffix)
    {
        _mappingKeySuffix = mappingKeySuffix;
    }
}