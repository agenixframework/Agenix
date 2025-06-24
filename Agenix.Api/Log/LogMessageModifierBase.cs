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

namespace Agenix.Api.Log;

/// <summary>
///     Special modifier adds message-related modifications on logger output on headers and body.
/// </summary>
public abstract class LogMessageModifierBase : ILogModifier
{
    public abstract string Mask(string statement);

    /// <summary>
    ///     Mask the given message body to not print sensitive data.
    /// </summary>
    /// <param name="message">the message</param>
    /// <returns></returns>
    public string MaskBody(IMessage message)
    {
        return Mask(message.GetPayload<string>().Trim());
    }

    /// <summary>
    ///     Mask the given message header values to not print sensitive data.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Dictionary<string, object> MaskHeaders(IMessage message)
    {
        var newDict = new Dictionary<string, object>();
        foreach (var entry in message.GetHeaders())
        {
            if (entry.Value == null)
            {
                newDict.Add(entry.Key, "");
                continue;
            }

            var keyValuePair = $"{entry.Key}={entry.Value}";
            if (!keyValuePair.Equals(Mask(keyValuePair)))
            {
                newDict.Add(entry.Key, AgenixSettings.GetLogMaskValue());
                continue;
            }

            newDict.Add(entry.Key, entry.Value);
        }

        return newDict;
    }


    public List<string> MaskHeaderData(IMessage message)
    {
        if (message.GetHeaderData == null || message.GetHeaderData().Count == 0)
        {
            return [];
        }

        return message.GetHeaderData().Select(Mask).ToList();
    }
}
