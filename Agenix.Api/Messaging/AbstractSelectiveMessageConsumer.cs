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
///     An abstract class that represents a selective message consumer capable of receiving messages
///     based on a given selector and context.
/// </summary>
public abstract class AbstractSelectiveMessageConsumer(string name, IEndpointConfiguration endpointConfiguration)
    : AbstractMessageConsumer(name, endpointConfiguration), ISelectiveConsumer
{
    private readonly IEndpointConfiguration _endpointConfiguration1 = endpointConfiguration;

    /// <summary>
    ///     Receives a message from a queue based on the specified selector and context.
    /// </summary>
    /// <param name="selector">The message selector to filter messages.</param>
    /// <param name="context">The context containing information about the test and execution environment.</param>
    /// <returns>An instance of IMessage if a message is received, otherwise null.</returns>
    public IMessage Receive(string selector, TestContext context)
    {
        return Receive(selector, context, _endpointConfiguration1.Timeout);
    }

    /// <summary>
    ///     Receives a message from a queue based on the specified selector and context.
    /// </summary>
    /// <param name="context">The context containing information about the test and execution environment.</param>
    /// <param name="timeout">The maximum time to wait for a message, in milliseconds.</param>
    /// <returns>An instance of <see cref="IMessage" /> if a message is received, otherwise null.</returns>
    public override IMessage Receive(TestContext context, long timeout)
    {
        return Receive(null, context, timeout);
    }

    /// <summary>
    ///     Receives a message from a queue based on the specified selector, context, and timeout.
    /// </summary>
    /// <param name="selector">The message selector to filter messages.</param>
    /// <param name="context">The context containing information about the test and execution environment.</param>
    /// <param name="timeout">The maximum time to wait for a message.</param>
    /// <returns>An instance of IMessage if a message is received, otherwise null.</returns>
    public abstract IMessage Receive(string selector, TestContext context, long timeout);
}
