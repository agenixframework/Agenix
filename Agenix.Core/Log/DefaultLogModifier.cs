using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Agenix.Core.Log;

/// <summary>
///     Default modifier implementation uses regular expressions to mask logger output. Regular expressions match on
///     default keywords.
/// </summary>
public class DefaultLogModifier : LogMessageModifierBase
{
    private readonly HashSet<string> keywords = CoreSettings.GetLogMaskKeywords();
    private readonly string logMaskValue = CoreSettings.GetLogMaskValue();
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
        if (!CoreSettings.IsLogModifierEnabled() || source == null || source.Length == 0) return source;

        var xml = maskXml && source.StartsWith('<');
        var json = maskJson && !xml && (source.StartsWith('{') || source.StartsWith('['));
        var formUrlEncoded = maskFormUrlEncoded && !json && source.Contains('&') && source.Contains('=');

        var masked = source;
        if (xml)
        {
            masked = CreateXmlPattern(keywords).Replace(masked, $"$1{logMaskValue}$2");
            if (maskKeyValue)
                // used for the attributes in the XML tags
                masked = CreateKeyValuePattern(keywords).Replace(masked, $"$1{logMaskValue}");
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
            if (string.IsNullOrEmpty(keywordExpression)) return null;

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
            if (string.IsNullOrEmpty(keywordExpression)) return null;

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
            if (string.IsNullOrEmpty(keywordExpression)) return null;

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
            if (string.IsNullOrEmpty(keywordExpression)) return null;

            var regex = $"(\"(?:{keywordExpression})\"\\s*:\\s*)(" + "\"?[^\",]*[\",])";
            jsonPattern = new Regex(regex, RegexOptions.IgnoreCase);
        }

        return jsonPattern;
    }

    protected static string CreateKeywordsExpression(HashSet<string> keywords)
    {
        if (keywords == null || keywords.Count == 0) return "";

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