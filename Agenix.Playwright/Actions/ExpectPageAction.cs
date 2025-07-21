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
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;


namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for making expectations about page properties such as title and URL.
///     This action uses Playwright's built-in page assertions for reliable testing.
/// </summary>
public class ExpectPageAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of page expectation types.
    /// </summary>
    public enum ExpectedType
    {
        /// <summary>
        ///     Expect the page to have a specific title.
        /// </summary>
        TITLE,

        /// <summary>
        ///     Expect the page to have a specific URL.
        /// </summary>
        URL
    }

    private const string Match = "match";
    private const string NotMatch = "NOT match";
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(ExpectPageAction));

    /// <summary>
    ///     Provides a Playwright action for making expectations about page properties.
    /// </summary>
    public ExpectPageAction(Builder builder) : this("expect-page", builder) { }

    /// <summary>
    ///     Provides a Playwright action for making expectations about page properties.
    /// </summary>
    public ExpectPageAction(string name, Builder builder) : base(name, builder)
    {
        ExpectationType = builder.ExpectationType;
        ExpectedValue = builder.ExpectedValue;
        ExpectedRegex = builder.ExpectedRegex;
        IsNot = builder.IsNot;
        TimeoutMs = builder.TimeoutMs;
    }

    /// <summary>
    ///     Gets the type of expectation being performed (e.g., title, URL).
    /// </summary>
    public ExpectedType ExpectationType { get; }

    /// <summary>
    ///     Gets the expected string value for the expectation.
    /// </summary>
    public string? ExpectedValue { get; }

    /// <summary>
    ///     Gets the expected regex pattern for the expectation.
    /// </summary>
    public Regex? ExpectedRegex { get; }

    /// <summary>
    ///     Gets whether this is a negated expectation (expect NOT to match).
    /// </summary>
    public bool IsNot { get; }

    /// <summary>
    ///     Gets the timeout value in milliseconds for the expectation.
    /// </summary>
    public int TimeoutMs { get; }


    /// <summary>
    ///     Executes the page expectation within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing page expectation: {ExpectationType}", ExpectationType);

            var page = browser.GetCurrentPage();
            if (page == null)
            {
                throw new InvalidOperationException("No active page available");
            }

            var pageAssertions = Expect(page);

            switch (ExpectationType)
            {
                case ExpectedType.TITLE:
                    await ExpectTitle(pageAssertions, context);
                    break;

                case ExpectedType.URL:
                    await ExpectUrl(pageAssertions, context);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown expectation type: {ExpectationType}");
            }

            Logger.LogInformation("Page expectation completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute page expectation");
            throw new AgenixSystemException("Failed to execute page expectation", ex);
        }
    }

    /// <summary>
    ///     Executes an expectation for the page's title, verifying that it matches either a specified value or a regular
    ///     expression.
    /// </summary>
    /// <param name="pageAssertions">The page assertions the object used to perform the title validation.</param>
    /// <param name="context">The test context which provides additional data and utilities for the action execution.</param>
    /// <returns>A task representing the asynchronous operation of the title expectation.</returns>
    private async Task ExpectTitle(IPageAssertions pageAssertions, TestContext context)
    {
        var options = new PageAssertionsToHaveTitleOptions { Timeout = TimeoutMs };

        try
        {
            if (ExpectedRegex != null)
            {
                Logger.LogDebug("Expecting page title to {Match} regex: {Pattern}", IsNot ? NotMatch : Match,
                    ExpectedRegex.ToString());

                var assertion = IsNot ? pageAssertions.Not : pageAssertions;
                await assertion.ToHaveTitleAsync(ExpectedRegex, options);
            }
            else if (ExpectedValue != null)
            {
                var resolvedDynamicValue = context.ReplaceDynamicContentInString(ExpectedValue);
                Logger.LogDebug("Expecting page title to {Match} value: {Value}", IsNot ? NotMatch : Match,
                    resolvedDynamicValue);

                var assertion = IsNot ? pageAssertions.Not : pageAssertions;
                await assertion.ToHaveTitleAsync(resolvedDynamicValue, options);
            }
            else
            {
                throw new InvalidOperationException(
                    "Either expected value or regex must be specified for title expectation");
            }

            Logger.LogDebug("Title expectation passed");
        }
        catch (PlaywrightException ex)
        {
            throw new AgenixSystemException("Title expectation failed", ex);
        }
    }

    /// <summary>
    ///     Executes an expectation to verify the URL of the current page.
    /// </summary>
    /// <param name="pageAssertions">The assertions object to perform URL validation on the current page.</param>
    /// <param name="context">The test context providing runtime data or helpers for the current test execution.</param>
    /// <returns>A task that represents the asynchronous operation of asserting the page URL.</returns>
    private async Task ExpectUrl(IPageAssertions pageAssertions, TestContext context)
    {
        var options = new PageAssertionsToHaveURLOptions { Timeout = TimeoutMs };

        try
        {
            if (ExpectedRegex != null)
            {
                Logger.LogDebug("Expecting page URL to {Match} regex: {Pattern}", IsNot ? NotMatch : Match,
                    ExpectedRegex.ToString());

                var assertion = IsNot ? pageAssertions.Not : pageAssertions;
                await assertion.ToHaveURLAsync(ExpectedRegex, options);
            }
            else if (ExpectedValue != null)
            {
                var resolvedDynamicValue = context.ReplaceDynamicContentInString(ExpectedValue);
                Logger.LogDebug("Expecting page URL to {Match} value: {Value}", IsNot ? NotMatch : Match,
                    resolvedDynamicValue);

                var assertion = IsNot ? pageAssertions.Not : pageAssertions;
                await assertion.ToHaveURLAsync(resolvedDynamicValue, options);
            }
            else
            {
                throw new InvalidOperationException(
                    "Either expected value or regex must be specified for URL expectation");
            }

            Logger.LogDebug("URL expectation passed");
        }
        catch (PlaywrightException ex)
        {
            throw new AgenixSystemException("URL expectation failed", ex);
        }
    }

    /// <summary>
    ///     Builder class for creating ExpectPageAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<ExpectPageAction, Builder>
    {
        internal ExpectedType ExpectationType { get; private set; }
        internal string? ExpectedValue { get; private set; }
        internal Regex? ExpectedRegex { get; private set; }
        internal bool IsNot { get; private set; }
        internal int TimeoutMs { get; private set; } = 5000;

        /// <summary>
        ///     Sets the expectation to check the page title.
        /// </summary>
        /// <param name="expectedTitle">The expected title value</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveTitle(string expectedTitle)
        {
            ExpectationType = ExpectedType.TITLE;
            ExpectedValue = expectedTitle ?? throw new ArgumentNullException(nameof(expectedTitle));
            ExpectedRegex = null;
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the page title using a regex pattern.
        /// </summary>
        /// <param name="titleRegex">The regex patterns to match against the title</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveTitle(Regex titleRegex)
        {
            ExpectationType = ExpectedType.TITLE;
            ExpectedValue = null;
            ExpectedRegex = titleRegex ?? throw new ArgumentNullException(nameof(titleRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the page URL.
        /// </summary>
        /// <param name="expectedUrl">The expected URL value</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveUrl(string expectedUrl)
        {
            ExpectationType = ExpectedType.URL;
            ExpectedValue = expectedUrl ?? throw new ArgumentNullException(nameof(expectedUrl));
            ExpectedRegex = null;
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the page URL using a regex pattern.
        /// </summary>
        /// <param name="urlRegex">The regex patterns to match against the URL</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveUrl(Regex urlRegex)
        {
            ExpectationType = ExpectedType.URL;
            ExpectedValue = null;
            ExpectedRegex = urlRegex ?? throw new ArgumentNullException(nameof(urlRegex));
            return this;
        }

        /// <summary>
        ///     Negates the expectation (expect NOT to match).
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Not()
        {
            IsNot = true;
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the expectation.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(int timeoutMs)
        {
            TimeoutMs = timeoutMs;
            return this;
        }

        /// <summary>
        ///     Builds the ExpectPageAction instance.
        /// </summary>
        /// <returns>A new ExpectPageAction instance</returns>
        public override ExpectPageAction Build()
        {
            if (ExpectedValue == null && ExpectedRegex == null)
            {
                throw new InvalidOperationException("Either expected value or regex must be specified");
            }

            return new ExpectPageAction(this);
        }
    }
}
