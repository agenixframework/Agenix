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
///     Playwright action for closing a browser window/tab.
///     This action closes the current page/tab or a specific page if specified.
/// </summary>
public class CloseWindowAction : AbstractPlaywrightAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(CloseWindowAction));

    private readonly bool _closeAllPages;
    private readonly bool _runBeforeUnload;

    /// <summary>
    ///     Represents a Playwright action designed to close browser windows or tabs.
    /// </summary>
    public CloseWindowAction(Builder builder) : this("close-window", builder) { }

    /// <summary>
    ///     Represents a Playwright action specifically designed for closing browser windows or tabs.
    /// </summary>
    public CloseWindowAction(string name, Builder builder) : base(name, builder)
    {
        _closeAllPages = builder.CloseAllThePages;
        _runBeforeUnload = builder.RunBeforeUnload;
    }

    /// <summary>
    ///     Executes the close window action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing close window action, closeAllPages: {CloseAllPages}", _closeAllPages);

            // Check if the browser is started
            if (!browser.IsStarted)
            {
                Logger.LogWarning("Browser is not started, skipping close window action");
                return;
            }

            var browserContext = browser.GetCurrentContext();
            if (browserContext == null)
            {
                Logger.LogWarning("No active browser context, skipping close window action");
                return;
            }

            if (_closeAllPages)
            {
                Logger.LogDebug("Closing all pages in context");
                await CloseAllPages(browserContext);
            }
            else
            {
                Logger.LogDebug("Closing current page");
                await CloseCurrentPage(browser);
            }

            Logger.LogInformation("Close window action completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute close window action");
            throw new AgenixSystemException("Failed to execute close window action", ex);
        }
    }

    /// <summary>
    ///     Closes the current page.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    private async Task CloseCurrentPage(PlaywrightBrowser browser)
    {
        var currentPage = browser.GetCurrentPage();
        if (currentPage == null)
        {
            Logger.LogWarning("No current page to close");
            return;
        }

        Logger.LogDebug("Closing current page");

        var options = new PageCloseOptions { RunBeforeUnload = _runBeforeUnload };

        await currentPage.CloseAsync(options);
        Logger.LogDebug("Current page closed successfully");
    }

    /// <summary>
    ///     Closes all pages in the browser context.
    /// </summary>
    /// <param name="browserContext">The browser context</param>
    private async Task CloseAllPages(IBrowserContext browserContext)
    {
        var pages = browserContext.Pages;
        if (pages.Count == 0)
        {
            Logger.LogWarning("No pages to close");
            return;
        }

        Logger.LogDebug("Closing {PageCount} pages", pages.Count);

        var options = new PageCloseOptions { RunBeforeUnload = _runBeforeUnload };

        var closeTasks = pages.Select(page => page.CloseAsync(options));
        await Task.WhenAll(closeTasks);

        Logger.LogDebug("All pages closed successfully");
    }

    /// <summary>
    ///     Builder class for creating CloseWindowAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<CloseWindowAction, Builder>
    {
        internal bool CloseAllThePages { get; private set; }
        internal bool RunBeforeUnload { get; private set; }

        /// <summary>
        ///     Sets the action to close all pages/windows in the current context.
        /// </summary>
        /// <param name="closeAll">True to close all pages, false to close only current page</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithCloseAllPages(bool closeAll = true)
        {
            CloseAllThePages = closeAll;
            return this;
        }

        /// <summary>
        ///     Sets the action to close only the current page/window.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder CloseCurrentPage()
        {
            CloseAllThePages = false;
            return this;
        }

        /// <summary>
        ///     Sets the action to close all pages/windows.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder CloseAllPages()
        {
            CloseAllThePages = true;
            return this;
        }

        /// <summary>
        ///     Sets whether to run beforeunload handlers when closing pages.
        /// </summary>
        /// <param name="runBeforeUnload">True to run beforeunload handlers, false to skip them</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithRunBeforeUnload(bool runBeforeUnload = true)
        {
            RunBeforeUnload = runBeforeUnload;
            return this;
        }

        /// <summary>
        ///     Builds the CloseWindowAction instance.
        /// </summary>
        /// <returns>A new CloseWindowAction instance</returns>
        public override CloseWindowAction Build()
        {
            return new CloseWindowAction(this);
        }
    }
}
