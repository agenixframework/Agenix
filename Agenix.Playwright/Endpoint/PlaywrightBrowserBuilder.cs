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

using Agenix.Core.Endpoint;
using Microsoft.Playwright;

namespace Agenix.Playwright.Endpoint;

/// <summary>
///     Builder class for creating and configuring PlaywrightBrowser instances.
///     Provides a fluent API for setting various browser configuration options.
/// </summary>
public class PlaywrightBrowserBuilder : AbstractEndpointBuilder<PlaywrightBrowser>
{
    /// <summary>
    ///     The endpoint target being built
    /// </summary>
    private readonly PlaywrightBrowser _endpoint = new();

    /// <summary>
    ///     Gets the endpoint being built
    /// </summary>
    /// <returns>The PlaywrightBrowser instance</returns>
    protected override PlaywrightBrowser GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    ///     Sets the browser type
    /// </summary>
    /// <param name="type">Browser type (e.g., "chromium", "firefox", "webkit")</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Type(string type)
    {
        _endpoint.EndpointConfiguration.BrowserType = type;
        return this;
    }

    /// <summary>
    ///     Sets whether to run in headless mode
    /// </summary>
    /// <param name="headless">True to run headless, false for headed</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Headless(bool headless)
    {
        _endpoint.EndpointConfiguration.Headless = headless;
        return this;
    }

    /// <summary>
    ///     Sets the start page URL
    /// </summary>
    /// <param name="url">Initial page URL to navigate to</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder StartPage(string url)
    {
        _endpoint.EndpointConfiguration.StartPageUrl = url;
        return this;
    }

    /// <summary>
    ///     Sets the browser channel
    /// </summary>
    /// <param name="channel">Browser channel (stable, beta, dev, canary)</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Channel(string channel)
    {
        _endpoint.EndpointConfiguration.Channel = channel;
        return this;
    }

    /// <summary>
    ///     Sets the custom executable path for the browser
    /// </summary>
    /// <param name="path">Path to browser executable</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder ExecutablePath(string path)
    {
        _endpoint.EndpointConfiguration.ExecutablePath = path;
        return this;
    }

    /// <summary>
    ///     Sets the viewport size
    /// </summary>
    /// <param name="width">Viewport width</param>
    /// <param name="height">Viewport height</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Viewport(int width, int height)
    {
        _endpoint.EndpointConfiguration.Viewport = new ViewportSize { Width = width, Height = height };
        return this;
    }

    /// <summary>
    ///     Sets the viewport size
    /// </summary>
    /// <param name="viewport">Viewport size settings</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Viewport(ViewportSize viewport)
    {
        _endpoint.EndpointConfiguration.Viewport = viewport;
        return this;
    }

    /// <summary>
    ///     Sets the default timeout
    /// </summary>
    /// <param name="timeout">Timeout value in milliseconds</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Timeout(float timeout)
    {
        _endpoint.EndpointConfiguration.DefaultTimeout = timeout;
        return this;
    }

    /// <summary>
    ///     Sets the default navigation timeout
    /// </summary>
    /// <param name="timeout">Navigation timeout value in milliseconds</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder NavigationTimeout(float timeout)
    {
        _endpoint.EndpointConfiguration.DefaultNavigationTimeout = timeout;
        return this;
    }

    /// <summary>
    ///     Sets whether to ignore HTTPS errors
    /// </summary>
    /// <param name="ignore">True to ignore HTTPS errors</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder IgnoreHttpsErrors(bool ignore)
    {
        _endpoint.EndpointConfiguration.IgnoreHttpsErrors = ignore;
        return this;
    }

    /// <summary>
    ///     Sets the JavaScript enabled flag
    /// </summary>
    /// <param name="enabled">True to enable JavaScript, false to disable</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder JavaScript(bool enabled)
    {
        _endpoint.EndpointConfiguration.JavaScript = enabled;
        return this;
    }

    /// <summary>
    ///     Sets the user agent string
    /// </summary>
    /// <param name="userAgent">User agent string</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder UserAgent(string userAgent)
    {
        _endpoint.EndpointConfiguration.UserAgent = userAgent;
        return this;
    }

    /// <summary>
    ///     Sets the device scale factor
    /// </summary>
    /// <param name="scaleFactor">Device scale factor</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder DeviceScaleFactor(float scaleFactor)
    {
        _endpoint.EndpointConfiguration.DeviceScaleFactor = scaleFactor;
        return this;
    }

    /// <summary>
    ///     Sets whether to enable touch events
    /// </summary>
    /// <param name="hasTouch">True to enable touch events</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder HasTouch(bool hasTouch)
    {
        _endpoint.EndpointConfiguration.HasTouch = hasTouch;
        return this;
    }

    /// <summary>
    ///     Sets whether to enable mobile mode
    /// </summary>
    /// <param name="isMobile">True to enable mobile mode</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder IsMobile(bool isMobile)
    {
        _endpoint.EndpointConfiguration.IsMobile = isMobile;
        return this;
    }

    /// <summary>
    ///     Sets the geolocation
    /// </summary>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="accuracy">Accuracy in meters</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Geolocation(float latitude, float longitude, float? accuracy = null)
    {
        _endpoint.EndpointConfiguration.SetGeolocation(latitude, longitude, accuracy);
        return this;
    }

    /// <summary>
    ///     Sets the geolocation
    /// </summary>
    /// <param name="geolocation">Geolocation settings</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Geolocation(Geolocation geolocation)
    {
        _endpoint.EndpointConfiguration.Geolocation = geolocation;
        return this;
    }

    /// <summary>
    ///     Adds permissions to grant
    /// </summary>
    /// <param name="permissions">Permissions to grant</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Permissions(params string[] permissions)
    {
        _endpoint.EndpointConfiguration.AddPermissions(permissions);
        return this;
    }

    /// <summary>
    ///     Sets the color scheme preference
    /// </summary>
    /// <param name="colorScheme">Color scheme preference</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder ColorScheme(ColorScheme colorScheme)
    {
        _endpoint.EndpointConfiguration.ColorScheme = colorScheme;
        return this;
    }

    /// <summary>
    ///     Sets the reduced motion preference
    /// </summary>
    /// <param name="reducedMotion">Reduced motion preference</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder ReducedMotion(ReducedMotion reducedMotion)
    {
        _endpoint.EndpointConfiguration.ReducedMotion = reducedMotion;
        return this;
    }

    /// <summary>
    ///     Sets the locale for the browser
    /// </summary>
    /// <param name="locale">Locale string (e.g., "en-US")</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Locale(string locale)
    {
        _endpoint.EndpointConfiguration.Locale = locale;
        return this;
    }

    /// <summary>
    ///     Sets the timezone for the browser
    /// </summary>
    /// <param name="timezoneId">Timezone ID (e.g., "America/New_York")</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Timezone(string timezoneId)
    {
        _endpoint.EndpointConfiguration.TimezoneId = timezoneId;
        return this;
    }

    /// <summary>
    ///     Sets HTTP credentials for basic authentication
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder HttpCredentials(string username, string password)
    {
        _endpoint.EndpointConfiguration.HttpCredentials = new HttpCredentials
        {
            Username = username,
            Password = password
        };
        return this;
    }

    /// <summary>
    ///     Sets HTTP credentials for basic authentication
    /// </summary>
    /// <param name="credentials">HTTP credentials</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder HttpCredentials(HttpCredentials credentials)
    {
        _endpoint.EndpointConfiguration.HttpCredentials = credentials;
        return this;
    }

    /// <summary>
    ///     Adds extra HTTP headers
    /// </summary>
    /// <param name="headers">Extra HTTP headers to send with every request</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder ExtraHttpHeaders(Dictionary<string, string> headers)
    {
        _endpoint.EndpointConfiguration.AddExtraHttpHeaders(headers);
        return this;
    }

    /// <summary>
    ///     Sets offline mode
    /// </summary>
    /// <param name="offline">True to enable offline mode</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Offline(bool offline)
    {
        _endpoint.EndpointConfiguration.Offline = offline;
        return this;
    }

    /// <summary>
    ///     Sets whether to accept downloads
    /// </summary>
    /// <param name="acceptDownloads">True to accept downloads</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder AcceptDownloads(bool acceptDownloads)
    {
        _endpoint.EndpointConfiguration.AcceptDownloads = acceptDownloads;
        return this;
    }

    /// <summary>
    ///     Sets video recording directory
    /// </summary>
    /// <param name="videoDir">Directory to save videos</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder VideoDir(string videoDir)
    {
        _endpoint.EndpointConfiguration.VideoDir = videoDir;
        return this;
    }

    /// <summary>
    ///     Sets video recording size
    /// </summary>
    /// <param name="width">Video width</param>
    /// <param name="height">Video height</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder VideoSize(int width, int height)
    {
        _endpoint.EndpointConfiguration.VideoSize = new RecordVideoSize { Width = width, Height = height };
        return this;
    }

    /// <summary>
    ///     Sets video recording size
    /// </summary>
    /// <param name="videoSize">Video size settings</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder VideoSize(RecordVideoSize videoSize)
    {
        _endpoint.EndpointConfiguration.VideoSize = videoSize;
        return this;
    }

    /// <summary>
    ///     Enables trace recording
    /// </summary>
    /// <param name="recordTrace">True to enable trace recording</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder RecordTrace(bool recordTrace)
    {
        _endpoint.EndpointConfiguration.RecordTrace = recordTrace;
        return this;
    }

    /// <summary>
    ///     Sets trace recording directory
    /// </summary>
    /// <param name="traceDir">Directory to save traces</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder TraceDir(string traceDir)
    {
        _endpoint.EndpointConfiguration.TraceDir = traceDir;
        return this;
    }

    /// <summary>
    ///     Sets a custom browser instance
    /// </summary>
    /// <param name="browser">Browser instance to use</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Browser(IBrowser browser)
    {
        _endpoint.EndpointConfiguration.Browser = browser;
        return this;
    }

    /// <summary>
    ///     Sets a custom page instance
    /// </summary>
    /// <param name="page">Page instance to use</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder Page(IPage page)
    {
        _endpoint.EndpointConfiguration.Page = page;
        return this;
    }

    /// <summary>
    ///     Sets browser context options
    /// </summary>
    /// <param name="contextOptions">Browser context options</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder ContextOptions(BrowserNewContextOptions contextOptions)
    {
        _endpoint.EndpointConfiguration.ContextOptions = contextOptions;
        return this;
    }

    /// <summary>
    ///     Sets browser launch options
    /// </summary>
    /// <param name="launchOptions">Browser launch options</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder LaunchOptions(BrowserTypeLaunchOptions launchOptions)
    {
        _endpoint.EndpointConfiguration.LaunchOptions = launchOptions;
        return this;
    }

    /// <summary>
    ///     Sets the WebSocket endpoint to connect to an existing browser instance.
    /// </summary>
    /// <param name="wsEndpoint">WebSocket endpoint (e.g., ws://localhost:9222/devtools/browser/...)</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder ConnectWsEndpoint(string wsEndpoint)
    {
        _endpoint.EndpointConfiguration.ConnectWsEndpoint = wsEndpoint;
        return this;
    }

    /// <summary>
    ///     Sets options for BrowserType.ConnectAsync when connecting to an existing browser.
    /// </summary>
    /// <param name="connectOptions">Connect options</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder ConnectOptions(BrowserTypeConnectOptions connectOptions)
    {
        _endpoint.EndpointConfiguration.ConnectOptions = connectOptions;
        return this;
    }

    /// <summary>
    ///     Sets page event handlers
    /// </summary>
    /// <param name="handlers">List of page event handler actions</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder PageEventHandlers(List<Action<IPage>> handlers)
    {
        _endpoint.EndpointConfiguration.PageEventHandlers = handlers;
        return this;
    }

    /// <summary>
    ///     Adds a single page event handler
    /// </summary>
    /// <param name="handler">Page event handler action to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder AddPageEventHandler(Action<IPage> handler)
    {
        _endpoint.EndpointConfiguration.PageEventHandlers.Add(handler);
        return this;
    }

    /// <summary>
    ///     Sets context event handlers
    /// </summary>
    /// <param name="handlers">List of context event handler actions</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder ContextEventHandlers(List<Action<IBrowserContext>> handlers)
    {
        _endpoint.EndpointConfiguration.ContextEventHandlers = handlers;
        return this;
    }

    /// <summary>
    ///     Adds a single context event handler
    /// </summary>
    /// <param name="handler">Context event handler action to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder AddContextEventHandler(Action<IBrowserContext> handler)
    {
        _endpoint.EndpointConfiguration.ContextEventHandlers.Add(handler);
        return this;
    }

    /// <summary>
    /// Configures the browser to automatically save the storage state.
    /// </summary>
    /// <param name="autoSavePageSource">Indicates whether the storage state should be automatically saved.</param>
    /// <returns>The current instance of <see cref="PlaywrightBrowserBuilder"/> for method chaining.</returns>
    public PlaywrightBrowserBuilder AutoSaveStorageState(bool autoSavePageSource)
    {
        _endpoint.EndpointConfiguration.AutoSaveStorageState = autoSavePageSource;
        return this;
    }

    /// <summary>
    /// Sets the storage state for the Playwright browser instance.
    /// </summary>
    /// <param name="storageState">The storage state to be applied to the browser.</param>
    /// <returns>The PlaywrightBrowserBuilder instance for method chaining.</returns>
    public PlaywrightBrowserBuilder StorageState(string storageState)
    {
        _endpoint.EndpointConfiguration.StorageState = storageState;
        return this;
    }

    /// <summary>
    /// Sets the path to the storage state file.
    /// </summary>
    /// <param name="storageStatePath">The path to the storage state file.</param>
    /// <returns>The current instance of <see cref="PlaywrightBrowserBuilder"/>.</returns>
    public PlaywrightBrowserBuilder StorageStatePath(string storageStatePath)
    {
        _endpoint.EndpointConfiguration.StorageStatePath = storageStatePath;
        return this;
    }

    /// <summary>
    ///     Convenience method to add a page event handler
    /// </summary>
    /// <param name="onRequest">Action to execute on page request</param>
    /// <param name="onResponse">Action to execute on page response</param>
    /// <param name="onPageError">Action to execute on page error</param>
    /// <param name="onConsole">Action to execute on a console message</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder OnPageEvents(
        Action<IPage, IRequest>? onRequest = null,
        Action<IPage, IResponse>? onResponse = null,
        Action<IPage, string>? onPageError = null,
        Action<IPage, IConsoleMessage>? onConsole = null)
    {
        _endpoint.EndpointConfiguration.AddPageHandler(onRequest, onResponse, onPageError, onConsole);
        return this;
    }

    /// <summary>
    ///     Convenience method to add a context event handler
    /// </summary>
    /// <param name="onPage">Action to execute when a new page is created</param>
    /// <param name="onRequest">Action to execute on context request</param>
    /// <param name="onResponse">Action to execute on context response</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder OnContextEvents(
        Action<IBrowserContext, IPage>? onPage = null,
        Action<IBrowserContext, IRequest>? onRequest = null,
        Action<IBrowserContext, IResponse>? onResponse = null)
    {
        _endpoint.EndpointConfiguration.AddContextHandler(onPage, onRequest, onResponse);
        return this;
    }

    /// <summary>
    ///     Convenience method to set mobile device emulation
    /// </summary>
    /// <param name="viewport">Viewport size for mobile device</param>
    /// <param name="userAgent">User agent string for mobile device</param>
    /// <param name="deviceScaleFactor">Device scale factor</param>
    /// <param name="hasTouch">Whether device has touch support</param>
    /// <param name="isMobile">Whether device is mobile</param>
    /// <returns>This builder instance for method chaining</returns>
    public PlaywrightBrowserBuilder MobileDevice(ViewportSize viewport, string userAgent,
        float deviceScaleFactor = 1.0f, bool hasTouch = false, bool isMobile = false)
    {
        _endpoint.EndpointConfiguration.SetMobileDevice(viewport, userAgent, deviceScaleFactor, hasTouch, isMobile);
        return this;
    }
}
