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

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     The DirectEndpointBuilder class is responsible for constructing instances of
///     <see cref="DirectEndpoint" />. This class provides methods to configure various
///     properties of the <see cref="DirectEndpoint" /> such as queue name and timeout.
/// </summary>
public class DirectEndpointBuilder : AbstractEndpointBuilder<DirectEndpoint>
{
    private readonly DirectEndpoint _endpoint = new();

    /// <summary>
    ///     Retrieves the DirectEndpoint instance associated with this builder.
    /// </summary>
    /// <returns>The DirectEndpoint instance that is being constructed by this builder.</returns>
    protected override DirectEndpoint GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    ///     Sets the queue name property.
    /// </summary>
    /// <param name="queueName">The name of the queue to set.</param>
    /// <returns>The current instance of <see cref="DirectEndpointBuilder" /> for method chaining.</returns>
    public DirectEndpointBuilder Queue(string queueName)
    {
        _endpoint.EndpointConfiguration.SetQueueName(queueName);
        return this;
    }

    /// <summary>
    ///     Sets the queue name property.
    /// </summary>
    /// <param name="queueName">The name of the queue to set.</param>
    /// <returns>The current instance of <see cref="DirectEndpointBuilder" /> for method chaining.</returns>
    public DirectEndpointBuilder Queue(IMessageQueue queue)
    {
        _endpoint.EndpointConfiguration.SetQueue(queue);
        return this;
    }

    /// <summary>
    ///     Sets the default timeout for the DirectEndpoint.
    /// </summary>
    /// <param name="timeout">The timeout duration in milliseconds.</param>
    /// <return>The current instance of the DirectEndpointBuilder for method chaining.</return>
    public DirectEndpointBuilder Timeout(long timeout)
    {
        _endpoint.EndpointConfiguration.Timeout = timeout;
        return this;
    }
}
