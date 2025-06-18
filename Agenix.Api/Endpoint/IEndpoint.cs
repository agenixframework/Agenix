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

using Agenix.Api.Common;
using Agenix.Api.Messaging;

namespace Agenix.Api.Endpoint;

/// <summary>
///     Represents an endpoint for messaging processes that includes configurations,
///     and provides capabilities to produce and consume messages.
/// </summary>
public interface IEndpoint : INamed
{
    /// <summary>
    ///     Gets the endpoint configuration holding all endpoint specific properties such as endpoint uri, connection timeout,
    ///     ports, etc.
    /// </summary>
    IEndpointConfiguration EndpointConfiguration { get; }

    /// <summary>
    ///     Gets/ Sets the endpoint name
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Creates a message producer for this endpoint for sending messages to this endpoint.
    /// </summary>
    /// <returns></returns>
    IProducer CreateProducer();

    /// <summary>
    ///     Creates a message consumer for this endpoint. Consumer receives messages on this endpoint.
    /// </summary>
    /// <returns></returns>
    IConsumer CreateConsumer();
}
