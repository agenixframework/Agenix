#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using Agenix.Api.Endpoint.Adapter.Mapping;
using Agenix.Api.Message;

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
