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
using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
///     Represents a matcher that checks if a string consists only of whitespace characters or is empty.
/// </summary>
/// <remarks>
///     This matcher is used to validate if a string is "blank," which means the string contains only whitespace characters
///     (spaces, tabs, etc.) or has a length of zero. It can also be combined with additional matchers for more complex
///     validation.
/// </remarks>
/// <example>
///     Not provided in this context.
/// </example>
/// <remarks>
///     This class is primarily intended to work with the NHamcrest library for fluent validation of string values.
/// </remarks>
public class IsBlankString : IMatcher<string>
{
    public static readonly IsBlankString BlankInstance = new();

    public static readonly IMatcher<string> NullOrBlankInstance =
        global::NHamcrest.Matches.AnyOf(Is.Null(), BlankInstance);

    private static readonly Regex RegexWhitespace = new(@"^\s*$", RegexOptions.Compiled);

    private IsBlankString()
    {
    }

    public void DescribeTo(IDescription description)
    {
        description.AppendText("a blank string");
    }

    public bool Matches(string item)
    {
        return RegexWhitespace.IsMatch(item);
    }

    public void DescribeMismatch(string item, IDescription mismatchDescription)
    {
        mismatchDescription.AppendText("was a ")
            .AppendText(" (")
            .AppendText(item)
            .AppendText(")");
    }
}
