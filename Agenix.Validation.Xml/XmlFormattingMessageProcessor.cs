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

using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Validation.Xml.Util;

namespace Agenix.Validation.Xml;

/// <summary>
///     The XmlFormattingMessageProcessor is responsible for processing messages
///     where the payload is expected to be in XML format. The processor formats
///     the XML payload into a pretty-printed format for enhanced readability.
/// </summary>
/// <remarks>
///     This class extends the AbstractMessageProcessor and overrides specific
///     methods to provide custom processing for XML messages. It performs message
///     type selection and content transformation specific to XML payloads.
/// </remarks>
/// <example>
///     Intended to process only messages of the type "XML". It ensures the
///     payload is transformed into a clean and formatted XML structure.
/// </example>
public class XmlFormattingMessageProcessor : AbstractMessageProcessor
{
    /// <summary>
    ///     Processes the provided message by applying XML formatting to its payload.
    /// </summary>
    /// <param name="message">The message to process. Its payload is expected to be XML content.</param>
    /// <param name="context">
    ///     The context in which the message is being processed. This provides additional metadata or
    ///     dependencies needed for processing.
    /// </param>
    public override void ProcessMessage(IMessage message, TestContext context)
    {
        message.Payload = XmlUtils.PrettyPrint(message.GetPayload<string>());
    }

    /// <summary>
    ///     Determines whether the given message type is supported by this processor.
    /// </summary>
    /// <param name="messageType">
    ///     The type of the message to evaluate for support. Possible values are defined in the
    ///     <see cref="MessageType" /> enumeration.
    /// </param>
    /// <returns>True if the message type is supported; otherwise, false.</returns>
    public override bool SupportsMessageType(string messageType)
    {
        return string.Equals(messageType, nameof(MessageType.XML), StringComparison.OrdinalIgnoreCase);
    }
}
