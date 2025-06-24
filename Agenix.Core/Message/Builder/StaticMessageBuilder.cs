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

using System.Collections.Generic;
using System.Linq;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core.Validation.Builder;

namespace Agenix.Core.Message.Builder;

/// StaticMessageBuilder is a class that inherits from DefaultMessageBuilder and provides functionality for building
/// static messages with a given IMessage instance.
public class StaticMessageBuilder : DefaultMessageBuilder
{
    protected static readonly List<string> FilteredHeaders =
    [
        MessageHeaders.Id,
        MessageHeaders.Timestamp
    ];

    private readonly IMessage _message;

    public StaticMessageBuilder(IMessage message)
    {
        _message = message;
        _message.Name = message.Name;
    }

    /// Builds the message payload based on the specified TestContext and message type.
    /// <param name="context">The TestContext that provides context-specific details for building the payload.</param>
    /// <param name="messageType">The type of the message used to determine the payload structure.</param>
    /// <return>Returns the constructed message payload object.</return>
    public override object BuildMessagePayload(TestContext context, string messageType)
    {
        if (GetPayloadBuilder() == null)
        {
            SetPayloadBuilder(new DefaultPayloadBuilder(_message.Payload));
        }

        return base.BuildMessagePayload(context, messageType);
    }

    /// Builds a dictionary of message headers based on the specified TestContext.
    /// <param name="context">The TestContext that provides context-specific details for building the headers.</param>
    /// <return>Returns a dictionary of headers with additional entries from the IMessage instance that are not filtered out.</return>
    public override Dictionary<string, object> BuildMessageHeaders(TestContext context)
    {
        var headers = base.BuildMessageHeaders(context);

        var filteredHeaders = _message.GetHeaders()
            .Where(entry => !FilteredHeaders.Contains(entry.Key))
            .ToDictionary(entry => entry.Key, entry => entry.Value);

        var defaultHeaderBuilder = new DefaultHeaderBuilder(filteredHeaders);
        var additionalHeaders = defaultHeaderBuilder.BuilderHeaders(context);

        foreach (var header in additionalHeaders)
        {
            headers[header.Key] = header.Value;
        }

        return headers;
    }

    /// Builds a list of message header data based on the specified TestContext.
    /// <param name="context">The TestContext that provides context-specific details for building the header data.</param>
    /// <return>Returns a list of strings representing the message header data.</return>
    public override List<string> BuildMessageHeaderData(TestContext context)
    {
        var headerData = base.BuildMessageHeaderData(context);
        headerData.AddRange(_message.GetHeaderData()
            .Select(data => new DefaultHeaderDataBuilder(data).BuildHeaderData(context)));

        return headerData;
    }

    /// Creates a new instance of StaticMessageBuilder with the specified message.
    /// <param name="message">The message to be used for building the StaticMessageBuilder.</param>
    /// <return>Returns an instance of StaticMessageBuilder with the specified message.</return>
    public static StaticMessageBuilder WithMessage(IMessage message)
    {
        return new StaticMessageBuilder(message);
    }

    /// Retrieves the current message.
    /// <return>Returns an instance of the current message.</return>
    /// /
    public virtual IMessage GetMessage()
    {
        return _message;
    }
}
