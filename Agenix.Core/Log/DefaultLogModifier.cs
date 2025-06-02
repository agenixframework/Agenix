#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
