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

using Agenix.Api.Log;
using Agenix.Api.Validation.Matcher;
using Agenix.Core.Validation.Matcher.Core;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Validation.Matcher;

public class DefaultValidationMatcherLibrary : ValidationMatcherLibrary
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultValidationMatcherLibrary));

    /// <summary>
    ///     Default constructor adds default matcher implementations.
    /// </summary>
    public DefaultValidationMatcherLibrary()
    {
        Name = " CoreValidationMatcherLibrary";

        Members.Add("EqualsIgnoreCase", new EqualsIgnoreCaseValidationMatcher());
        Members.Add("Empty", new EmptyValidationMatcher());
        Members.Add("Ignore", new IgnoreValidationMatcher());
        Members.Add("ContainsIgnoreCase", new ContainsIgnoreCaseValidationMatcher());
        Members.Add("Contains", new ContainsValidationMatcher());
        Members.Add("DatePattern", new DatePatternValidationMatcher());
        Members.Add("EndsWith", new EndsWithValidationMatcher());
        Members.Add("LowerThan", new LowerThanValidationMatcher());
        Members.Add("GreaterThan", new GreaterThanValidationMatcher());
        Members.Add("IgnoreNewLine", new IgnoreNewLineValidationMatcher());
        Members.Add("IsNumber", new IsNumberValidationMatcher());
        Members.Add("Matches", new MatchesValidationMatcher());
        Members.Add("StartsWith", new StartsWithValidationMatcher());
        Members.Add("StringLength", new StringLengthValidationMatcher());
        Members.Add("Variable", new CreateVariableValidationMatcher());
        Members.Add("Trim", new TrimValidationMatcher());
        Members.Add("TrimAllWhiteSpaces", new TrimAllWhitespacesValidationMatcher());

        LookupValidationMatchers();
    }

    /// <summary>
    ///     Add custom matcher implementations loaded from the resource path lookup.
    /// </summary>
    private void LookupValidationMatchers()
    {
        foreach (var (key, matcher) in IValidationMatcher.Lookup())
        {
            Members.Add(key, matcher);

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Register message matcher '{Key}' as {Type}", key, matcher.GetType());
            }
        }
    }
}
