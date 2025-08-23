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
using Agenix.Api.Endpoint;
using Agenix.Core.Endpoint;
using Microsoft.Playwright;

namespace Agenix.Playwright.Endpoint;

/// <summary>
///     Playwright endpoint component that provides integration with the Agenix testing framework.
///     This component extends AbstractEndpointComponent to provide Playwright-specific functionality
///     for managing browser instances and their lifecycle within the test execution context.
/// </summary>
public class PlaywrightEndpointComponent : AbstractEndpointComponent
{
    /// <summary>
    ///     Default constructor using the name for this component
    /// </summary>
    public PlaywrightEndpointComponent() : base("playwright")
    {
    }

    /// <summary>
    ///     Creates a new Playwright browser endpoint based on the provided resource path and parameters
    /// </summary>
    /// <param name="resourcePath">The resource path that may specify a browser type</param>
    /// <param name="parameters">Configuration parameters for the endpoint</param>
    /// <param name="context">The test context</param>
    /// <returns>A configured PlaywrightBrowser endpoint</returns>
    protected override IEndpoint CreateEndpoint(string resourcePath, IDictionary<string, string> parameters,
        TestContext context)
    {
        var browser = new PlaywrightBrowser();
        var config = browser.EndpointConfiguration;

        ConfigureBrowserType(config, resourcePath);
        ConfigureBasicParameters(config, parameters);
        ConfigureViewportParameters(config, parameters);
        ConfigureTimeoutParameters(config, parameters);
        ConfigureBooleanParameters(config, parameters);
        ConfigureStringParameters(config, parameters);
        ConfigureDirectoryParameters(config, parameters);

        EnrichEndpointConfiguration(config,
            GetEndpointConfigurationParameters(parameters, typeof(PlaywrightBrowserConfiguration)), context);

        return browser;
    }

    private static void ConfigureBrowserType(PlaywrightBrowserConfiguration config, string resourcePath)
    {
        if (!string.IsNullOrWhiteSpace(resourcePath) &&
            !resourcePath.Equals("browser", StringComparison.OrdinalIgnoreCase))
        {
            config.BrowserType = resourcePath;
        }
    }

    private static void ConfigureBasicParameters(PlaywrightBrowserConfiguration config,
        IDictionary<string, string> parameters)
    {
        SetStringParameter(parameters, "start-page", value => config.StartPageUrl = value);
        SetStringParameter(parameters, "channel", value => config.Channel = value);
        SetStringParameter(parameters, "executable-path", value => config.ExecutablePath = value);
        SetStringParameter(parameters, "user-agent", value => config.UserAgent = value);
    }

    private static void ConfigureViewportParameters(PlaywrightBrowserConfiguration config,
        IDictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("viewport-width", out var widthStr) &&
            parameters.TryGetValue("viewport-height", out var heightStr))
        {
            if (int.TryParse(widthStr, out var width) && int.TryParse(heightStr, out var height))
            {
                config.Viewport = new ViewportSize { Width = width, Height = height };
            }

            parameters.Remove("viewport-width");
            parameters.Remove("viewport-height");
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="config"></param>
    /// <param name="parameters"></param>
    private static void ConfigureTimeoutParameters(PlaywrightBrowserConfiguration config,
        IDictionary<string, string> parameters)
    {
        SetFloatParameter(parameters, "timeout", value => config.DefaultTimeout = value);
        SetFloatParameter(parameters, "navigation-timeout", value => config.DefaultNavigationTimeout = value);
    }

    private static void ConfigureBooleanParameters(PlaywrightBrowserConfiguration config,
        IDictionary<string, string> parameters)
    {
        SetBooleanParameter(parameters, "headless", value => config.Headless = value);
        SetBooleanParameter(parameters, "ignore-https-errors", value => config.IgnoreHttpsErrors = value);
        SetBooleanParameter(parameters, "javascript", value => config.JavaScript = value);
        SetBooleanParameter(parameters, "offline", value => config.Offline = value);
        SetBooleanParameter(parameters, "accept-downloads", value => config.AcceptDownloads = value);
        SetBooleanParameter(parameters, "record-trace", value => config.RecordTrace = value);
    }

    private static void ConfigureStringParameters(PlaywrightBrowserConfiguration config,
        IDictionary<string, string> parameters)
    {
        SetStringParameter(parameters, "locale", value => config.Locale = value);
        SetStringParameter(parameters, "timezone", value => config.TimezoneId = value);
    }

    private static void ConfigureDirectoryParameters(PlaywrightBrowserConfiguration config,
        IDictionary<string, string> parameters)
    {
        SetStringParameter(parameters, "video-dir", value => config.VideoDir = value);
        SetStringParameter(parameters, "trace-dir", value => config.TraceDir = value);
    }

    private static void SetStringParameter(IDictionary<string, string> parameters,
        string key, Action<string> setter)
    {
        if (parameters.TryGetValue(key, out var value))
        {
            setter(value);
            parameters.Remove(key);
        }
    }

    private static void SetBooleanParameter(IDictionary<string, string> parameters,
        string key, Action<bool> setter)
    {
        if (parameters.TryGetValue(key, out var value) && bool.TryParse(value, out var parsedValue))
        {
            setter(parsedValue);
            parameters.Remove(key);
        }
    }

    private static void SetFloatParameter(IDictionary<string, string> parameters,
        string key, Action<float> setter)
    {
        if (parameters.TryGetValue(key, out var value) && float.TryParse(value, out var parsedValue))
        {
            setter(parsedValue);
            parameters.Remove(key);
        }
    }
}
