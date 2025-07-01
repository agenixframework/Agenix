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

namespace Agenix.Selenium.Util;

/// <summary>
///     Utility class for browser-related operations.
///     Provides helper methods for URL manipulation and browser-specific functionality.
/// </summary>
public static class BrowserUtils
{
    /// <summary>
    ///     Makes new unique URL to avoid IE caching by adding or updating a timestamp parameter.
    /// </summary>
    /// <param name="url">The original URL</param>
    /// <param name="unique">The unique timestamp value to append</param>
    /// <returns>The URL with timestamp parameter to prevent caching</returns>
    public static string MakeIeCachingSafeUrl(string url, long unique)
    {
        if (url.Contains("timestamp="))
        {
            // Replace existing timestamp parameter
            // Pattern matches: (anything)(timestamp=)(value)([&#]more) or (anything)(timestamp=)(value)(end)
            const string pattern1 = @"(.*)(timestamp=)(.*)([&#].*)";
            const string pattern2 = @"(.*)(timestamp=)(.*)$";

            var result = Regex.Replace(url, pattern1, $"$1$2{unique}$4");
            if (result == url) // The first pattern didn't match, try the second
            {
                result = Regex.Replace(url, pattern2, $"$1$2{unique}");
            }

            return result;
        }

        // Add new timestamp parameter
        return url.Contains('?')
            ? $"{url}&timestamp={unique}"
            : $"{url}?timestamp={unique}";
    }

    /// <summary>
    ///     Makes new unique URL to avoid IE caching using current timestamp.
    /// </summary>
    /// <param name="url">The original URL</param>
    /// <returns>The URL with current timestamp parameter to prevent caching</returns>
    public static string MakeIeCachingSafeUrl(string url)
    {
        return MakeIeCachingSafeUrl(url, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
}
