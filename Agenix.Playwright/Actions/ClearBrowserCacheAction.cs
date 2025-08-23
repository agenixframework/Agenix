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
///     Playwright action for clearing browser cache.
///     This action clears various types of browser data including cache, cookies, storage,
///     and other browsing data from the current browser context.
/// </summary>
public class ClearBrowserCacheAction : AbstractPlaywrightAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(ClearBrowserCacheAction));

    private readonly ClearDataOptions _clearOptions;

    /// <summary>
    ///     Represents an action responsible for clearing the browser cache within a Playwright context.
    ///     This action targets various types of browser data, including cookies, storage, cache, and other browsing data.
    /// </summary>
    public ClearBrowserCacheAction(Builder builder) : this("clear-cache", builder) { }

    /// <summary>
    ///     Represents an action for clearing the browser cache within a Playwright context.
    ///     This includes clearing cookies, storage, cache, and other browsing data.
    /// </summary>
    public ClearBrowserCacheAction(string name, Builder builder) : base(name, builder)
    {
        _clearOptions = builder.ClearOptions;
    }

    /// <summary>
    ///     Executes the browser cache clearing action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance to clear cache from</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Clearing browser cache");

            // Check if the browser is started
            if (!browser.IsStarted)
            {
                throw new InvalidOperationException("Browser is not started");
            }

            // Clear different types of data based on options
            await ClearBrowserData(browser);

            Logger.LogInformation("Browser cache cleared successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to clear browser cache");
            throw new AgenixSystemException("Failed to clear browser cache", ex);
        }
    }

    /// <summary>
    ///     Clears browser data based on the configured options.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    private async Task ClearBrowserData(PlaywrightBrowser browser)
    {
        var currentContext = browser.GetCurrentContext();
        if (currentContext == null)
        {
            throw new InvalidOperationException("No active browser context");
        }

        var tasks = new List<Task>();

        // Clear cookies if requested
        if (_clearOptions.ClearCookies)
        {
            Logger.LogDebug("Clearing cookies");
            tasks.Add(currentContext.ClearCookiesAsync());
        }

        // Clear storage data if requested
        if (_clearOptions.ClearStorage)
        {
            Logger.LogDebug("Clearing storage data");

            var currentPage = browser.GetCurrentPage();
            if (currentPage != null)
            {
                // Clear local storage, session storage, and IndexedDB
                tasks.Add(currentPage.EvaluateAsync("""

                                                                        () => {
                                                                            // Clear localStorage
                                                                            if (typeof localStorage !== 'undefined') {
                                                                                localStorage.clear();
                                                                            }

                                                                            // Clear sessionStorage
                                                                            if (typeof sessionStorage !== 'undefined') {
                                                                                sessionStorage.clear();
                                                                            }

                                                                            // Clear IndexedDB
                                                                            if (typeof indexedDB !== 'undefined') {
                                                                                return new Promise((resolve) => {
                                                                                    indexedDB.databases().then(databases => {
                                                                                        const promises = databases.map(db => {
                                                                                            return new Promise((resolve, reject) => {
                                                                                                const deleteReq = indexedDB.deleteDatabase(db.name);
                                                                                                deleteReq.onsuccess = () => resolve();
                                                                                                deleteReq.onerror = () => resolve(); // Continue even if deletion fails
                                                                                                deleteReq.onblocked = () => resolve(); // Continue even if blocked
                                                                                            });
                                                                                        });
                                                                                        Promise.all(promises).then(() => resolve());
                                                                                    }).catch(() => resolve()); // Continue even if databases() fails
                                                                                });
                                                                            }
                                                                            return Promise.resolve();
                                                                        }

                                                    """));
            }
        }

        // For cache clearing, we can use CDP (Chrome DevTools Protocol) if available
        if (_clearOptions.ClearCache)
        {
            Logger.LogDebug("Clearing browser cache");

            try
            {
                // Try to use CDP to clear cache (works for Chromium-based browsers)
                var cdpSession = await currentContext.NewCDPSessionAsync(browser.Page);
                await cdpSession.SendAsync("Network.clearBrowserCache");
                await cdpSession.DetachAsync();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex,
                    "Failed to clear cache using CDP, this may not be supported for the current browser type");

                // Fallback: force reloads pages to bypass cache
                var currentPage = browser.GetCurrentPage();
                if (currentPage != null)
                {
                    await currentPage.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.Load });
                }
            }
        }

        // Execute all clearing tasks
        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    ///     Configuration options for specifying what browser data to clear.
    /// </summary>
    public class ClearDataOptions
    {
        /// <summary>
        ///     Gets or sets whether to clear cookies. Default is true.
        /// </summary>
        public bool ClearCookies { get; set; } = true;

        /// <summary>
        ///     Gets or sets whether to clear storage data (localStorage, sessionStorage, IndexedDB). Default is true.
        /// </summary>
        public bool ClearStorage { get; set; } = true;

        /// <summary>
        ///     Gets or sets whether to clear a browser cache. Default is true.
        /// </summary>
        public bool ClearCache { get; set; } = true;
    }

    /// <summary>
    ///     Builder class for creating ClearBrowserCacheAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<ClearBrowserCacheAction, Builder>
    {
        internal ClearDataOptions ClearOptions { get; } = new();

        /// <summary>
        ///     Sets whether to clear cookies.
        /// </summary>
        /// <param name="clear">True to clear cookies, false to skip</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClearCookies(bool clear = true)
        {
            ClearOptions.ClearCookies = clear;
            return this;
        }

        /// <summary>
        ///     Sets whether to clear storage data (localStorage, sessionStorage, IndexedDB).
        /// </summary>
        /// <param name="clear">True to clear storage, false to skip</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClearStorage(bool clear = true)
        {
            ClearOptions.ClearStorage = clear;
            return this;
        }

        /// <summary>
        ///     Sets whether to clear browser cache.
        /// </summary>
        /// <param name="clear">True to clear cache, false to skip</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClearCache(bool clear = true)
        {
            ClearOptions.ClearCache = clear;
            return this;
        }

        /// <summary>
        ///     Configures to clear all browser data (cookies, storage, and cache).
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClearAll()
        {
            ClearOptions.ClearCookies = true;
            ClearOptions.ClearStorage = true;
            ClearOptions.ClearCache = true;
            return this;
        }

        /// <summary>
        ///     Configures to clear only cookies.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClearCookiesOnly()
        {
            ClearOptions.ClearCookies = true;
            ClearOptions.ClearStorage = false;
            ClearOptions.ClearCache = false;
            return this;
        }

        /// <summary>
        ///     Configures to clear only storage data.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClearStorageOnly()
        {
            ClearOptions.ClearCookies = false;
            ClearOptions.ClearStorage = true;
            ClearOptions.ClearCache = false;
            return this;
        }

        /// <summary>
        ///     Builds the ClearBrowserCacheAction instance.
        /// </summary>
        /// <returns>A new ClearBrowserCacheAction instance</returns>
        public override ClearBrowserCacheAction Build()
        {
            return new ClearBrowserCacheAction(this);
        }
    }
}
