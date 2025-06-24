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

using Agenix.Api.Message;

namespace Agenix.Core.Util;

/// <summary>
///     Utility class for message-related operations.
/// </summary>
public static class MessageUtils
{
    /// <summary>
    ///     Checks if given message payload is of type XML. If starts with '<' is considered valid XML.
    /// </summary>
    /// <param name="message">to check.</param>
    /// <returns>true if message payload is XML, false otherwise.</returns>
    public static bool HasXmlPayload(IMessage message)
    {
        if (message.Payload is not string)
        {
            return false;
        }

        var payload = message.GetPayload<string>().Trim();

        return payload.StartsWith('<');
    }

    /// <summary>
    ///     Checks if message payload is of type Json. An empty payload is considered to be a valid Json payload.
    /// </summary>
    /// <param name="message">to check.</param>
    /// <returns>true if payload is Json, false otherwise.</returns>
    public static bool HasJsonPayload(IMessage message)
    {
        if (message.Payload is not string)
        {
            return false;
        }

        var payload = message.GetPayload<string>().Trim();

        return payload.Length == 0 || payload.StartsWith('{') || payload.StartsWith('[');
    }
}
