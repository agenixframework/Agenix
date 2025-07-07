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

using OpenQA.Selenium;

namespace Agenix.Selenium.Util;

/// <summary>
/// Represents the types of web browsers supported for automated testing or interaction.
/// </summary>
public enum BrowserType
{
    /// <summary>
    /// Represents the Google Chrome web browser as a supported browser type
    /// for automated testing or interaction in the <c>BrowserType</c> enumeration.
    /// </summary>
    CHROME,

    /// <summary>
    /// Represents the Microsoft Edge web browser as a supported browser type
    /// in the <c>BrowserType</c> enumeration used for automated testing or interaction.
    /// </summary>
    EDGE,

    /// <summary>
    /// Represents the Mozilla Firefox web browser as a supported browser type
    /// for automated testing or interaction in the <c>BrowserType</c> enumeration.
    /// </summary>
    FIREFOX,

    /// <summary>
    /// Represents the Safari web browser as a supported browser type
    /// in the <c>BrowserType</c> enumeration for automated testing or interaction.
    /// </summary>
    SAFARI,

    /// <summary>
    /// Represents the Internet Explorer web browser as a supported browser type
    /// for automated testing or interaction in the <c>BrowserType</c> enumeration.
    /// </summary>
    INTERNET_EXPLORER,

    /// <summary>
    /// Represents the Opera web browser as a supported browser type
    /// within the <c>BrowserType</c> enumeration for automated testing or interaction.
    /// </summary>
    OPERA,

    /// <summary>
    /// Represents the HtmlUnit browser as a lightweight, headless browser type
    /// in the <c>BrowserType</c> enumeration, primarily used for testing purposes
    /// without a graphical user interface.
    /// </summary>
    HTML_UNIT,
}

/// <summary>
/// Provides extension methods for the <c>BrowserType</c> enumeration, enabling retrieval of
/// browser names and type matching functionality.
/// </summary>
public static class BrowserTypeExtensions
{
    /// <summary>
    /// Retrieves the corresponding browser name as a string based on the specified browser type.
    /// </summary>
    /// <param name="browser">The browser type for which to get the name.</param>
    /// <returns>The string representation of the browser name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the browser type is not recognized.</exception>
    public static string GetBrowserName(this BrowserType browser)
    {
        return browser switch
        {
            BrowserType.CHROME => "chrome",
            BrowserType.EDGE => "MicrosoftEdge",
            BrowserType.FIREFOX => "firefox",
            BrowserType.SAFARI => "safari",
            BrowserType.INTERNET_EXPLORER => "internet explorer",
            BrowserType.OPERA => "opera",
            BrowserType.HTML_UNIT => "htmlunit",
            _ => throw new ArgumentOutOfRangeException(nameof(browser))
        };
    }

    /// <summary>
    /// Determines whether the specified browser type matches the given browser name.
    /// </summary>
    /// <param name="browser">The browser type to evaluate.</param>
    /// <param name="browserName">The browser name to compare against the browser type.</param>
    /// <returns>true if the browser type matches the given browser name; otherwise, false.</returns>
    public static bool Is(this BrowserType browser, string browserName)
    {
        var mainName = browser.GetBrowserName();
        if (mainName.Equals(browserName, StringComparison.OrdinalIgnoreCase))
            return true;

        return browser switch
        {
            BrowserType.CHROME => "chrome-headless-shell".Equals(browserName, StringComparison.OrdinalIgnoreCase),
            BrowserType.EDGE => "msedge".Equals(browserName, StringComparison.OrdinalIgnoreCase),
            BrowserType.SAFARI => "Safari".Equals(browserName, StringComparison.Ordinal),
            _ => false
        };
    }

    /// <summary>
    /// Checks if the given browser type matches the browser name specified in the provided capabilities.
    /// </summary>
    /// <param name="browser">The browser type to check.</param>
    /// <param name="capabilities">The capabilities containing the browser name to match against the browser type.</param>
    /// <returns>True if the browser type matches the browser name in the capabilities; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="capabilities"/> argument is null.</exception>
    public static bool Is(this BrowserType browser, ICapabilities capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        return browser.Is(capabilities.GetCapability("browserName")?.ToString() ?? "");
    }
}

