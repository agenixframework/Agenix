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

using System;
using System.Collections.Generic;
using System.Linq;
using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Core.Message;

namespace Agenix.Core.Validation.Builder;

/// <summary>
///     Default message builder delegates to given message header builders and message payload builder..
///     Implements IMessageBuilder, IWithPayloadBuilder, IWithHeaderBuilder, and INamed interfaces.
/// </summary>
public class DefaultMessageBuilder : IMessageBuilder, IWithPayloadBuilder, IWithHeaderBuilder, INamed
{
    private readonly List<IMessageHeaderBuilder> _headerBuilders = [];
    private string _name = "";

    private IMessagePayloadBuilder _payloadBuilder;

    /// <summary>
    ///     Builds the message based on the provided test context and message type.
    /// </summary>
    /// <param name="context">The test context that contains various variables and settings needed to build the message.</param>
    /// <param name="messageType">The type of the message being built.</param>
    /// <returns>The constructed message.</returns>
    public virtual IMessage Build(TestContext context, string messageType)
    {
        var payload = BuildMessagePayload(context, messageType);

        try
        {
            var headers = BuildMessageHeaders(context);
            var message = new DefaultMessage(payload, headers) { Name = _name };
            message.SetType(messageType);

            var headerData = BuildMessageHeaderData(context);
            message.GetHeaderData().AddRange(headerData);

            return message;
        }
        catch (SystemException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to build message content", e);
        }
    }

    public void SetName(string name)
    {
        _name = name;
    }

    public void AddHeaderBuilder(IMessageHeaderBuilder builder)
    {
        _headerBuilders.Add(builder);
    }

    public List<IMessageHeaderBuilder> GetHeaderBuilders()
    {
        return _headerBuilders;
    }

    public void SetPayloadBuilder(IMessagePayloadBuilder builder)
    {
        _payloadBuilder = builder;
    }

    public IMessagePayloadBuilder GetPayloadBuilder()
    {
        return _payloadBuilder;
    }

    /// <summary>
    ///     Builds the message payload based on the provided test context and message type.
    /// </summary>
    /// <param name="context">The test context that contains various variables and settings needed to build the payload.</param>
    /// <param name="messageType">The type of the message for which the payload is being built.</param>
    /// <returns>The built message payload. If no payload builder is set, an empty string is returned.</returns>
    public virtual object BuildMessagePayload(TestContext context, string messageType)
    {
        switch (_payloadBuilder)
        {
            case null:
                return "";
            case IMessageTypeAware messageTypeAwareBuilder:
                messageTypeAwareBuilder.SetMessageType(messageType);
                break;
        }

        return _payloadBuilder.BuildPayload(context);
    }

    /// <summary>
    ///     Builds the message headers based on the provided test context.
    /// </summary>
    /// <param name="context">The test context containing necessary variables and settings for building the headers.</param>
    /// <returns>The constructed dictionary of message headers with their corresponding values.</returns>
    public virtual Dictionary<string, object> BuildMessageHeaders(TestContext context)
    {
        try
        {
            var headers = new Dictionary<string, object>();
            foreach (var entry in _headerBuilders.SelectMany(builder => builder.BuilderHeaders(context)))
            {
                headers[entry.Key] = entry.Value;
            }

            foreach (var entry in headers.ToList())
            {
                if (entry.Value is string value)
                {
                    if (MessageHeaderTypeExtensions.IsTyped(value))
                    {
                        var type = MessageHeaderTypeExtensions.FromTypedValue(value);
                        var clrType = type.GetClrType();
                        var valueToConvert = MessageHeaderTypeExtensions.RemoveTypeDefinition(value);
                        headers[entry.Key] = Convert.ChangeType(valueToConvert, clrType);
                    }
                }
            }

            MessageHeaderUtils.CheckHeaderTypes(headers);

            return headers;
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to build message content", e);
        }
    }

    /// <summary>
    ///     Builds the message header data based on the provided test context.
    /// </summary>
    /// <param name="context">The test context that contains various variables and settings needed to build the header data.</param>
    /// <returns>A list of header data strings built from the provided context.</returns>
    public virtual List<string> BuildMessageHeaderData(TestContext context)
    {
        var headerData = new List<string>();

        foreach (var builder in _headerBuilders)
        {
            if (builder is IMessageHeaderDataBuilder headerDataBuilder)
            {
                headerData.Add(headerDataBuilder.BuildHeaderData(context));
            }
        }

        return headerData;
    }

    public string GetName()
    {
        return _name;
    }
}
