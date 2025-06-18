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

using Agenix.Api.Exceptions;
using Agenix.Api.Message;

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
        if (request.GetHeader(_headerName) != null)
        {
            return request.GetHeader(_headerName)?.ToString();
        }

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
