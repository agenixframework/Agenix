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

using System;
using System.Collections.Generic;
using System.Text;
using Agenix.Api.Context;

namespace Agenix.Core.Message;

/// <summary>
///     Constructs message selectors either from string value or from key value maps. Currently only AND logical
///     combination of multiple expressions is supported.
/// </summary>
public class MessageSelectorBuilder
{
    /// Holds the selector string used for constructing message selectors.
    /// /
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
        if (!string.IsNullOrEmpty(messageSelector))
        {
            return context.ReplaceDynamicContentInString(messageSelector);
        }

        return messageSelectorMap is { Count: > 0 }
            ? FromKeyValueMap(context.ResolveDynamicValuesInMap(messageSelectorMap)).Build()
            : "";
    }

    /// Static builder method using a selector string.
    /// <param name="selectorString">The selector string used to initialize the builder.</param>
    /// <return>A new instance of MessageSelectorBuilder initialized with the given selector string.</return>
    public static MessageSelectorBuilder WithString(string selectorString)
    {
        return new MessageSelectorBuilder(selectorString);
    }

    /// <summary>
    ///     Constructs a message selector builder from a key-value map.
    /// </summary>
    /// <param name="valueMap">A dictionary containing key-value pairs to construct the message selector.</param>
    /// <return>A new instance of the <see cref="MessageSelectorBuilder" /> with the constructed selector string.</return>
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
    /// This is necessary because Xpath expressions can contain equal characters in node tests,
    /// which need to be escaped before evaluating the message selector expression.
    /// @param selectorExpression The selector expression that may contain Xpath node tests.
    /// @return The modified selector expression with equal characters in node tests escaped.
    /// /
    private string EscapeEqualsFromXpathNodeTest(string selectorExpression)
    {
        var nodeTestStart = "[";
        var nodeTestEnd = "]";

        // check presence of Xpath node test first
        if (!selectorExpression.Contains(nodeTestStart) || !selectorExpression.Contains(nodeTestEnd))
        {
            return selectorExpression; //no Xpath node test return initial value - nothing to escape
        }

        var selectorBuilder = new StringBuilder();
        var nodeTestStartIndex = selectorExpression.IndexOf(nodeTestStart, StringComparison.Ordinal);
        var nodeTestEndIndex = selectorExpression.IndexOf(nodeTestEnd, StringComparison.Ordinal);
        var escape = false;
        for (var i = 0; i < selectorExpression.Length; i++)
        {
            if (i == nodeTestStartIndex)
            {
                escape = true;
            }

            if (escape && selectorExpression[i] == '=')
            {
                selectorBuilder.Append("@equals@");
            }
            else
            {
                selectorBuilder.Append(selectorExpression[i]);
            }

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

    /// Parses expression string and replaces all equals character escaping with initial
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
