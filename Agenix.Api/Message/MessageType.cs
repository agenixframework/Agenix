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

namespace Agenix.Api.Message;

/// <summary>
///     Enumeration for message protocol types used in test cases.
/// </summary>
public enum MessageType
{
    UNSPECIFIED,
    XML,
    XHTML,
    CSV,
    JSON,
    PLAINTEXT,
    BINARY,
    BINARY_BASE64,
    GZIP,
    GZIP_BASE64,
    MSCONS
}

/// <summary>
///     Provides extension methods for the MessageType enumeration.
/// </summary>
public class MessageTypeExtensions
{
    public static readonly string FormUrlEncoded = "x-www-form-urlencoded";

    /// Determines whether the specified message type name matches any value in the MessageType enumeration.
    /// @param messageTypeName The name of the message type to be checked.
    /// @return True if the message type name matches a value in the enumeration; otherwise, false.
    /// /
    public static bool Knows(string messageTypeName)
    {
        return Enum.GetValues(typeof(MessageType)).Cast<object>().Any(value =>
            value.ToString()!.Equals(messageTypeName, StringComparison.OrdinalIgnoreCase));
    }

    /// Checks if the given message type is handled as binary content.
    /// @param messageTypeName The name of the message type to evaluate.
    /// @return True if the message type is binary content, otherwise false.
    /// /
    public static bool IsBinary(string messageTypeName)
    {
        return nameof(MessageType.GZIP).Equals(messageTypeName, StringComparison.OrdinalIgnoreCase)
               || nameof(MessageType.BINARY).Equals(messageTypeName, StringComparison.OrdinalIgnoreCase);
    }

    /// Determines whether the specified message type corresponds to XML formats, including XML and XHTML.
    /// <param name="messageType">The message type to be evaluated.</param>
    /// <return>True if the message type is XML or XHTML; otherwise, false.</return>
    public static bool IsXml(string messageType)
    {
        return nameof(MessageType.XML).Equals(messageType, StringComparison.OrdinalIgnoreCase)
               || nameof(MessageType.XHTML).Equals(messageType, StringComparison.OrdinalIgnoreCase);
    }
}
