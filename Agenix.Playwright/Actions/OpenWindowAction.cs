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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for opening new browser windows/tabs or navigating to URLs.
///     Supports creating new pages, new browser contexts, and managing window/tab switching.
/// </summary>
public class OpenWindowAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of window opening modes.
    /// </summary>
    public enum WindowOpenMode
    {
        /// <summary>
        ///     Open a new tab in the current context.
        /// </summary>
        NEW_TAB,

        /// <summary>
        ///     Open a new window (same as the new tab in Playwright).
        /// </summary>
        NEW_WINDOW,

        /// <summary>
        ///     Open a new browser context with a new page.
        /// </summary>
        NEW_CONTEXT,

        /// <summary>
        ///     Navigate the current page to a new URL.
        /// </summary>
        CURRENT_PAGE
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(OpenWindowAction));
    private readonly BrowserNewContextOptions? _contextOptions;
    private readonly string? _customContextId;
    private readonly string? _customPageId;
    private readonly WindowOpenMode _openMode;
    private readonly bool _switchToNew;
    private readonly int _timeoutMs;

    private readonly string? _url;

    /// <summary>
    ///     Represents a Playwright action for opening new browser windows, tabs, or navigating to URLs.
    ///     Supports creating new pages, new browser contexts, and managing window/tab switching.
    /// </summary>
    public OpenWindowAction(Builder builder) : this("open-window", builder)
    {
    }

    /// <summary>
    ///     Playwright action for opening new browser windows/tabs or navigating to URLs.
    ///     Supports creating new pages, new browser contexts, and managing window/tab switching.
    /// </summary>
    public OpenWindowAction(string name, Builder builder) : base(name, builder)
    {
        _url = builder.Url;
        _openMode = builder.OpenMode;
        _customPageId = builder.CustomPageId;
        _customContextId = builder.CustomContextId;
        _switchToNew = builder.SwitchToNew;
        _contextOptions = builder.ContextOptions;
        _timeoutMs = builder.TimeoutMs;
    }

    /// <summary>
    ///     Executes the open window action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Opening window with mode: {OpenMode}, URL: {Url}", _openMode, _url ?? "(none)");

            string newPageId;
            string newContextId;

            switch (_openMode)
            {
                case WindowOpenMode.NEW_TAB:
                    (newPageId, newContextId) = await OpenNewTab(browser, context);
                    break;

                case WindowOpenMode.NEW_WINDOW:
                    (newPageId, newContextId) = await OpenNewWindow(browser, context);
                    break;

                case WindowOpenMode.NEW_CONTEXT:
                    (newPageId, newContextId) = await OpenNewContext(browser, context);
                    break;

                case WindowOpenMode.CURRENT_PAGE:
                    (newPageId, newContextId) = await NavigateCurrentPage(browser, context);
                    break;

                default:
                    throw new ArgumentException($"Unsupported open mode: {_openMode}");
            }

            // Store the new page and context IDs in the test context
            context.SetVariable("NEW_PAGE_ID", newPageId);
            context.SetVariable("NEW_CONTEXT_ID", newContextId ?? string.Empty);

            // Switch to the new page if requested
            if (_switchToNew)
            {
                browser.SwitchToContext(newContextId ??
                                        throw new InvalidOperationException("No current context available"));
                browser.SwitchToPage(newPageId);
                Logger.LogDebug("Switched to new page: {PageId} in context: {ContextId}", newPageId, newContextId);
            }

            Logger.LogInformation("Window opened successfully. Page ID: {PageId}, Context ID: {ContextId}",
                newPageId, newContextId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to open window");
            throw new AgenixSystemException("Failed to open window", ex);
        }
    }

    /// <summary>
    ///     Opens a new tab in the current context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    /// <returns>The new page ID and context ID</returns>
    private async Task<(string newPageId, string? currentContextId)> OpenNewTab(PlaywrightBrowser browser,
        TestContext context)
    {
        Logger.LogDebug("Opening new tab");

        var currentContextId = browser.CurrentContextId ??
                               throw new InvalidOperationException("No current context available");
        var newPageId = await browser.CreatePageAsync(currentContextId, _customPageId);

        if (!string.IsNullOrEmpty(_url))
        {
            var page = browser.GetPage(newPageId);
            var resolvedUrl = context.ReplaceDynamicContentInString(_url);

            await page.GotoAsync(resolvedUrl, new PageGotoOptions { Timeout = _timeoutMs > 0 ? _timeoutMs : null });

            Logger.LogDebug("Navigated new tab to: {Url}", resolvedUrl);
        }

        return (newPageId, currentContextId);
    }

    /// <summary>
    ///     Opens a new window (new page in current context - Playwright doesn't distinguish between tabs and windows).
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    /// <returns>The new page ID and context ID</returns>
    private async Task<(string pageId, string? contextId)> OpenNewWindow(PlaywrightBrowser browser, TestContext context)
    {
        Logger.LogDebug("Opening new window");

        // In Playwright, there's no distinction between tabs and windows at the API level
        // Both are just new pages in the same context
        return await OpenNewTab(browser, context);
    }

    /// <summary>
    ///     Opens a new browser context with a new page.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    /// <returns>The new page ID and context ID</returns>
    private async Task<(string pageId, string contextId)> OpenNewContext(PlaywrightBrowser browser, TestContext context)
    {
        Logger.LogDebug("Opening new context");

        var newContextId = await browser.CreateContextAsync(_contextOptions, _customContextId);
        var newPageId = await browser.CreatePageAsync(newContextId, _customPageId);

        if (!string.IsNullOrEmpty(_url))
        {
            var page = browser.GetPage(newPageId);
            var resolvedUrl = context.ReplaceDynamicContentInString(_url);

            await page.GotoAsync(resolvedUrl, new PageGotoOptions { Timeout = _timeoutMs > 0 ? _timeoutMs : null });

            Logger.LogDebug("Navigated new context page to: {Url}", resolvedUrl);
        }

        return (newPageId, newContextId);
    }

    /// <summary>
    ///     Navigates the current page to a new URL.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    /// <returns>The current page ID and context ID</returns>
    private async Task<(string pageId, string contextId)> NavigateCurrentPage(PlaywrightBrowser browser,
        TestContext context)
    {
        if (string.IsNullOrEmpty(_url))
        {
            throw new InvalidOperationException("URL must be specified when using CurrentPage mode");
        }

        Logger.LogDebug("Navigating current page");

        var currentPage = browser.GetCurrentPage();
        if (currentPage == null)
        {
            throw new InvalidOperationException("No current page available");
        }

        var resolvedUrl = context.ReplaceDynamicContentInString(_url);
        await currentPage.GotoAsync(resolvedUrl, new PageGotoOptions { Timeout = _timeoutMs > 0 ? _timeoutMs : null });

        Logger.LogDebug("Navigated current page to: {Url}", resolvedUrl);

        var currentPageId = browser.CurrentPageId ?? throw new InvalidOperationException("No current page available");
        var currentContextId = browser.CurrentContextId ??
                               throw new InvalidOperationException("No current context available");

        return (currentPageId, currentContextId);
    }

    /// <summary>
    ///     Builder class for creating OpenWindowAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<OpenWindowAction, Builder>
    {
        internal string? Url { get; private set; }
        internal WindowOpenMode OpenMode { get; private set; } = WindowOpenMode.NEW_TAB;
        internal string? CustomPageId { get; private set; }
        internal string? CustomContextId { get; private set; }
        internal bool SwitchToNew { get; private set; } = true;
        internal BrowserNewContextOptions? ContextOptions { get; private set; }
        internal int TimeoutMs { get; private set; } = 30000;

        /// <summary>
        ///     Sets the URL to navigate to.
        /// </summary>
        /// <param name="url">The URL to open</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithUrl(string url)
        {
            Url = url;
            return this;
        }

        /// <summary>
        ///     Configures the action to open a new tab.
        /// </summary>
        /// <param name="pageId">Optional custom page ID</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder OpenNewTab(string? pageId = null)
        {
            OpenMode = WindowOpenMode.NEW_TAB;
            CustomPageId = pageId;
            return this;
        }

        /// <summary>
        ///     Configures the action to open a new window.
        /// </summary>
        /// <param name="pageId">Optional custom page ID</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder OpenNewWindow(string? pageId = null)
        {
            OpenMode = WindowOpenMode.NEW_WINDOW;
            CustomPageId = pageId;
            return this;
        }

        /// <summary>
        ///     Configures the action to open a new browser context.
        /// </summary>
        /// <param name="contextId">Optional custom context ID</param>
        /// <param name="pageId">Optional custom page ID</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder OpenNewContext(string? contextId = null, string? pageId = null)
        {
            OpenMode = WindowOpenMode.NEW_CONTEXT;
            CustomContextId = contextId;
            CustomPageId = pageId;
            return this;
        }

        /// <summary>
        ///     Configures the action to navigate the current page.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder NavigateCurrentPage()
        {
            OpenMode = WindowOpenMode.CURRENT_PAGE;
            return this;
        }

        /// <summary>
        ///     Sets whether to switch to the newly opened window/tab.
        /// </summary>
        /// <param name="switchTo">Whether to switch to the new window</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSwitchToNew(bool switchTo = true)
        {
            SwitchToNew = switchTo;
            return this;
        }

        /// <summary>
        ///     Sets context options for new context mode.
        /// </summary>
        /// <param name="options">The context options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithContextOptions(BrowserNewContextOptions options)
        {
            ContextOptions = options;
            return this;
        }

        /// <summary>
        ///     Configures context options using a fluent builder.
        /// </summary>
        /// <param name="configureOptions">Action to configure context options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithContextOptions(Action<ContextOptionsBuilder> configureOptions)
        {
            var builder = new ContextOptionsBuilder();
            configureOptions(builder);
            ContextOptions = builder.Build();
            return this;
        }

        /// <summary>
        ///     Sets the navigation timeout.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(int timeoutMs)
        {
            TimeoutMs = timeoutMs;
            return this;
        }

        /// <summary>
        ///     Sets a custom user agent for new contexts.
        /// </summary>
        /// <param name="userAgent">The user agent string</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithUserAgent(string userAgent)
        {
            ContextOptions ??= new BrowserNewContextOptions();
            ContextOptions.UserAgent = userAgent;
            return this;
        }

        /// <summary>
        ///     Sets the viewport size for new contexts.
        /// </summary>
        /// <param name="width">Viewport width</param>
        /// <param name="height">Viewport height</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithViewportSize(int width, int height)
        {
            ContextOptions ??= new BrowserNewContextOptions();
            ContextOptions.ViewportSize = new ViewportSize { Width = width, Height = height };
            return this;
        }

        /// <summary>
        ///     Builds the OpenWindowAction instance.
        /// </summary>
        /// <returns>A new OpenWindowAction instance</returns>
        public override OpenWindowAction Build()
        {
            return new OpenWindowAction(this);
        }
    }

    /// <summary>
    ///     Builder for configuring browser context options.
    /// </summary>
    public class ContextOptionsBuilder
    {
        private readonly BrowserNewContextOptions _options = new();

        /// <summary>
        ///     Sets the user agent.
        /// </summary>
        /// <param name="userAgent">The user agent string</param>
        /// <returns>The builder instance for method chaining</returns>
        public ContextOptionsBuilder WithUserAgent(string userAgent)
        {
            _options.UserAgent = userAgent;
            return this;
        }

        /// <summary>
        ///     Sets the viewport size.
        /// </summary>
        /// <param name="width">Viewport width</param>
        /// <param name="height">Viewport height</param>
        /// <returns>The builder instance for method chaining</returns>
        public ContextOptionsBuilder WithViewportSize(int width, int height)
        {
            _options.ViewportSize = new ViewportSize { Width = width, Height = height };
            return this;
        }

        /// <summary>
        ///     Sets whether to ignore HTTPS errors.
        /// </summary>
        /// <param name="ignore">Whether to ignore HTTPS errors</param>
        /// <returns>The builder instance for method chaining</returns>
        public ContextOptionsBuilder WithIgnoreHttpsErrors(bool ignore = true)
        {
            _options.IgnoreHTTPSErrors = ignore;
            return this;
        }

        /// <summary>
        ///     Sets whether JavaScript is enabled.
        /// </summary>
        /// <param name="enabled">Whether JavaScript is enabled</param>
        /// <returns>The builder instance for method chaining</returns>
        public ContextOptionsBuilder WithJavaScriptEnabled(bool enabled = true)
        {
            _options.JavaScriptEnabled = enabled;
            return this;
        }

        /// <summary>
        ///     Sets device scale factor.
        /// </summary>
        /// <param name="scaleFactor">The device scale factor</param>
        /// <returns>The builder instance for method chaining</returns>
        public ContextOptionsBuilder WithDeviceScaleFactor(float scaleFactor)
        {
            _options.DeviceScaleFactor = scaleFactor;
            return this;
        }

        /// <summary>
        ///     Builds the context options.
        /// </summary>
        /// <returns>The configured context options</returns>
        internal BrowserNewContextOptions Build()
        {
            return _options;
        }
    }
}
