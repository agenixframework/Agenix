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
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Selenium.Endpoint;
using Microsoft.Extensions.Logging;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Navigates to new page either by using new absolute page URL or relative page path.
///     Also supports history forward and back navigation as well as page refresh.
/// </summary>
public class NavigateAction : AbstractSeleniumAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(NavigateAction));

    /// <summary>
    ///     Page URL to navigate to
    /// </summary>
    private readonly string _page;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public NavigateAction(Builder builder) : base("navigate", builder)
    {
        _page = builder.Page;
    }

    /// <summary>
    ///     Gets the page URL
    /// </summary>
    public string Page => _page;

    /// <summary>
    ///     Executes the navigation action based on the specified page instruction.
    ///     Handles browser navigation such as going back, forward, refreshing, or navigating to a specific URL.
    /// </summary>
    /// <param name="browser">The Selenium browser instance used for performing the navigation action.</param>
    /// <param name="context">The testing context providing additional configuration or state for the action.</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        switch (_page?.ToLowerInvariant())
        {
            case "back":
                browser.WebDriver.Navigate().Back();
                break;

            case "forward":
                browser.WebDriver.Navigate().Forward();
                break;

            case "refresh":
                browser.WebDriver.Navigate().Refresh();
                break;

            default:
                NavigateToUrl(browser, context);
                break;
        }
    }

    /// <summary>
    ///     Navigates the web browser to a specified URL. If the provided URL is invalid, attempts to navigate to it as a
    ///     relative path.
    ///     Handles browser-specific issues, including caching in Internet Explorer.
    /// </summary>
    /// <param name="browser">The Selenium browser instance responsible for executing the navigation.</param>
    /// <param name="context">The test context used to dynamically resolve placeholders in the URL.</param>
    private void NavigateToUrl(SeleniumBrowser browser, TestContext context)
    {
        var resolvedPage = context.ReplaceDynamicContentInString(_page);

        try
        {
            // Try to create absolute URL
            var uri = new Uri(resolvedPage);

            // Handle IE caching issues if needed
            if (IsInternetExplorer(browser))
            {
                var cachingSafeUrl = MakeIeCachingSafeUrl(resolvedPage, DateTimeOffset.Now.ToUnixTimeMilliseconds());
                browser.WebDriver.Navigate().GoToUrl(cachingSafeUrl);
            }
            else
            {
                browser.WebDriver.Navigate().GoToUrl(uri);
            }
        }
        catch (UriFormatException)
        {
            // Handle relative URL
            NavigateToRelativeUrl(browser, context, resolvedPage);
        }
    }

    /// <summary>
    ///     Navigates to a relative URL by combining it with the base URL of the browser.
    /// </summary>
    /// <param name="browser">The browser instance used for navigation.</param>
    /// <param name="context">The test context in which the action is performed.</param>
    /// <param name="relativePage">The relative page path to navigate to.</param>
    private static void NavigateToRelativeUrl(SeleniumBrowser browser, TestContext context, string relativePage)
    {
        var baseUrl = GetBaseUrl(browser);

        try
        {
            // Validate base URL
            var baseUri = new Uri(baseUrl);
        }
        catch (Exception e) when (e is UriFormatException or ArgumentNullException)
        {
            // Try to get base URL from a browser configuration
            if (!string.IsNullOrWhiteSpace(browser.EndpointConfiguration.StartPageUrl))
            {
                baseUrl = browser.EndpointConfiguration.StartPageUrl;
            }
            else
            {
                throw new AgenixSystemException("Failed to create relative page URL - must set start page on browser");
            }
        }

        // Ensure the base URL ends with slash
        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        var fullUrl = baseUrl + relativePage;
        browser.WebDriver.Navigate().GoToUrl(fullUrl);
    }

    /// <summary>
    ///     Retrieves the base URL of the current page being used by the Selenium browser.
    /// </summary>
    /// <param name="browser">An instance of the <c>SeleniumBrowser</c> from which the current URL will be fetched.</param>
    /// <returns>The base URL as a string. If the URL cannot be retrieved, an empty string is returned.</returns>
    private static string GetBaseUrl(SeleniumBrowser browser)
    {
        try
        {
            return browser.WebDriver.Url;
        }
        catch (Exception ex)
        {
            Logger.LogDebug("Could not get current URL: {ExMessage}", ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    ///     Determines whether the specified browser instance is Internet Explorer.
    /// </summary>
    /// <param name="browser">The Selenium browser instance to check.</param>
    /// <returns>True if the browser is Internet Explorer; otherwise, false.</returns>
    private static bool IsInternetExplorer(SeleniumBrowser browser)
    {
        // Check if a browser type is Internet Explorer
        var browserType = browser.EndpointConfiguration.BrowserType?.ToLowerInvariant();
        return browserType != null &&
               (browserType.Contains("ie") ||
                browserType.Contains("internet") ||
                browserType.Contains("internet explorer") ||
                browserType.Contains("internetexplorer"));
    }

    /// <summary>
    ///     Generates a URL that is safe for Internet Explorer caching by appending
    ///     or updating a timestamp parameter to avoid serving outdated cached versions.
    /// </summary>
    /// <param name="url">The original URL to be made caching-safe.</param>
    /// <param name="timestamp">The timestamp in milliseconds since the Unix epoch, used for ensuring cache uniqueness.</param>
    /// <returns>A caching-safe URL with an appended or replaced timestamp parameter.</returns>
    private string MakeIeCachingSafeUrl(string url, long timestamp)
    {
        if (url.Contains("timestamp="))
        {
            // Replace existing timestamp parameter
            return Regex.Replace(url,
                @"(.*)(timestamp=)([^&#]*)(.*)",
                $"$1$2{timestamp}$4");
        }

        // Add new timestamp parameter
        return url.Contains('?')
            ? $"{url}&timestamp={timestamp}"
            : $"{url}?timestamp={timestamp}";
    }

    /// <summary>
    ///     Action builder for NavigateAction
    /// </summary>
    public class Builder : Builder<NavigateAction, Builder>
    {
        public string Page { get; private set; }

        /// <summary>
        ///     Set the page URL to navigate to
        /// </summary>
        public Builder SetPage(string page)
        {
            Page = page;
            return self;
        }

        /// <summary>
        ///     Constructs and returns a new instance of the NavigateAction class.
        /// </summary>
        /// <returns>A new instance of NavigateAction.</returns>
        public override NavigateAction Build()
        {
            return new NavigateAction(this);
        }
    }
}
