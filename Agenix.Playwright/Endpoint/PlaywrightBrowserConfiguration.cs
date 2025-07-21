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

using Agenix.Api.Log;
using Agenix.Core.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Agenix.Playwright.Endpoint;

/// <summary>
///     Provides configuration settings for a Playwright-based browser instance.
///     This class allows specifying browser type, headless mode, viewport settings,
///     start page URL, timeout settings, and custom event handlers.
///     Inherits from <see cref="AbstractEndpointConfiguration" />.
/// </summary>
public class PlaywrightBrowserConfiguration : AbstractEndpointConfiguration
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(PlaywrightBrowserConfiguration));

    /// <summary>
    ///     Browser type - defaults to Chromium
    /// </summary>
    public virtual string BrowserType { get; set; } = "chromium";

    /// <summary>
    ///     Enable headless mode
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    ///     Start page URL
    /// </summary>
    public virtual string StartPageUrl { get; set; } = "about:blank";

    /// <summary>
    ///     Browser channel (stable, beta, dev, canary)
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    ///     Custom executable path for the browser
    /// </summary>
    public string? ExecutablePath { get; set; }

    /// <summary>
    ///     Viewport settings
    /// </summary>
    public ViewportSize? Viewport { get; set; } = ViewportSize.NoViewport;

    /// <summary>
    ///     Default timeout for page operations in milliseconds
    /// </summary>
    public float DefaultTimeout { get; set; } = 30000;

    /// <summary>
    ///     Default navigation timeout in milliseconds
    /// </summary>
    public float DefaultNavigationTimeout { get; set; } = 30000;

    /// <summary>
    ///     Ignore HTTPS errors
    /// </summary>
    public bool IgnoreHttpsErrors { get; set; }

    /// <summary>
    ///     Enable JavaScript
    /// </summary>
    public bool JavaScript { get; set; } = true;

    /// <summary>
    ///     User agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    ///     Device scale factor
    /// </summary>
    public float DeviceScaleFactor { get; set; } = 1.0f;

    /// <summary>
    ///     Enable touch events
    /// </summary>
    public bool HasTouch { get; set; }

    /// <summary>
    ///     Enable mobile mode
    /// </summary>
    public bool IsMobile { get; set; }

    /// <summary>
    ///     Geolocation settings
    /// </summary>
    public Geolocation? Geolocation { get; set; }

    /// <summary>
    ///     Permissions to grant
    /// </summary>
    public string[] Permissions { get; set; } = [];

    /// <summary>
    ///     Color scheme preference
    /// </summary>
    public ColorScheme ColorScheme { get; set; } = ColorScheme.Light;

    /// <summary>
    ///     Reduced motion preference
    /// </summary>
    public ReducedMotion ReducedMotion { get; set; } = ReducedMotion.NoPreference;

    /// <summary>
    ///     Locale for the browser
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    ///     Time zone for the browser
    /// </summary>
    public string? TimezoneId { get; set; }

    /// <summary>
    ///     HTTP credentials for basic authentication
    /// </summary>
    public HttpCredentials? HttpCredentials { get; set; }

    /// <summary>
    ///     Extra HTTP headers to send with every request
    /// </summary>
    public Dictionary<string, string> ExtraHttpHeaders { get; set; } = [];

    /// <summary>
    ///     Offline mode
    /// </summary>
    public bool Offline { get; set; } = false;

    /// <summary>
    ///     Whether to accept downloads
    /// </summary>
    public bool AcceptDownloads { get; set; }

    /// <summary>
    ///     Video recording settings
    /// </summary>
    public string? VideoDir { get; set; }

    /// <summary>
    ///     Video recording size
    /// </summary>
    public RecordVideoSize? VideoSize { get; set; }

    /// <summary>
    ///     Trace settings
    /// </summary>
    public bool RecordTrace { get; set; }

    /// <summary>
    ///     Trace directory
    /// </summary>
    public string? TraceDir { get; set; }

    /// <summary>
    ///     Custom browser instance
    /// </summary>
    public IBrowser? Browser { get; set; }

    /// <summary>
    ///     Custom page instance
    /// </summary>
    public IPage? Page { get; set; }

    /// <summary>
    ///     Browser context options
    /// </summary>
    public BrowserNewContextOptions? ContextOptions { get; set; }

    /// <summary>
    ///     Browser launch options
    /// </summary>
    public BrowserTypeLaunchOptions? LaunchOptions { get; set; }

    /// <summary>
    ///     Page event handlers
    /// </summary>
    public List<Action<IPage>> PageEventHandlers { get; set; } = [];

    /// <summary>
    ///     Browser context event handlers
    /// </summary>
    public List<Action<IBrowserContext>> ContextEventHandlers { get; set; } = [];

    /// <summary>
    ///     Enables automatic test isolation by creating a new browser context for each test.
    ///     When enabled, each test will run in its own isolated browser context while reusing the same browser instance.
    ///     This provides test isolation without the performance overhead of starting/stopping browsers between tests.
    /// </summary>
    public bool TestIsolation { get; set; } = false;

    /// <summary>
    ///     Configuration for test isolation behavior.
    ///     Specifies how test contexts should be managed and cleaned up.
    /// </summary>
    public TestIsolationMode TestIsolationMode { get; set; } = TestIsolationMode.NONE;

    /// <summary>
    ///     Helper method to add page event handlers
    /// </summary>
    public void AddPageHandler(
        Action<IPage, IRequest>? onRequest = null,
        Action<IPage, IResponse>? onResponse = null,
        Action<IPage, string>? onPageError = null,
        Action<IPage, IConsoleMessage>? onConsole = null)
    {
        PageEventHandlers.Add(page =>
        {
            if (onRequest != null)
            {
                page.Request += (sender, request) => onRequest(page, request);
            }

            if (onResponse != null)
            {
                page.Response += (sender, response) => onResponse(page, response);
            }

            if (onPageError != null)
            {
                page.PageError += (sender, error) => onPageError(page, error);
            }

            if (onConsole != null)
            {
                page.Console += (sender, msg) => onConsole(page, msg);
            }
        });
    }

    /// <summary>
    ///     Helper method to add context event handlers
    /// </summary>
    public void AddContextHandler(
        Action<IBrowserContext, IPage>? onPage = null,
        Action<IBrowserContext, IRequest>? onRequest = null,
        Action<IBrowserContext, IResponse>? onResponse = null)
    {
        ContextEventHandlers.Add(context =>
        {
            if (onPage != null)
            {
                context.Page += (sender, page) => onPage(context, page);
            }

            if (onRequest != null)
            {
                context.Request += (sender, request) => onRequest(context, request);
            }

            if (onResponse != null)
            {
                context.Response += (sender, response) => onResponse(context, response);
            }
        });
    }

    /// <summary>
    ///     Helper method to set mobile device emulation
    /// </summary>
    /// <summary>
    ///     Helper method to set mobile device emulation
    /// </summary>
    public void SetMobileDevice(ViewportSize viewport, string userAgent, float deviceScaleFactor = 1.0f,
        bool hasTouch = false, bool isMobile = false)
    {
        Viewport = viewport;
        UserAgent = userAgent;
        DeviceScaleFactor = deviceScaleFactor;
        HasTouch = hasTouch;
        IsMobile = isMobile;

        Log.LogDebug("Set mobile device emulation: {Width}x{Height}, UserAgent: {UserAgent}", viewport.Width,
            viewport.Height, userAgent);
    }

    /// <summary>
    ///     Helper method to set geolocation
    /// </summary>
    public void SetGeolocation(float latitude, float longitude, float? accuracy = null)
    {
        Geolocation = new Geolocation { Latitude = latitude, Longitude = longitude, Accuracy = accuracy };
    }

    /// <summary>
    ///     Helper method to add permissions
    /// </summary>
    public void AddPermissions(params string[] permissions)
    {
        Permissions = permissions?.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray() ?? [];
    }

    /// <summary>
    ///     Helper method to add extra HTTP headers
    /// </summary>
    public void AddExtraHttpHeaders(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            ExtraHttpHeaders[header.Key] = header.Value;
        }
    }
}
