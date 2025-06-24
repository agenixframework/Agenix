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

using System.Text.RegularExpressions;

namespace Agenix.Api.Variable;

/// <summary>
///     Matcher that matches segments of variable expressions. The matcher is capable of matching the following segments:
///     - indexed variables/properties segments of the form: 'var[1]'
///     - jsonPath segments of the form: 'jsonPath($.person.name)'
///     - xpath segments of the form: 'xpath(//person/name)'
///     Note that jsonPath and xpath segments must terminate the expression, i.e. they cannot be followed by further
///     expressions.
///     If a variable expression is used to access a variable from the test context it must start with a variable segment
///     which
///     extracts the first variable from the test context.
///     Sample valid variable expressions:
///     - var1
///     - var1.var2
///     - var1[1]
///     - var1[1].var2[2]
///     - var1[1].var2[2].var3
///     - var1.jsonPath($.person.name)
///     - var1[1].jsonPath($.person.name)
///     - var1.xpath(//title[@lang='en'])
///     - var1[1].xpath(//title[@lang='en'])
/// </summary>
public partial class VariableExpressionSegmentMatcher
{
    /// <summary>
    ///     The regex group index for the full xpath segment
    /// </summary>
    private const int XpathSegmentGroup = 1;

    /// <summary>
    ///     The regex group index for the xpath path
    /// </summary>
    private const int XpathGroup = 2;

    /// <summary>
    ///     The regex group index for the full jsonPath segment
    /// </summary>
    private const int JsonpathSegmentGroup = 3;

    /// <summary>
    ///     The regex group index for the jsonPath part
    /// </summary>
    private const int JsonPathGroup = 4;

    /// <summary>
    ///     The regex group index for the full variable/property segment incl. index
    /// </summary>
    private const int VarPropSegmentGroup = 5;

    /// <summary>
    ///     The regex group index for the name of the variable/property
    /// </summary>
    private const int VarPropNameGroup = 6;

    /// <summary>
    ///     The regex group index for the full name/index expression
    /// </summary>
    private const int NameIndexGroup = 7;

    /// <summary>
    ///     The regex group index for the index when accessing array elements
    /// </summary>
    private const int IndexGroup = 8;

    /// <summary>
    ///     Pattern to parse a variable expression
    /// </summary>
    private static readonly Regex VarPathPattern = RegexPattern();

    /// <summary>
    ///     Match collection from the regex
    /// </summary>
    private readonly MatchCollection _matches;

    /// <summary>
    ///     The total number of segments in the variableExpression
    /// </summary>
    private readonly int _totalSegmentCount;

    /// <summary>
    ///     The variable expression the matcher is working on
    /// </summary>
    private readonly string _variableExpression;

    /// <summary>
    ///     Current position in the matches collection
    /// </summary>
    private int _currentMatchIndex;

    /// <summary>
    ///     The current expression the matcher has matched
    /// </summary>
    private string? _currentSegmentExpression;

    /// <summary>
    ///     The current segment expression index. A value of -1 indicates "no index".
    /// </summary>
    private int _currentSegmentIndex = -1;

    /// <summary>
    ///     Creates a new variable expression segment matcher
    /// </summary>
    /// <param name="variableExpression">The variable expression to match</param>
    public VariableExpressionSegmentMatcher(string variableExpression)
    {
        _variableExpression = variableExpression;
        _matches = VarPathPattern.Matches(variableExpression);
        _totalSegmentCount = _matches.Count;
    }

    /// <summary>
    ///     Gets the total number of segments in the variable expression of this matcher
    /// </summary>
    /// <returns>The total segment count</returns>
    public int TotalSegmentCount => _totalSegmentCount;

    /// <summary>
    ///     Gets the variable expression which backs the matcher.
    /// </summary>
    /// <returns>The variable expression</returns>
    public string VariableExpression => _variableExpression;

    /// <summary>
    ///     Gets the segment expression of the current match. Null if the matcher has run out of matches.
    /// </summary>
    /// <returns>The current segment expression</returns>
    public string SegmentExpression => _currentSegmentExpression;

    /// <summary>
    ///     Gets the segment index of the current match. -1 if the match is not indexed, or the matcher has run out of matches.
    /// </summary>
    /// <returns>The current segment index</returns>
    public int SegmentIndex => _currentSegmentIndex;

    /// <summary>
    ///     Attempts to find the next segment in the variable expression and sets the current
    ///     segment expression as well as the current segment index.
    /// </summary>
    /// <returns>True if a match was found, false otherwise</returns>
    public bool NextMatch()
    {
        _currentSegmentExpression = null;
        _currentSegmentIndex = -1;

        if (_currentMatchIndex >= _matches.Count)
        {
            return false;
        }

        var match = _matches[_currentMatchIndex++];

        if (!string.IsNullOrEmpty(match.Groups[JsonPathGroup].Value))
        {
            _currentSegmentExpression = match.Groups[JsonPathGroup].Value;
        }
        else if (!string.IsNullOrEmpty(match.Groups[XpathGroup].Value))
        {
            _currentSegmentExpression = match.Groups[XpathGroup].Value;
        }
        else
        {
            _currentSegmentExpression = match.Groups[VarPropNameGroup].Value;
            _currentSegmentIndex = match.Groups[IndexGroup].Success
                ? int.Parse(match.Groups[IndexGroup].Value)
                : -1;
        }

        return true;
    }

    /// <summary>
    ///     Generates a regular expression to match segments of a variable expression.
    /// </summary>
    /// <returns>
    ///     A compiled <see cref="Regex" /> object to parse variable expressions, supporting segments such as indexed
    ///     variables,
    ///     JSONPath, or XPath.
    /// </returns>
    [GeneratedRegex(@"(xpath\((.*)\)$)|(jsonPath\((\$[.\[].*)\)$)|(([^\[\].]+)(\[([0-9])])?)(\.|$)")]
    private static partial Regex RegexPattern();
}
