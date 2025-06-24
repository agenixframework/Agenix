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
using System.Text.RegularExpressions;
using Agenix.Api;
using Agenix.Api.Log;

namespace Agenix.Core.Log;

/// <summary>
///     Default modifier implementation uses regular expressions to mask logger output. Regular expressions match on
///     default keywords.
/// </summary>
public class DefaultLogModifier : LogMessageModifierBase
{
    private readonly HashSet<string> keywords = AgenixSettings.GetLogMaskKeywords();
    private readonly string logMaskValue = AgenixSettings.GetLogMaskValue();
    private readonly bool maskFormUrlEncoded = true;
    private Regex formUrlEncodedPattern;
    private Regex jsonPattern;

    private Regex keyValuePattern;
    private bool maskJson = true;
    private bool maskKeyValue = true;

    private bool maskXml = true;
    private Regex xmlPattern;

    public override string Mask(string source)
    {
        if (!AgenixSettings.IsLogModifierEnabled() || source == null || source.Length == 0)
        {
            return source;
        }

        var xml = maskXml && source.StartsWith('<');
        var json = maskJson && !xml && (source.StartsWith('{') || source.StartsWith('['));
        var formUrlEncoded = maskFormUrlEncoded && !json && source.Contains('&') && source.Contains('=');

        var masked = source;
        if (xml)
        {
            masked = CreateXmlPattern(keywords).Replace(masked, $"$1{logMaskValue}$2");
            if (maskKeyValue)
            // used for the attributes in the XML tags
            {
                masked = CreateKeyValuePattern(keywords).Replace(masked, $"$1{logMaskValue}");
            }
        }
        else if (json)
        {
            masked = CreateJsonPattern(keywords).Replace(masked, $"$1\"{logMaskValue}\"");
        }
        else if (formUrlEncoded)
        {
            masked = CreateFormUrlEncodedPattern(keywords).Replace(masked, $"$1{logMaskValue}");
        }
        else if (maskKeyValue)
        {
            masked = CreateKeyValuePattern(keywords).Replace(masked, $"$1{logMaskValue}");
        }

        return masked;
    }

    protected Regex CreateKeyValuePattern(HashSet<string> keywords)
    {
        if (keyValuePattern == null)
        {
            var keywordExpression = CreateKeywordsExpression(keywords);
            if (string.IsNullOrEmpty(keywordExpression))
            {
                return null;
            }

            var regex = $"((?:{keywordExpression})\\s*=\\s*['\"]?)([^,'\"]+)";
            keyValuePattern = new Regex(regex, RegexOptions.IgnoreCase);
        }

        return keyValuePattern;
    }

    protected Regex CreateFormUrlEncodedPattern(HashSet<string> keywords)
    {
        if (formUrlEncodedPattern == null)
        {
            var keywordExpression = CreateKeywordsExpression(keywords);
            if (string.IsNullOrEmpty(keywordExpression))
            {
                return null;
            }

            var regex = $"((?:{keywordExpression})\\s*=\\s*)([^&]*)";
            formUrlEncodedPattern = new Regex(regex, RegexOptions.IgnoreCase);
        }

        return formUrlEncodedPattern;
    }

    protected Regex CreateXmlPattern(HashSet<string> keywords)
    {
        if (xmlPattern == null)
        {
            var keywordExpression = CreateKeywordsExpression(keywords);
            if (string.IsNullOrEmpty(keywordExpression))
            {
                return null;
            }

            var regex = $"(<(?:{keywordExpression})>)[^<]*(</(?:{keywordExpression})>)";
            xmlPattern = new Regex(regex, RegexOptions.IgnoreCase);
        }

        return xmlPattern;
    }

    protected Regex CreateJsonPattern(HashSet<string> keywords)
    {
        if (jsonPattern == null)
        {
            var keywordExpression = CreateKeywordsExpression(keywords);
            if (string.IsNullOrEmpty(keywordExpression))
            {
                return null;
            }

            var regex = $"(\"(?:{keywordExpression})\"\\s*:\\s*)(" + "\"?[^\",]*[\",])";
            jsonPattern = new Regex(regex, RegexOptions.IgnoreCase);
        }

        return jsonPattern;
    }

    protected static string CreateKeywordsExpression(HashSet<string> keywords)
    {
        if (keywords == null || keywords.Count == 0)
        {
            return "";
        }

        return string.Join("|", keywords.Select(t => Regex.Escape(t)));
    }

    public void SetMaskJson(bool maskJson)
    {
        this.maskJson = maskJson;
    }

    public void SetMaskXml(bool maskXml)
    {
        this.maskXml = maskXml;
    }

    public void SetMaskKeyValue(bool maskKeyValue)
    {
        this.maskKeyValue = maskKeyValue;
    }
}
