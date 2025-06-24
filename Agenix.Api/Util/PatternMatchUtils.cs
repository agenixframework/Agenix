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

namespace Agenix.Api.Util;

/// <summary>
///     Utility methods for simple pattern matching.
/// </summary>
public abstract class PatternMatchUtils
{
    /// <summary>
    ///     Match a String against the given pattern, supporting the following simple
    ///     pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
    /// </summary>
    /// <param name="pattern">
    ///     the pattern to match against
    /// </param>
    /// <param name="str">
    ///     the String to match
    /// </param>
    /// <returns>
    ///     whether the String matches the given pattern
    /// </returns>
    public static bool SimpleMatch(string pattern, string str)
    {
        if (ObjectUtils.NullSafeEquals(pattern, str) || "*".Equals(pattern))
        {
            return true;
        }

        if (pattern == null || str == null)
        {
            return false;
        }

        if (pattern.StartsWith("*") && pattern.EndsWith("*") &&
            str.Contains(pattern.Substring(1, pattern.Length - 1 - 1)))
        {
            return true;
        }

        if (pattern.StartsWith("*") && str.EndsWith(pattern.Substring(1, pattern.Length - 1)))
        {
            return true;
        }

        return pattern.EndsWith("*") && str.StartsWith(pattern.Substring(0, pattern.Length - 1 - 0));
    }

    /// <summary>
    ///     Match a String against the given patterns, supporting the following simple
    ///     pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
    /// </summary>
    /// <param name="patterns">
    ///     the patterns to match against
    /// </param>
    /// <param name="str">
    ///     the String to match
    /// </param>
    /// <returns>
    ///     whether the String matches any of the given patterns
    /// </returns>
    public static bool SimpleMatch(string[] patterns, string str)
    {
        return patterns != null && patterns.Any(t => SimpleMatch(t, str));
    }
}
