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

namespace Agenix.Api.Endpoint;

/// <summary>
/// Endpoint adapter represents a special message handler that delegates incoming request messages to some message endpoint.
/// Clients can receive request messages from endpoint and provide proper response messages that will be used as
/// adapter response.
/// </summary>
public interface IEndpointAdapter
{
    /// <summary>
    /// Handles a request message and returning a proper response.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <returns>The response message.</returns>
    IMessage HandleMessage(IMessage message);

    /// <summary>
    /// Gets message endpoint to interact with this endpoint adapter.
    /// </summary>
    /// <returns>The endpoint instance.</returns>
    IEndpoint GetEndpoint();

    /// <summary>
    /// Gets the endpoint configuration.
    /// </summary>
    /// <returns>The endpoint configuration.</returns>
    IEndpointConfiguration GetEndpointConfiguration();
}
