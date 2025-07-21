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

using System.Collections.Concurrent;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core.Endpoint;
using Agenix.Playwright.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Agenix.Playwright.Endpoint;

/// <summary>
///     Playwright browser endpoint implementation that provides browser automation capabilities.
///     This class manages browser instances, pages, and contexts for web testing scenarios.
///     Supports multiple browser contexts and multiple pages within each context.
///     Inherits from <see cref="AbstractEndpoint" /> and implements <see cref="IProducer" /> and
///     <see cref="IDisposable" />.
/// </summary>
public class PlaywrightBrowser : AbstractEndpoint, IProducer
{
    /// <summary>
    ///     Logger instance for this class
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(PlaywrightBrowser));

    /// <summary>
    ///     Collection of browser contexts with their IDs
    /// </summary>
    private readonly ConcurrentDictionary<string, IBrowserContext> _contexts = new();

    /// <summary>
    ///     Configuration for this browser instance
    /// </summary>
    private readonly PlaywrightBrowserConfiguration _endpointConfiguration;

    /// <summary>
    ///     Collection of pages with their IDs and associated context IDs
    /// </summary>
    private readonly ConcurrentDictionary<string, (IPage Page, string ContextId)> _pages = new();

    private readonly object _shutdownLock = new();

    private readonly object _startLock = new();

    /// <summary>
    ///     Object used to synchronize access to the browser's internal state to ensure thread safety.
    /// </summary>
    private readonly object _stateLock = new();

    /// <summary>
    ///     Lock the object used for synchronizing access to test session-related operations within this class.
    /// </summary>
    private readonly object _testSessionLock = new();

    /// <summary>
    ///     A thread-safe dictionary that maintains active test sessions,
    ///     mapping session identifiers to corresponding <see cref="TestSession" /> instances.
    /// </summary>
    private readonly ConcurrentDictionary<string, TestSession> _testSessions = new();

    /// <summary>
    ///     Browser instance
    /// </summary>
    private IBrowser? _browser;

    /// <summary>
    ///     Counter for generating unique context IDs
    /// </summary>
    private int _contextCounter;

    /// <summary>
    ///     Current active context ID
    /// </summary>
    private string? _currentContextId;

    /// <summary>
    ///     Current active page ID
    /// </summary>
    private string? _currentPageId;

    private volatile bool _isShuttingDown;

    /// <summary>
    ///     Counter for generating unique page IDs
    /// </summary>
    private int _pageCounter;

    /// <summary>
    ///     Playwright instance
    /// </summary>
    private IPlaywright? _playwright;


    /// <summary>
    ///     Represents a Playwright browser endpoint implementation that facilitates automated browser interactions
    ///     for testing and web automation purposes. Manages browser instances, contexts, and pages,
    ///     providing operations critical for end-to-end web testing scenarios with full multi-context and multi-page support.
    ///     Inherits from <see cref="AbstractEndpoint" /> and implements <see cref="IProducer" /> and
    ///     <see cref="IDisposable" />.
    /// </summary>
    public PlaywrightBrowser() : this(new PlaywrightBrowserConfiguration()) { }

    /// <summary>
    ///     Represents a Playwright browser endpoint implementation designed for advanced browser automation and end-to-end web
    ///     testing.
    ///     Provides mechanisms to manage browser instances, contexts, and pages, while integrating with messaging systems and
    ///     resource management.
    ///     Supports multiple browser contexts and multiple pages within each context.
    ///     Inherits from <see cref="AbstractEndpoint" /> and implements <see cref="IProducer" /> and
    ///     <see cref="IDisposable" />.
    /// </summary>
    public PlaywrightBrowser(PlaywrightBrowserConfiguration endpointConfiguration) : base(endpointConfiguration)
    {
        _endpointConfiguration = endpointConfiguration;
    }

    /// <summary>
    ///     Gets the endpoint configuration
    /// </summary>
    public override PlaywrightBrowserConfiguration EndpointConfiguration => _endpointConfiguration;

    /// <summary>
    ///     Gets the current browser instance
    /// </summary>
    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not started");

    /// <summary>
    ///     Gets the current active browser context
    /// </summary>
    public virtual IBrowserContext BrowserContext =>
        GetCurrentContext() ?? throw new InvalidOperationException("No active browser context");

    /// <summary>
    ///     Gets the current active page instance
    /// </summary>
    public IPage Page => GetCurrentPage() ?? throw new InvalidOperationException("No active page");

    /// <summary>
    ///     Gets the Playwright instance
    /// </summary>
    public IPlaywright Playwright => _playwright ?? throw new InvalidOperationException("Playwright not initialized");

    /// <summary>
    ///     Gets the current active context ID
    /// </summary>
    public virtual string? CurrentContextId => _currentContextId;

    /// <summary>
    ///     Gets the current active page ID
    /// </summary>
    public virtual string? CurrentPageId => _currentPageId;

    /// <summary>
    ///     Gets all context IDs
    /// </summary>
    public virtual IReadOnlyCollection<string> ContextIds => _contexts.Keys.ToList().AsReadOnly();

    /// <summary>
    ///     Gets all page IDs
    /// </summary>
    public virtual IReadOnlyCollection<string> PageIds => _pages.Keys.ToList().AsReadOnly();

    /// <summary>
    ///     Indicates whether the browser is started
    /// </summary>
    public virtual bool IsStarted => _browser is { IsConnected: true };

    /// <summary>
    ///     Gets the number of active contexts
    /// </summary>
    public virtual int ContextCount => _contexts.Count;

    /// <summary>
    ///     Gets the number of active pages
    /// </summary>
    public int PageCount => _pages.Count;

    /// <summary>
    ///     Gets the number of active test sessions.
    /// </summary>
    public int ActiveSessionCount => _testSessions.Count;

    /// <summary>
    ///     Sends a message and executes the corresponding Playwright action within the provided test context.
    /// </summary>
    /// <param name="message">The message containing the payload to retrieve and execute an action.</param>
    /// <param name="context">The test context within which the Playwright action will execute.</param>
    public void Send(IMessage message, TestContext context)
    {
        var action = message.GetPayload<IPlaywrightAction>();
        action.Execute(context);

        Log.LogInformation("Playwright action successfully executed");
    }

    /// <summary>
    ///     Starts the browser with the configured settings and creates the default context and page
    /// </summary>
    public async Task StartAsync()
    {
        lock (_startLock)
        {
            if (IsStarted)
            {
                Log.LogWarning("Browser is already started");
                return;
            }
        }

        try
        {
            Log.LogDebug("Starting Playwright browser with configuration: {BrowserType}",
                _endpointConfiguration.BrowserType);

            // Initialize Playwright
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            // Get a browser type
            var browserType = _endpointConfiguration.BrowserType.ToLower() switch
            {
                "firefox" => playwright.Firefox,
                "webkit" => playwright.Webkit,
                "chromium" or "chrome" => playwright.Chromium,
                _ => throw new AgenixSystemException($"Unsupported browser type: {_endpointConfiguration.BrowserType}")
            };

            // Create launch options
            var launchOptions = _endpointConfiguration.LaunchOptions ?? new BrowserTypeLaunchOptions();

            // Apply configuration to launch options
            launchOptions.Headless = _endpointConfiguration.Headless;
            launchOptions.Channel = _endpointConfiguration.Channel;
            launchOptions.ExecutablePath = _endpointConfiguration.ExecutablePath;

            // Launch browser
            var browser = await browserType.LaunchAsync(launchOptions);

            // Atomically assign the initialized objects
            lock (_startLock)
            {
                if (IsStarted)
                {
                    // Another thread completed initialization, cleanup and return
                    try
                    {
                        browser.CloseAsync();
                        playwright.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.LogWarning(ex, "Failed to cleanup duplicate browser initialization");
                    }

                    Log.LogWarning("Browser was already started by another thread");
                    return;
                }

                _playwright = playwright;
                _browser = browser;
            }

            // Create the default context and page
            await CreateDefaultContextAndPageAsync();

            Log.LogDebug("Playwright browser started successfully with {ContextCount} contexts and {PageCount} pages",
                ContextCount, PageCount);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to start Playwright browser");
            await StopAsync();
            throw new AgenixSystemException("Failed to start Playwright browser", ex);
        }
    }


    /// <summary>
    ///     Creates the default context and page
    /// </summary>
    private async Task CreateDefaultContextAndPageAsync()
    {
        // Create default context
        var defaultContextId = await CreateContextAsync();

        // Create the default page in the default context
        var defaultPageId = await CreatePageAsync(defaultContextId);

        // Navigate to the start page if specified
        if (!string.IsNullOrEmpty(_endpointConfiguration.StartPageUrl) &&
            _endpointConfiguration.StartPageUrl != "about:blank")
        {
            await NavigateToAsync(defaultPageId, _endpointConfiguration.StartPageUrl);
        }

        Log.LogDebug("Created default context {ContextId} and page {PageId}", defaultContextId, defaultPageId);
    }

    /// <summary>
    ///     Creates a new browser context with the specified options
    /// </summary>
    /// <param name="contextOptions">Optional context options. If null, uses default configuration</param>
    /// <param name="contextId">Optional custom context ID. If null, generates a unique ID</param>
    /// <returns>The ID of the created context</returns>
    public async Task<string> CreateContextAsync(BrowserNewContextOptions? contextOptions = null,
        string? contextId = null)
    {
        if (!IsStarted)
        {
            throw new InvalidOperationException("Browser not started");
        }

        contextId ??= GenerateContextId();

        try
        {
            // Prepare everything outside the lock
            var options = contextOptions ?? _endpointConfiguration.ContextOptions ?? new BrowserNewContextOptions();

            if (contextOptions == null)
            {
                ApplyContextConfiguration(options);
            }

            // Create browser context
            var context = await _browser!.NewContextAsync(options);

            // Apply event handlers
            var handlers = _endpointConfiguration.ContextEventHandlers.ToList();
            foreach (var handler in handlers)
            {
                try
                {
                    handler(context);
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "Failed to apply context event handler");
                }
            }

            // Store context atomically
            lock (_stateLock)
            {
                if (!_contexts.TryAdd(contextId, context))
                {
                    // Clean up and throw
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await context.CloseAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.LogWarning(ex, "Failed to cleanup duplicate context");
                        }
                    });

                    throw new ArgumentException($"Context with ID '{contextId}' already exists");
                }

                _currentContextId ??= contextId;
            }

            Log.LogDebug("Created new context with ID: {ContextId}", contextId);
            return contextId;
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to create context with ID: {ContextId}", contextId);
            throw new AgenixSystemException($"Failed to create context with ID: {contextId}", ex);
        }
    }


    /// <summary>
    ///     Creates a new page in the specified context
    /// </summary>
    /// <param name="contextId">The ID of the context to create the page in. If null, uses current context</param>
    /// <param name="pageId">Optional custom page ID. If null, generates a unique ID</param>
    /// <returns>The ID of the created page</returns>
    public virtual async Task<string> CreatePageAsync(string? contextId = null, string? pageId = null)
    {
        pageId ??= GeneratePageId();

        try
        {
            // Prepare everything we need atomically
            string resolvedContextId;
            IBrowserContext context;

            lock (_stateLock)
            {
                resolvedContextId = contextId ?? _currentContextId ??
                    throw new InvalidOperationException("No active context");

                if (!_contexts.TryGetValue(resolvedContextId, out context))
                {
                    throw new ArgumentException($"Context with ID '{resolvedContextId}' not found");
                }
            }

            // Create page outside of lock
            var page = await context.NewPageAsync();

            // Apply configuration outside of lock
            var handlers = _endpointConfiguration.PageEventHandlers.ToList();
            foreach (var handler in handlers)
            {
                try
                {
                    handler(page);
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "Failed to apply page event handler");
                }
            }

            // Set timeouts
            page.SetDefaultTimeout(_endpointConfiguration.DefaultTimeout);
            page.SetDefaultNavigationTimeout(_endpointConfiguration.DefaultNavigationTimeout);

            // Store page atomically
            lock (_stateLock)
            {
                if (_pages.ContainsKey(pageId))
                {
                    // Clean up and throw
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await page.CloseAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.LogWarning(ex, "Failed to cleanup duplicate page");
                        }
                    });

                    throw new ArgumentException($"Page with ID '{pageId}' already exists");
                }

                _pages[pageId] = (page, resolvedContextId);
                _currentPageId ??= pageId;
            }

            Log.LogDebug("Created new page with ID: {PageId} in context: {ContextId}", pageId, resolvedContextId);
            return pageId;
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to create page with ID: {PageId}", pageId);
            throw new AgenixSystemException($"Failed to create page with ID: {pageId}", ex);
        }
    }

    /// <summary>
    ///     Switches to the specified context
    /// </summary>
    /// <param name="contextId">The ID of the context to switch to</param>
    public virtual void SwitchToContext(string contextId)
    {
        if (string.IsNullOrEmpty(contextId))
        {
            throw new ArgumentException("Context ID cannot be null or empty", nameof(contextId));
        }

        lock (_stateLock)
        {
            // Validate context exists
            if (!_contexts.ContainsKey(contextId))
            {
                throw new ArgumentException($"Context with ID '{contextId}' not found");
            }

            // If already on the target context, do nothing
            if (_currentContextId == contextId)
            {
                Log.LogDebug("Already on context '{ContextId}', no switch needed", contextId);
                return;
            }

            var previousContextId = _currentContextId;
            _currentContextId = contextId;

            // Update current page efficiently
            UpdateCurrentPageForContext(contextId);

            Log.LogDebug("Switched from context '{PreviousContext}' to context '{CurrentContext}'",
                previousContextId, contextId);
        }
    }

    private void UpdateCurrentPageForContext(string contextId)
    {
        // Check if the current page is valid and belongs to the new context
        if (_currentPageId != null &&
            _pages.TryGetValue(_currentPageId, out var currentPageInfo) &&
            currentPageInfo.ContextId == contextId)
        {
            // The current page is already in the target context, keep it
            return;
        }

        // Find the first page in the new context
        foreach (var pageEntry in _pages)
        {
            if (pageEntry.Value.ContextId == contextId)
            {
                _currentPageId = pageEntry.Key;
                return;
            }
        }

        // No pages found in the context
        _currentPageId = null;
        Log.LogDebug("No pages found in context '{ContextId}'", contextId);
    }


    /// <summary>
    ///     Switches to the specified page
    /// </summary>
    /// <param name="pageId">The ID of the page to switch to</param>
    public virtual void SwitchToPage(string pageId)
    {
        if (string.IsNullOrEmpty(pageId))
        {
            throw new ArgumentException("Page ID cannot be null or empty", nameof(pageId));
        }

        lock (_stateLock)
        {
            // Validate page exists and gets its info atomically
            if (!_pages.TryGetValue(pageId, out var pageInfo))
            {
                throw new ArgumentException($"Page with ID '{pageId}' not found");
            }

            // Early return if already on the target page
            if (_currentPageId == pageId)
            {
                Log.LogDebug("Already on page '{PageId}', no switch needed", pageId);
                return;
            }

            // Validate that the page's context still exists
            if (!_contexts.ContainsKey(pageInfo.ContextId))
            {
                throw new InvalidOperationException(
                    $"Page '{pageId}' belongs to context '{pageInfo.ContextId}' which no longer exists");
            }

            var previousPageId = _currentPageId;
            var previousContextId = _currentContextId;

            // Update current page and context atomically
            _currentPageId = pageId;
            _currentContextId = pageInfo.ContextId;

            Log.LogDebug(
                "Switched from page '{PreviousPage}' (context '{PreviousContext}') to page '{CurrentPage}' (context '{CurrentContext}')",
                previousPageId, previousContextId, pageId, pageInfo.ContextId);
        }
    }

    /// <summary>
    ///     Gets a specific context by ID
    /// </summary>
    /// <param name="contextId">The ID of the context to get</param>
    /// <returns>The browser context</returns>
    public virtual IBrowserContext GetContext(string contextId)
    {
        if (!_contexts.TryGetValue(contextId, out var context))
        {
            throw new ArgumentException($"Context with ID '{contextId}' not found");
        }

        return context;
    }

    /// <summary>
    ///     Gets a specific page by ID
    /// </summary>
    /// <param name="pageId">The ID of the page to get</param>
    /// <returns>The page</returns>
    public virtual IPage GetPage(string pageId)
    {
        if (!_pages.TryGetValue(pageId, out var pageInfo))
        {
            throw new ArgumentException($"Page with ID '{pageId}' not found");
        }

        return pageInfo.Page;
    }

    /// <summary>
    ///     Gets all pages in the specified context
    /// </summary>
    /// <param name="contextId">The ID of the context</param>
    /// <returns>A dictionary of page IDs and pages</returns>
    public virtual IReadOnlyDictionary<string, IPage> GetPagesInContext(string contextId)
    {
        return _pages
            .Where(p => p.Value.ContextId == contextId)
            .ToDictionary(p => p.Key, p => p.Value.Page)
            .AsReadOnly();
    }

    /// <summary>
    ///     Gets the current active context
    /// </summary>
    /// <returns>The current context or null if none is active</returns>
    public virtual IBrowserContext? GetCurrentContext()
    {
        lock (_stateLock)
        {
            return _currentContextId != null && _contexts.TryGetValue(_currentContextId, out var context)
                ? context
                : null;
        }
    }

    /// <summary>
    ///     Gets the current active page
    /// </summary>
    /// <returns>The current page or null if none is active</returns>
    public virtual IPage? GetCurrentPage()
    {
        lock (_stateLock)
        {
            return _currentPageId != null && _pages.TryGetValue(_currentPageId, out var pageInfo)
                ? pageInfo.Page
                : null;
        }
    }

    /// <summary>
    ///     Navigates the specified page to the given URL
    /// </summary>
    /// <param name="pageId">The ID of the page to navigate. If null, uses current page</param>
    /// <param name="url">The URL to navigate to</param>
    /// <param name="options">Optional navigation options</param>
    public async Task NavigateToAsync(string? pageId, string url, PageGotoOptions? options = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        IPage page;
        string resolvedPageId;

        lock (_stateLock)
        {
            // Resolve page ID atomically
            resolvedPageId = pageId ?? _currentPageId ?? throw new InvalidOperationException("No active page");

            // Get page info atomically
            if (!_pages.TryGetValue(resolvedPageId, out var pageInfo))
            {
                throw new ArgumentException($"Page with ID '{resolvedPageId}' not found");
            }

            page = pageInfo.Page;
        }

        // Navigate outside the lock to avoid blocking other operations
        await page.GotoAsync(url, options);
        Log.LogDebug("Navigated page {PageId} to: {Url}", resolvedPageId, url);
    }

    /// <summary>
    ///     Closes the specified context and all its pages
    /// </summary>
    /// <param name="contextId">The ID of the context to close</param>
    public async Task CloseContextAsync(string contextId)
    {
        // Step 1: Remove everything from the shared state atomically
        var (context, pagesToClose) = RemoveContextFromState(contextId);

        if (context == null)
        {
            throw new ArgumentException($"Context with ID '{contextId}' not found");
        }

        // Step 2: Clean up resources (safe to do outside lock)
        await CleanupResources(contextId, context, pagesToClose);
    }

    private (IBrowserContext context, List<(string pageId, IPage page)> pages) RemoveContextFromState(string contextId)
    {
        lock (_stateLock)
        {
            // Try to remove context
            if (!_contexts.TryRemove(contextId, out var context))
            {
                return (null, new List<(string, IPage)>());
            }

            // Find and remove all pages for this context
            var pagesToClose = new List<(string, IPage)>();
            var pageKeysToRemove = new List<string>();

            foreach (var page in _pages)
            {
                if (page.Value.ContextId == contextId)
                {
                    pageKeysToRemove.Add(page.Key);
                    pagesToClose.Add((page.Key, page.Value.Page));
                }
            }

            // Remove pages
            foreach (var pageId in pageKeysToRemove)
            {
                _pages.TryRemove(pageId, out _);
            }

            // Update the current context if needed
            if (_currentContextId == contextId)
            {
                SetNewCurrentContext();
            }

            return (context, pagesToClose);
        }
    }

    private void SetNewCurrentContext()
    {
        _currentContextId = _contexts.Keys.FirstOrDefault();
        _currentPageId = null;

        if (_currentContextId != null)
        {
            var firstPage = _pages.FirstOrDefault(p => p.Value.ContextId == _currentContextId);
            _currentPageId = firstPage.Key;
        }
    }


    /// <summary>
    ///     Closes the specified page
    /// </summary>
    /// <param name="pageId">The ID of the page to close</param>
    public async Task ClosePageAsync(string pageId)
    {
        IPage pageToClose;

        // Remove the page atomically
        lock (_stateLock)
        {
            if (!_pages.TryRemove(pageId, out var pageInfo))
            {
                throw new ArgumentException($"Page with ID '{pageId}' not found");
            }

            pageToClose = pageInfo.Page;

            // Update the current page if needed
            if (_currentPageId == pageId)
            {
                UpdateCurrentPage();
            }
        }

        // Close the page outside the lock
        try
        {
            await pageToClose.CloseAsync();
            Log.LogDebug("Closed page: {PageId}", pageId);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to close page: {PageId}", pageId);
            throw new AgenixSystemException($"Failed to close page: {pageId}", ex);
        }
    }

    private void UpdateCurrentPage()
    {
        // Already inside lock
        _currentPageId = null;

        if (_currentContextId != null)
        {
            // Find first page in current context
            foreach (var page in _pages)
            {
                if (page.Value.ContextId == _currentContextId)
                {
                    _currentPageId = page.Key;
                    return;
                }
            }
        }

        // Fallback: take any page
        _currentPageId = _pages.Keys.FirstOrDefault();
    }

    /// <summary>
    ///     Starts a new test session with appropriate isolation based on configuration.
    /// </summary>
    /// <param name="testId">Unique identifier for the test</param>
    /// <param name="contextOptions">Optional context configuration for the test</param>
    /// <returns>The test session information</returns>
    public async Task<TestSession> StartTestSessionAsync(string testId, BrowserNewContextOptions? contextOptions = null)
    {
        if (!EndpointConfiguration.TestIsolation)
        {
            return new TestSession(testId, CurrentContextId!, [CurrentPageId!], DateTime.UtcNow);
        }

        // Create session first
        var session = EndpointConfiguration.TestIsolationMode switch
        {
            TestIsolationMode.NEW_CONTEXT_PER_TEST => await CreateNewContextSessionAsync(testId, contextOptions),
            TestIsolationMode.NEW_PAGE_PER_TEST => await CreateNewPageSessionAsync(testId),
            TestIsolationMode.NONE => new TestSession(testId, CurrentContextId!, [CurrentPageId!], DateTime.UtcNow),
            _ => throw new AgenixSystemException(nameof(EndpointConfiguration.TestIsolationMode))
        };

        // Atomic add - fails if the key already exists
        if (!_testSessions.TryAdd(testId, session))
        {
            // Clean up resources if session creation involved async operations
            if (session.ContextId != CurrentContextId)
            {
                await CloseContextAsync(session.ContextId);
            }

            throw new InvalidOperationException($"Test session '{testId}' already exists");
        }

        // Still need synchronization for browser state changes
        lock (_testSessionLock)
        {
            SwitchToContext(session.ContextId);
            if (session.PageIds.Count > 0)
            {
                SwitchToPage(session.PageIds[0]);
            }
        }

        Log.LogDebug("Started test session {TestId} with context {ContextId} and {PageCount} pages",
            testId, session.ContextId, session.PageIds.Count);

        return session;
    }

    /// <summary>
    ///     Ends a test session and cleans up associated resources.
    /// </summary>
    /// <param name="testId">The test identifier</param>
    public async Task EndTestSessionAsync(string testId)
    {
        if (!_testSessions.TryRemove(testId, out var session))
        {
            Log.LogWarning("Test session {TestId} not found", testId);
            return;
        }

        if (!EndpointConfiguration.TestIsolation)
        {
            return;
        }

        try
        {
            switch (EndpointConfiguration.TestIsolationMode)
            {
                case TestIsolationMode.NEW_CONTEXT_PER_TEST:
                    await CloseContextAsync(session.ContextId);
                    break;

                case TestIsolationMode.NEW_PAGE_PER_TEST:
                    var closeTasks = session.PageIds.Select(ClosePageAsync);
                    await Task.WhenAll(closeTasks);
                    break;

                case TestIsolationMode.NONE:
                    // No cleanup needed
                    break;
            }

            Log.LogDebug("Ended test session {TestId} after {Duration}ms",
                testId, session.Duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "Failed to cleanup test session {TestId}", testId);
        }
    }

    /// <summary>
    ///     Creates a new context session for test isolation.
    /// </summary>
    private async Task<TestSession> CreateNewContextSessionAsync(string testId,
        BrowserNewContextOptions? contextOptions)
    {
        var contextId = await CreateContextAsync(contextOptions);
        var pageId = await CreatePageAsync(contextId);

        // Navigate to the start page if specified
        if (!string.IsNullOrEmpty(EndpointConfiguration.StartPageUrl) &&
            EndpointConfiguration.StartPageUrl != "about:blank")
        {
            await NavigateToAsync(pageId, EndpointConfiguration.StartPageUrl);
        }

        return new TestSession(testId, contextId, [pageId], DateTime.UtcNow);
    }

    /// <summary>
    ///     Creates a new page session for test isolation.
    /// </summary>
    private async Task<TestSession> CreateNewPageSessionAsync(string testId)
    {
        var pageId = await CreatePageAsync(CurrentContextId);

        // Navigate to the start page if specified
        if (!string.IsNullOrEmpty(EndpointConfiguration.StartPageUrl) &&
            EndpointConfiguration.StartPageUrl != "about:blank")
        {
            await NavigateToAsync(pageId, EndpointConfiguration.StartPageUrl);
        }

        return new TestSession(testId, CurrentContextId!, [pageId], DateTime.UtcNow);
    }

    /// <summary>
    ///     Gets the current test session information.
    /// </summary>
    /// <param name="testId">The test identifier</param>
    /// <returns>The test session or null if not found</returns>
    public TestSession? GetTestSession(string testId)
    {
        return _testSessions.GetValueOrDefault(testId);
    }

    /// <summary>
    ///     Gets all active test sessions.
    /// </summary>
    public IReadOnlyCollection<TestSession> GetActiveTestSessions()
    {
        return _testSessions.Values.ToList().AsReadOnly();
    }


    /// <summary>
    ///     Stops the browser and releases all resources
    /// </summary>
    public async Task StopAsync()
    {
        // Prevent concurrent shutdown
        lock (_shutdownLock)
        {
            if (_isShuttingDown || !IsStarted)
            {
                Log.LogWarning(_isShuttingDown ? "Browser is already shutting down" : "Browser is not started");
                return;
            }

            _isShuttingDown = true;
        }

        try
        {
            Log.LogDebug("Stopping Playwright browser");

            // Atomically get all resources and clear collections
            var (pages, contexts, browser, playwright) = ExtractAllResources();

            // Clean up everything in parallel where possible
            await CleanupResources(pages, contexts, browser, playwright);

            Log.LogDebug("Playwright browser stopped successfully");
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error occurred while stopping Playwright browser");
            throw new AgenixSystemException("Failed to stop Playwright browser", ex);
        }
        finally
        {
            lock (_shutdownLock)
            {
                _isShuttingDown = false;
            }
        }
    }

    private (List<IPage> pages, List<IBrowserContext> contexts, IBrowser? browser, IPlaywright? playwright)
        ExtractAllResources()
    {
        lock (_stateLock)
        {
            var pages = _pages.Values.Select(p => p.Page).ToList();
            var contexts = _contexts.Values.ToList();
            var browser = _browser;
            var playwright = _playwright;

            // Clear everything
            _pages.Clear();
            _contexts.Clear();
            _browser = null;
            _playwright = null;
            _currentContextId = null;
            _currentPageId = null;

            return (pages, contexts, browser, playwright);
        }
    }

    private static async Task CleanupResources(string contextId, IBrowserContext context,
        List<(string pageId, IPage page)> pages)
    {
        try
        {
            // Close all pages in parallel
            var closeTasks = pages.Select(async p =>
            {
                try
                {
                    await p.page.CloseAsync();
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "Failed to close page {PageId}", p.pageId);
                }
            });

            await Task.WhenAll(closeTasks);

            // Close the context
            await context.CloseAsync();

            Log.LogDebug("Closed context: {ContextId}", contextId);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to close context: {ContextId}", contextId);
            throw new AgenixSystemException($"Failed to close context: {contextId}", ex);
        }
    }

    private static async Task CleanupResources(List<IPage> pages, List<IBrowserContext> contexts,
        IBrowser? browser, IPlaywright? playwright)
    {
        // Close pages in parallel
        var pageCloseTasks = pages.Select(async page =>
        {
            try
            {
                await page.CloseAsync();
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex, "Failed to close page during shutdown");
            }
        });

        // Close contexts in parallel
        var contextCloseTasks = contexts.Select(async context =>
        {
            try
            {
                await context.CloseAsync();
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex, "Failed to close context during shutdown");
            }
        });

        // Wait for all pages and contexts to close
        await Task.WhenAll(pageCloseTasks.Concat(contextCloseTasks));

        // Close browser
        if (browser != null)
        {
            try
            {
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex, "Failed to close browser during shutdown");
            }
        }

        // Dispose playwright
        playwright?.Dispose();
    }


    /// <summary>
    ///     Generates a unique context ID
    /// </summary>
    private string GenerateContextId()
    {
        return $"context-{Interlocked.Increment(ref _contextCounter)}";
    }

    /// <summary>
    ///     Generates a unique page ID
    /// </summary>
    private string GeneratePageId()
    {
        return $"page-{Interlocked.Increment(ref _pageCounter)}";
    }

    /// <summary>
    ///     Applies configuration settings to context options
    /// </summary>
    private void ApplyContextConfiguration(BrowserNewContextOptions contextOptions)
    {
        if (_endpointConfiguration.Viewport != null)
        {
            contextOptions.ViewportSize = _endpointConfiguration.Viewport;
        }

        if (!string.IsNullOrEmpty(_endpointConfiguration.UserAgent))
        {
            contextOptions.UserAgent = _endpointConfiguration.UserAgent;
        }

        contextOptions.DeviceScaleFactor = _endpointConfiguration.DeviceScaleFactor;
        contextOptions.HasTouch = _endpointConfiguration.HasTouch;
        contextOptions.IsMobile = _endpointConfiguration.IsMobile;
        contextOptions.JavaScriptEnabled = _endpointConfiguration.JavaScript;
        contextOptions.IgnoreHTTPSErrors = _endpointConfiguration.IgnoreHttpsErrors;
        contextOptions.Offline = _endpointConfiguration.Offline;
        contextOptions.AcceptDownloads = _endpointConfiguration.AcceptDownloads;

        if (_endpointConfiguration.Geolocation != null)
        {
            contextOptions.Geolocation = _endpointConfiguration.Geolocation;
        }

        if (_endpointConfiguration.Permissions.Length > 0)
        {
            contextOptions.Permissions = _endpointConfiguration.Permissions.ToArray();
        }

        if (_endpointConfiguration.ExtraHttpHeaders.Count > 0)
        {
            contextOptions.ExtraHTTPHeaders = _endpointConfiguration.ExtraHttpHeaders;
        }

        if (_endpointConfiguration.HttpCredentials != null)
        {
            contextOptions.HttpCredentials = _endpointConfiguration.HttpCredentials;
        }

        if (!string.IsNullOrEmpty(_endpointConfiguration.Locale))
        {
            contextOptions.Locale = _endpointConfiguration.Locale;
        }

        if (!string.IsNullOrEmpty(_endpointConfiguration.TimezoneId))
        {
            contextOptions.TimezoneId = _endpointConfiguration.TimezoneId;
        }

        contextOptions.ColorScheme = _endpointConfiguration.ColorScheme;
        contextOptions.ReducedMotion = _endpointConfiguration.ReducedMotion;

        if (!string.IsNullOrEmpty(_endpointConfiguration.VideoDir))
        {
            contextOptions.RecordVideoDir = _endpointConfiguration.VideoDir;
            if (_endpointConfiguration.VideoSize != null)
            {
                contextOptions.RecordVideoSize = _endpointConfiguration.VideoSize;
            }
        }
    }

    /// <summary>
    ///     Creates and returns an instance of <see cref="IProducer" /> for managing
    ///     messaging production. This method overrides the base implementation
    ///     to provide a concrete producer instance tied to the current object.
    /// </summary>
    /// <returns>An instance of <see cref="IProducer" /> representing the producer for messaging operations.</returns>
    public override IProducer CreateProducer()
    {
        return this;
    }

    /// <summary>
    ///     Creates a message consumer for processing inbound messages.
    ///     This method is overridden in derived classes to provide specific consumer implementation.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="IConsumer" />, representing the message consumer.
    ///     Throws <see cref="AgenixSystemException" /> if the operation is not supported.
    /// </returns>
    public override IConsumer CreateConsumer()
    {
        throw new AgenixSystemException("Playwright browser must not be used as message consumer");
    }
}
