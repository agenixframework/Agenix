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

using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;

namespace Agenix.Api.Messaging;

/// <summary>
///     Abstract base class for message consumers, providing common functionality
///     for handling messages within an endpoint configuration context.
/// </summary>
public abstract class AbstractMessageConsumer : IConsumer
{
    private readonly IEndpointConfiguration _endpointConfiguration;

    /// <summary>
    ///     Default constructor using receive timeout setting.
    /// </summary>
    /// <param name="name">The name of the consumer</param>
    /// <param name="endpointConfiguration">Endpoint configuration</param>
    public AbstractMessageConsumer(string name, IEndpointConfiguration endpointConfiguration)
    {
        Name = name;
        _endpointConfiguration = endpointConfiguration;
    }

    public string Name { get; }

    /// <summary>
    ///     Synchronously receives a message with a context provided by TestContext.
    /// </summary>
    /// <param name="context">The context containing the state and environment for the message reception.</param>
    /// <returns>The received message as an instance of IMessage.</returns>
    public IMessage Receive(TestContext context)
    {
        return Receive(context, _endpointConfiguration.Timeout);
    }

    // Abstract method to be implemented by subclasses
    public abstract IMessage Receive(TestContext context, long timeout);
}
