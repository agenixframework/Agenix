using System;
using System.Collections.Generic;
using System.Text;

namespace Agenix.Core.Message;

/// <summary>
///     Constructs message selectors either from string value or from key value maps. Currently only AND logical
///     combination of multiple expressions is supported.
/// </summary>
public class MessageSelectorBuilder
{
    /**
     * Selector string
     */
    private readonly string _selectorString;

    /// Constructor using fields.
    /// @param selectorString The selector string to initialize the object with.
    /// /
    public MessageSelectorBuilder(string selectorString)
    {
        _selectorString = selectorString;
    }

    /// Constructs a message selector from a string expression or a key-value map, automatically replacing test variables.
    /// @param messageSelector The initial message selector string.
    /// @param messageSelectorMap A dictionary containing key-value pairs for the message selector.
    /// @param context The current test context used for resolving dynamic values.
    /// @return A constructed message selector string.
    /// /
    public static string Build(string messageSelector, Dictionary<string, object> messageSelectorMap,
        TestContext context)
    {
        if (!string.IsNullOrEmpty(messageSelector)) return context.ReplaceDynamicContentInString(messageSelector);

        return messageSelectorMap is { Count: > 0 }
            ? FromKeyValueMap(context.ResolveDynamicValuesInMap(messageSelectorMap)).Build()
            : "";
    }

    /**
     * Static builder method using a selector string.
     * @param selectorString
     * @return
     */
    public static MessageSelectorBuilder WithString(string selectorString)
    {
        return new MessageSelectorBuilder(selectorString);
    }

    /**
     * Static builder method using a key value map.
     * @param valueMap
     * @return
     */
    public static MessageSelectorBuilder FromKeyValueMap(Dictionary<string, object> valueMap)
    {
        var buf = new StringBuilder();

        var iter = valueMap.GetEnumerator();

        if (iter.MoveNext())
        {
            var entry = iter.Current;
            var key = entry.Key;
            var value = entry.Value.ToString();

            buf.Append(key).Append(" = '").Append(value).Append('\'');
        }

        while (iter.MoveNext())
        {
            var entry = iter.Current;
            var key = entry.Key;
            var value = entry.Value.ToString();

            buf.Append(" AND ").Append(key).Append(" = '").Append(value).Append('\'');
        }

        return new MessageSelectorBuilder(buf.ToString());
    }

    /// Constructs a key value map from selector string representation.
    /// @return Dictionary where keys and values are extracted from the selector string.
    /// /
    public Dictionary<string, string> ToKeyValueDictionary()
    {
        var valueMap = new Dictionary<string, string>();
        string[] tokens;

        if (_selectorString.Contains(" AND"))
        {
            var chunks = _selectorString.Split([" AND"], StringSplitOptions.None);
            foreach (var chunk in chunks)
            {
                tokens = EscapeEqualsFromXpathNodeTest(chunk).Split('=');
                valueMap.Add(UnescapeEqualsFromXpathNodeTest(tokens[0].Trim()),
                    tokens[1].Trim().Substring(1, tokens[1].Trim().Length - 2));
            }
        }
        else
        {
            tokens = EscapeEqualsFromXpathNodeTest(_selectorString).Split('=');
            valueMap.Add(UnescapeEqualsFromXpathNodeTest(tokens[0].Trim()),
                tokens[1].Trim().Substring(1, tokens[1].Trim().Length - 2));
        }

        return valueMap;
    }

    /// Escapes equals characters in Xpath node tests within the given selector expression.
    /// This is necessary because Xpath expressions can contain equals characters in node tests,
    /// which need to be escaped before evaluating the message selector expression.
    /// @param selectorExpression The selector expression that may contain Xpath node tests.
    /// @return The modified selector expression with equals characters in node tests escaped.
    /// /
    private string EscapeEqualsFromXpathNodeTest(string selectorExpression)
    {
        var nodeTestStart = "[";
        var nodeTestEnd = "]";

        // check presence of Xpath node test first
        if (!selectorExpression.Contains(nodeTestStart) || !selectorExpression.Contains(nodeTestEnd))
            return selectorExpression; //no Xpath node test return initial value - nothing to escape

        var selectorBuilder = new StringBuilder();
        var nodeTestStartIndex = selectorExpression.IndexOf(nodeTestStart, StringComparison.Ordinal);
        var nodeTestEndIndex = selectorExpression.IndexOf(nodeTestEnd, StringComparison.Ordinal);
        var escape = false;
        for (var i = 0; i < selectorExpression.Length; i++)
        {
            if (i == nodeTestStartIndex) escape = true;

            if (escape && selectorExpression[i] == '=')
                selectorBuilder.Append("@equals@");
            else
                selectorBuilder.Append(selectorExpression[i]);

            if (i == nodeTestEndIndex)
            {
                nodeTestStartIndex =
                    selectorExpression.IndexOf(nodeTestStart, nodeTestEndIndex + 1, StringComparison.Ordinal);
                nodeTestEndIndex =
                    selectorExpression.IndexOf(nodeTestEnd, nodeTestEndIndex + 1, StringComparison.Ordinal);
                escape = false;
            }
        }

        return selectorBuilder.ToString();
    }

    /// Parses expression string and replaces all equals character escapings with initial
    /// equals character.
    /// @param expression The input expression with escaped equals characters.
    /// @return The expression with escaped equals characters replaced by the actual equals character.
    private static string UnescapeEqualsFromXpathNodeTest(string expression)
    {
        return expression.Replace("@equals@", "=");
    }

    /// Builds the message selector.
    /// @return The constructed message selector string.
    /// /
    public string Build()
    {
        return _selectorString;
    }
}