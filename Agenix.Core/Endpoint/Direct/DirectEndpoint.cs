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

using Agenix.Api.Messaging;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     Direct message endpoint implementation sends and receives message from in memory message queue.
/// </summary>
public class DirectEndpoint : AbstractEndpoint
{
    /**
     * Cached producer or consumer
     */
    private DirectConsumer _channelConsumer;

    private DirectProducer _channelProducer;

    /// A class representing a direct message endpoint.
    /// This class is responsible for creating producers and consumers for the direct endpoint.
    /// /
    public DirectEndpoint() : base(new DirectEndpointConfiguration())
    {
    }

    /// <summary>
    ///     Direct message endpoint implementation sends and receives message from in memory message queue.
    /// </summary>
    public DirectEndpoint(DirectEndpointConfiguration configuration) : base(configuration)
    {
    }

    /// <summary>
    ///     Provides the specific configuration settings used by the DirectEndpoint.
    /// </summary>
    public override DirectEndpointConfiguration EndpointConfiguration =>
        (DirectEndpointConfiguration)base.EndpointConfiguration;

    /// <summary>
    ///     Creates a new producer instance for the direct endpoint.
    /// </summary>
    /// <returns>Returns an instance of <see cref="IProducer" />.</returns>
    public override IProducer CreateProducer()
    {
        return _channelProducer ??= new DirectProducer(ProducerName, EndpointConfiguration);
    }

    /// Creates a consumer for the direct message endpoint.
    /// This method instantiates and returns a new DirectConsumer if one does not already exist.
    /// <returns>An IConsumer instance representing the message consumer.</returns>
    public override ISelectiveConsumer CreateConsumer()
    {
        return _channelConsumer ??= new DirectConsumer(ConsumerName, EndpointConfiguration);
    }
}
