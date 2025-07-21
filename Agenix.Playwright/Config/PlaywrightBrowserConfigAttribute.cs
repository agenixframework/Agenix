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

using System.ComponentModel;
using Agenix.Api.Annotations;
using Agenix.Playwright.Endpoint;

namespace Agenix.Playwright.Config;

/// <summary>
///     Configuration attribute for Playwright browser endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
[AgenixEndpointConfig("playwright.browser")]
public class PlaywrightBrowserConfigAttribute : Attribute
{
    /// <summary>
    ///     Browser start page
    /// </summary>
    public string StartPage { get; set; } = "";

    /// <summary>
    ///     Browser type (chromium, firefox, webkit)
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    ///     Enable headless mode
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    ///     Browser channel (stable, beta, dev, canary)
    /// </summary>
    public string Channel { get; set; } = "";

    /// <summary>
    ///     Custom executable path for the browser
    /// </summary>
    public string ExecutablePath { get; set; } = "";

    /// <summary>
    ///     Viewport width
    /// </summary>
    public int ViewportWidth { get; set; } = 1280;

    /// <summary>
    ///     Viewport height
    /// </summary>
    public int ViewportHeight { get; set; } = 720;

    /// <summary>
    ///     Default timeout for page operations in milliseconds
    /// </summary>
    public float DefaultTimeout { get; set; } = 30000f;

    /// <summary>
    ///     Default navigation timeout in milliseconds
    /// </summary>
    public float DefaultNavigationTimeout { get; set; } = 30000f;

    /// <summary>
    ///     Ignore HTTPS errors
    /// </summary>
    public bool IgnoreHttpsErrors { get; set; } = false;

    /// <summary>
    ///     Enable JavaScript
    /// </summary>
    public bool JavaScript { get; set; } = true;

    /// <summary>
    ///     User agent string
    /// </summary>
    public string UserAgent { get; set; } = "";

    /// <summary>
    ///     Device scale factor
    /// </summary>
    public float DeviceScaleFactor { get; set; } = 1.0f;

    /// <summary>
    ///     Enable touch events
    /// </summary>
    public bool HasTouch { get; set; } = false;

    /// <summary>
    ///     Enable mobile mode
    /// </summary>
    public bool IsMobile { get; set; } = false;

    /// <summary>
    ///     Geolocation latitude
    /// </summary>
    public float? GeolocationLatitude { get; set; }

    /// <summary>
    ///     Geolocation longitude
    /// </summary>
    public float? GeolocationLongitude { get; set; }

    /// <summary>
    ///     Geolocation accuracy in meters
    /// </summary>
    public float? GeolocationAccuracy { get; set; }

    /// <summary>
    ///     Permissions to grant (comma-separated)
    /// </summary>
    public string Permissions { get; set; } = "";

    /// <summary>
    ///     Color scheme preference (Light, Dark, NoPreference)
    /// </summary>
    public string ColorScheme { get; set; } = "Light";

    /// <summary>
    ///     Reduced motion preference (Reduce, NoPreference)
    /// </summary>
    public string ReducedMotion { get; set; } = "NoPreference";

    /// <summary>
    ///     Locale for the browser
    /// </summary>
    public string Locale { get; set; } = "";

    /// <summary>
    ///     Time zone for the browser
    /// </summary>
    public string TimezoneId { get; set; } = "";

    /// <summary>
    ///     HTTP credentials username for basic authentication
    /// </summary>
    public string HttpCredentialsUsername { get; set; } = "";

    /// <summary>
    ///     HTTP credentials password for basic authentication
    /// </summary>
    public string HttpCredentialsPassword { get; set; } = "";

    /// <summary>
    ///     Extra HTTP headers (comma-separated key:value pairs)
    /// </summary>
    public string ExtraHttpHeaders { get; set; } = "";

    /// <summary>
    ///     Offline mode
    /// </summary>
    public bool Offline { get; set; } = false;

    /// <summary>
    ///     Whether to accept downloads
    /// </summary>
    public bool AcceptDownloads { get; set; } = false;

    /// <summary>
    ///     Video recording directory
    /// </summary>
    public string VideoDir { get; set; } = "";

    /// <summary>
    ///     Video recording width
    /// </summary>
    public int VideoWidth { get; set; } = 0;

    /// <summary>
    ///     Video recording height
    /// </summary>
    public int VideoHeight { get; set; } = 0;

    /// <summary>
    ///     Record trace
    /// </summary>
    public bool RecordTrace { get; set; } = false;

    /// <summary>
    ///     Trace directory
    /// </summary>
    public string TraceDir { get; set; } = "";

    /// <summary>
    ///     Page event listeners (comma-separated)
    /// </summary>
    public string[] PageEventListeners { get; set; } = [];

    /// <summary>
    ///     Context event listeners (comma-separated)
    /// </summary>
    public string[] ContextEventListeners { get; set; } = [];

    /// <summary>
    ///     Enables automatic test isolation by creating a new browser context for each test.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Description("EXPERIMENTAL: Test isolation features - subject to breaking changes")]
    public bool TestIsolation { get; set; } = false;

    /// <summary>
    ///     Test isolation mode: None, NewContextPerTest, or NewPagePerTest.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Description("EXPERIMENTAL: Test isolation features - subject to breaking changes")]
    public TestIsolationMode TestIsolationMode { get; set; } = TestIsolationMode.NONE;
}
