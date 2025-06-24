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

public class MatchesPatternMatcher : IMatcher<string>
{
    private readonly string _pattern;

    public MatchesPatternMatcher(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern must not be null or empty", nameof(pattern));
        }

        _pattern = pattern;
    }

    public void DescribeTo(IDescription description)
    {
        description.AppendText("a string matching the pattern ")
            .AppendValue(_pattern);
    }

    public bool Matches(string actual)
    {
        if (actual == null)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(actual, _pattern);
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("Invalid regular expression pattern: " + _pattern);
        }
    }

    public void DescribeMismatch(string item, IDescription mismatchDescription)
    {
        mismatchDescription.AppendText("was ")
            .AppendValue(item)
            .AppendText(", which does not match the pattern");
    }
}
