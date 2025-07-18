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

using System.Collections.ObjectModel;
using System.Text;
using Agenix.Api.Context;
using Agenix.Api.Log;

namespace Agenix.Api.Message;

/// <summary>
///     Represents a generic message structure that contains an identifier, name, payload, headers,
///     and provides methods for managing headers and accessing payload information.
/// </summary>
public interface IMessage
{
    /// <summary>
    ///     Gets the unique message id;
    /// </summary>
    string Id { get; }

    /// <summary>
    ///     Gets/Sets the message name for internal use.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     Gets/Sets the message payload.
    /// </summary>
    object? Payload { get; set; }

    /// <summary>
    ///     Gets the message header value by its header name.
    /// </summary>
    /// <param name="headerName"></param>
    /// <returns></returns>
    object GetHeader(string headerName);

    /// <summary>
    ///     Sets new header entry in message header list.
    /// </summary>
    /// <param name="headerName"></param>
    /// <param name="headerValue"></param>
    /// <returns></returns>
    IMessage SetHeader(string headerName, object headerValue);

    /// <summary>
    ///     Removes the message header if it not a reserved message header such as unique message id.
    /// </summary>
    /// <param name="headerName"></param>
    void RemoveHeader(string headerName);

    /// <summary>
    ///     Adds new header data.
    /// </summary>
    /// <param name="headerData"></param>
    /// <returns></returns>
    IMessage AddHeaderData(string headerData);

    /// <summary>
    ///     Gets the list of header data in this message.
    /// </summary>
    /// <returns></returns>
    List<string> GetHeaderData();

    /// <summary>
    ///     Gets message headers.
    /// </summary>
    /// <returns></returns>
    Dictionary<string, object> GetHeaders();

    /// <summary>
    ///     Gets message payload with required type conversion.
    /// </summary>
    /// <typeparam name="TX"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    TX GetPayload<TX>();

    /// <summary>
    ///     Indicates the type of the message content (e.g. Xml, Json, binary)
    /// </summary>
    /// <returns></returns>
    string GetType();

    /// <summary>
    ///     Sets the message type indicating the content type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IMessage SetType(string type);

    /// <summary>
    ///     Retrieves the timestamp header value from the message.
    /// </summary>
    /// <returns>
    ///     The timestamp value as a long data type.
    /// </returns>
    int GetTimestamp();

    /// <summary>
    ///     Prints given message content (body, headers, headerData) to String representation.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="headers"></param>
    /// <param name="headerData"></param>
    /// <returns></returns>
    string Print(string body, Dictionary<string, object> headers, List<string> headerData)
    {
        var interfaceName = nameof(IMessage).ToUpper();

        if (headerData == null || headerData.Count == 0)
        {
            return
                $"{interfaceName} [id: {Id}, payload: {MessagePayloadUtils.PrettyPrint(body)}][headers: {ToStringRepresentation(new ReadOnlyDictionary<string, object>(headers))}]";
        }

        return
            $"{interfaceName} [id: {Id}, payload: {MessagePayloadUtils.PrettyPrint(body)}][headers: {ToStringRepresentation(new ReadOnlyDictionary<string, object>(headers))}][header-data: {string.Join(", ", new ReadOnlyCollection<string>(headerData))}]";
    }

    public static string ToStringRepresentation(ReadOnlyDictionary<string, object> dictionary)
    {
        var builder = new StringBuilder();
        builder.Append('{');
        foreach (var kvp in dictionary)
        {
            builder.Append($"{kvp.Key}={kvp.Value}, ");
        }

        if (dictionary.Count > 0)
        {
            builder.Length -= 2; // Remove the trailing ", "
        }

        builder.Append('}');
        return builder.ToString();
    }

    /// <summary>
    ///     Prints message content to String representation.
    /// </summary>
    /// <returns></returns>
    string Print()
    {
        var trimmedPayload = GetPayload<string>().Trim();
        return Print(trimmedPayload, GetHeaders(), GetHeaderData());
    }

    /// <summary>
    ///     Prints message content and applies logger modifier provided in given test context.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    string Print(TestContext context)
    {
        if (context == null)
        {
            return Print();
        }

        var logModifier = context.LogModifier;

        if (logModifier is LogMessageModifierBase modifier)
        {
            return Print(modifier.MaskBody(this), modifier.MaskHeaders(this), modifier.MaskHeaderData(this));
        }

        return logModifier != null
            ? Print(logModifier.Mask(GetPayload<string>()?.Trim()), GetHeaders(), GetHeaderData())
            : Print();
    }
}
