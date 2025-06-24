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

namespace Agenix.Http.Message;

/// Utility class for handling operations related to HTTP messages.
/// Provides methods to copy settings from one message to another,
/// ensuring compatibility and preserving headers and properties.
/// /
public static class HttpMessageUtils
{
    /// Copies settings from a source message to a target HTTP message, converting if necessary.
    /// @param from The source message, which can be either an IMessage or HttpMessage.
    /// @param to The target HTTP message to which settings will be applied.
    /// /
    public static void Copy(IMessage from, HttpMessage to)
    {
        HttpMessage source;
        if (from is HttpMessage httpMessage)
        {
            source = httpMessage;
        }
        else
        {
            source = new HttpMessage(from);
        }

        Copy(source, to);
    }

    /// Copies the properties and headers from one HttpMessage instance to another.
    /// @param from the source HttpMessage from which properties are to be copied
    /// @param to the target HttpMessage to which properties are to be copied
    /// /
    public static void Copy(HttpMessage from, HttpMessage to)
    {
        to.Name = from.Name;
        to.SetType(from.GetType());
        to.Payload = from.Payload;

        foreach (var entry in from.GetHeaders().Where(entry =>
                     !entry.Key.Equals(MessageHeaders.Id) && !entry.Key.Equals(MessageHeaders.Timestamp)))
        {
            to.Header(entry.Key, entry.Value);
        }

        foreach (var headerData in from.GetHeaderData())
        {
            to.AddHeaderData(headerData);
        }

        foreach (var cookie in from.GetCookies())
        {
            to.Cookie(cookie);
        }
    }
}
