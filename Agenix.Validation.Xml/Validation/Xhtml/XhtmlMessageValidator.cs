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

using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;

namespace Agenix.Validation.Xml.Validation.Xhtml;

/// <summary>
///     Provides validation mechanisms for XHTML messages by extending the functionality
///     of the base DomXmlMessageValidator class. This validator is specifically designed
///     to handle messages with XHTML content while ensuring XML schema validation is also applied.
/// </summary>
public class XhtmlMessageValidator : DomXmlMessageValidator, InitializingPhase
{
    /// <summary>
    ///     Represents the property that allows getting or setting the instance of
    ///     <see cref="XhtmlMessageConverter" /> used for converting HTML content to
    ///     XHTML format and other related operations.
    /// </summary>
    public XhtmlMessageConverter XhtmlMessageConverter { get; set; } = new();

    public void Initialize()
    {
        XhtmlMessageConverter.Initialize();
    }

    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        XmlMessageValidationContext validationContext)
    {
        var messagePayload = receivedMessage.GetPayload<string>();
        var convertedMessage =
            new DefaultMessage(XhtmlMessageConverter.Convert(messagePayload), receivedMessage.GetHeaders());
        base.ValidateMessage(convertedMessage, controlMessage, context, validationContext);
    }

    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(nameof(MessageType.XHTML), StringComparison.OrdinalIgnoreCase)
               && MessageUtils.HasXmlPayload(message);
    }
}
