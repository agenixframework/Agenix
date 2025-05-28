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

using Agenix.Api.Message;

namespace Agenix.Api.Endpoint.Adapter.Mapping;

/// <summary>
///     Interface describes mapping extraction along message dispatching endpoint adapter processing steps. Extractor
///     is supposed to read mapping name predicate from the request (e.g., via XPath, header value, etc.). Dispatching
///     endpoint adapters
///     may then dispatch message processing according to this mapping name.
/// </summary>
public interface IMappingKeyExtractor
{
    /// Extracts the mapping key from an incoming request message.
    /// @param request The incoming request message from which the mapping key is to be extracted.
    /// @return The mapping key extracted from the request message.
    /// /
    string ExtractMappingKey(IMessage request);
}
