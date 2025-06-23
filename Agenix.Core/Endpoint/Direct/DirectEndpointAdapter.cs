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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;

namespace Agenix.Core.Endpoint.Direct;

using Microsoft.Extensions.Logging;

/// <summary>
/// Endpoint adapter forwards incoming requests to message queue and waits synchronously for response
/// on reply queue. Provides simple endpoint for clients to connect to message queue in order to provide proper
/// response message.
/// </summary>
public class DirectEndpointAdapter : AbstractEndpointAdapter
{
    /// <summary>
    /// Endpoint handling incoming requests
    /// </summary>
    private readonly DirectSyncEndpoint _endpoint;
    private readonly DirectSyncProducer _producer;

    /// <summary>
    /// Endpoint configuration
    /// </summary>
    private readonly DirectSyncEndpointConfiguration _endpointConfiguration;

    /// <summary>
    /// Logger
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(DirectEndpointAdapter));

    /// <summary>
    /// Constructor using endpoint.
    /// </summary>
    /// <param name="endpoint">The direct sync endpoint.</param>
    public DirectEndpointAdapter(DirectSyncEndpoint endpoint) : base(Logger)
    {
        _endpointConfiguration = endpoint.EndpointConfiguration;

        endpoint.SetName(Name);
        _producer = new DirectSyncProducer(endpoint.ProducerName, _endpointConfiguration);
        _endpoint = endpoint;
    }

    /// <summary>
    /// Constructor using endpoint configuration.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    public DirectEndpointAdapter(DirectSyncEndpointConfiguration endpointConfiguration) : base(Logger)
    {
        _endpointConfiguration = endpointConfiguration;

        _endpoint = new DirectSyncEndpoint(endpointConfiguration);
        _endpoint.SetName(Name);
        _producer = new DirectSyncProducer(_endpoint.ProducerName, endpointConfiguration);
    }

    /// <summary>
    /// Handles the incoming request message internally.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <returns>The reply message.</returns>
    protected override IMessage HandleMessageInternal(IMessage request)
    {
        Logger.LogDebug("Forwarding request to message queue ...");

        var context = GetTestContext();
        IMessage replyMessage = null;

        try
        {
            _producer.Send(request, context);

            if (_endpointConfiguration.Correlator != null)
            {
                replyMessage = _producer.Receive(
                    _endpointConfiguration.Correlator.GetCorrelationKey(request),
                    context,
                    _endpointConfiguration.Timeout);
            }
            else
            {
                replyMessage = _producer.Receive(context, _endpointConfiguration.Timeout);
            }
        }
        catch (ActionTimeoutException e)
        {
            Logger.LogWarning(e.Message);
        }

        return replyMessage;
    }

    /// <summary>
    /// Gets the direct endpoint.
    /// </summary>
    /// <returns>The direct endpoint.</returns>
    public override IEndpoint GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    /// Gets the endpoint configuration.
    /// </summary>
    /// <returns>The direct sync endpoint configuration.</returns>
    public override IEndpointConfiguration GetEndpointConfiguration()
    {
        return _endpointConfiguration;
    }
}
