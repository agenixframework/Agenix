#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Net.Http.Headers;
using System.Text;
using Agenix.Api.Log;
using Agenix.Api.Report;
using Agenix.Core;
using Agenix.Core.Message;
using Microsoft.Extensions.Logging;

namespace Agenix.Http.Interceptor;

/// <summary>
///     The LoggingClientHandler class is a custom HTTP message handler that logs HTTP request and response messages.
///     It extends the DelegatingHandler, enabling modification or inspection of requests and responses during HTTP
///     communication.
/// </summary>
public class LoggingClientHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private static readonly string Newline = Environment.NewLine;

    private static readonly ILogger Log = LogManager.GetLogger(typeof(LoggingClientHandler));

    private readonly TestContextFactory _contextFactory = TestContextFactory.NewInstance();

    private MessageListeners _messageListener = null!;

    /// <summary>
    ///     Sends an HTTP request asynchronously and processes both the request and response by logging their contents and
    ///     notifying message listeners if any are registered.
    /// </summary>
    /// <param name="request">The HTTP request message to be sent.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation if needed.</param>
    /// <returns>
    ///     A task representing the asynchronous operation, with an HttpResponseMessage result containing the response
    ///     from the server.
    /// </returns>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            var requestBody = request.Content.ReadAsStringAsync(cancellationToken).Result;
            HandleRequest(GetRequestContent(request, requestBody));
        }
        else
        {
            HandleRequest(GetRequestContent(request, string.Empty));
        }

        // Execute request
        var response = base.Send(request, cancellationToken);

        HandleResponse(GetResponseContent(response));

        return response;
    }

    /// <summary>
    ///     Processes the HTTP request by logging the message and notifying message listeners if any are registered.
    /// </summary>
    /// <param name="request">The HTTP request content as a string.</param>
    public void HandleRequest(string request)
    {
        if (HasMessageListeners())
        {
            Log.LogDebug("Sending Http request message");
            _messageListener.OnOutboundMessage(new RawMessage(request), _contextFactory.GetObject());
        }
        else
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Sending Http request message:" + Newline + request);
            }
        }
    }

    /// <summary>
    ///     Handles the HTTP response by logging the message and notifying message listeners if any are registered.
    /// </summary>
    /// <param name="response">The HTTP response content as a string.</param>
    public void HandleResponse(string response)
    {
        if (HasMessageListeners())
        {
            Log.LogDebug("Received Http response message");
            _messageListener.OnInboundMessage(new RawMessage(response), _contextFactory.GetObject());
        }
        else
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Received Http response message:" + Newline + response);
            }
        }
    }

    /// <summary>
    ///     Asynchronously retrieves the HTTP response content as a formatted string, including status line, headers, and body.
    /// </summary>
    /// <param name="response">The HTTP response message to be processed.</param>
    /// <returns>
    ///     A task representing the asynchronous operation, with a string result containing the formatted HTTP response
    ///     content.
    /// </returns>
    private string GetResponseContent(HttpResponseMessage response)
    {
        if (response != null)
        {
            var builder = new StringBuilder();

            builder.Append(response.Version);
            builder.Append(response.StatusCode); // StatusCode is an enum, casting gives numeric value
            builder.Append(' ');
            builder.Append(response.ReasonPhrase);
            builder.Append(Newline);

            AppendHeadersCommon(response.Headers, builder);

            if (response.Content != null)
            {
                AppendHeadersCommon(response.Content.Headers, builder); // Content headers should also be appended;
                builder.Append(Newline);
                builder.Append(response.Content.ReadAsStringAsync().Result); // Body content already read as a string
            }

            return builder.ToString();
        }

        return string.Empty;
    }

    /// <summary>
    ///     Determines whether there are any message listeners registered.
    /// </summary>
    /// <returns>Returns true if there are message listeners registered; otherwise, false.</returns>
    public bool HasMessageListeners()
    {
        return _messageListener != null && !_messageListener.IsEmpty();
    }

    private string GetRequestContent(HttpRequestMessage request, string body)
    {
        var builder = new StringBuilder();

        builder.Append(request.Method);
        builder.Append(' ');
        builder.Append(request.RequestUri);
        builder.Append(Newline);

        AppendHeadersCommon(request.Headers, builder);
        if (request.Content != null)
        {
            AppendHeadersCommon(request.Content.Headers, builder);
        }

        builder.Append(Newline);
        builder.Append(body);

        return builder.ToString();
    }

    /// <summary>
    ///     Appends headers to the specified StringBuilder in a formatted manner.
    /// </summary>
    /// <param name="headers">The collection of HTTP headers to be appended.</param>
    /// <param name="builder">The StringBuilder to which headers are appended.</param>
    private void AppendHeadersCommon(HttpHeaders headers, StringBuilder builder)
    {
        foreach (var header in headers)
        {
            builder.Append(header.Key);
            builder.Append(':');
            builder.Append(string.Join(",", header.Value)); // Join the list of values with a comma
            builder.Append(Environment.NewLine);
        }
    }

    public void SetMessageListener(MessageListeners messageListener)
    {
        _messageListener = messageListener;
    }
}
