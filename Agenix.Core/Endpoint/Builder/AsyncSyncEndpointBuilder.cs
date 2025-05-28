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

using Agenix.Api.Endpoint;

namespace Agenix.Core.Endpoint.Builder;

/// <summary>
///     Provides a builder for both asynchronous and synchronous endpoints, enabling the construction of endpoints that
///     support both types of operations.
/// </summary>
/// <typeparam name="TA">The type of the asynchronous endpoint builder.</typeparam>
/// <typeparam name="TS">The type of the synchronous endpoint builder.</typeparam>
public class AsyncSyncEndpointBuilder<TA, TS>
    where TA : IEndpointBuilder<IEndpoint>
    where TS : IEndpointBuilder<IEndpoint>
{
    private readonly TA _asyncEndpointBuilder;
    private readonly TS _syncEndpointBuilder;

    /// <summary>
    ///     Default constructor setting the sync and async builder implementation.
    /// </summary>
    /// <param name="asyncEndpointBuilder">The asynchronous endpoint builder.</param>
    /// <param name="syncEndpointBuilder">The synchronous endpoint builder.</param>
    public AsyncSyncEndpointBuilder(TA asyncEndpointBuilder, TS syncEndpointBuilder)
    {
        _asyncEndpointBuilder = asyncEndpointBuilder;
        _syncEndpointBuilder = syncEndpointBuilder;
    }

    /// <summary>
    ///     Gets the async endpoint builder.
    /// </summary>
    /// <returns>The asynchronous endpoint builder.</returns>
    public TA Asynchronous()
    {
        return _asyncEndpointBuilder;
    }

    /// <summary>
    ///     Gets the sync endpoint builder.
    /// </summary>
    /// <returns>The synchronous endpoint builder.</returns>
    public TS Synchronous()
    {
        return _syncEndpointBuilder;
    }
}
