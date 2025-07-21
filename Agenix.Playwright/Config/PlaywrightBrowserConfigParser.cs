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

using System.Linq.Expressions;
using System.Reflection;
using Agenix.Api.Config.Annotation;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Agenix.Playwright.Config;

/// <summary>
///     Parser for PlaywrightBrowserConfigAttribute that creates and configures PlaywrightBrowser instances
/// </summary>
public class
    PlaywrightBrowserConfigParser : IAnnotationConfigParser<PlaywrightBrowserConfigAttribute, PlaywrightBrowser>
{
    /// <summary>
    ///     Logger instance for this parser
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(PlaywrightBrowserConfigParser));

    /// <summary>
    ///     Parses the PlaywrightBrowserConfigAttribute to create a configured PlaywrightBrowser instance
    /// </summary>
    /// <param name="attribute">The configuration attribute to be parsed</param>
    /// <param name="referenceResolver">The resolver used to handle references within the configuration</param>
    /// <returns>A configured PlaywrightBrowser instance</returns>
    public PlaywrightBrowser Parse(PlaywrightBrowserConfigAttribute attribute, IReferenceResolver referenceResolver)
    {
        Logger.LogDebug("Parsing PlaywrightBrowserConfigAttribute");

        var browser = new PlaywrightBrowser();
        var config = browser.EndpointConfiguration;

        ConfigureBasicProperties(config, attribute);
        ConfigureViewportAndTimeouts(config, attribute);
        ConfigureDeviceSettings(config, attribute);
        ConfigureLocationAndPermissions(config, attribute);
        ConfigurePreferences(config, attribute);
        ConfigureCredentialsAndHeaders(config, attribute);
        ConfigureRecording(config, attribute);
        ConfigureEventListeners(config, attribute, referenceResolver);

        Logger.LogInformation("PlaywrightBrowser configuration parsed successfully");
        return browser;
    }

    /// <summary>
    ///     Parses the given attribute and reference resolver to create a configured PlaywrightBrowser instance.
    /// </summary>
    /// <param name="annotation">The annotation to parse, expected to be of type PlaywrightBrowserConfigAttribute.</param>
    /// <param name="referenceResolver">The reference resolver to resolve dependencies for the PlaywrightBrowser configuration.</param>
    /// <returns>A configured PlaywrightBrowser instance.</returns>
    public object Parse(Attribute annotation, IReferenceResolver referenceResolver)
    {
        if (annotation is PlaywrightBrowserConfigAttribute seleniumBrowserConfig)
        {
            return Parse(seleniumBrowserConfig, referenceResolver);
        }

        throw new ArgumentException(
            $"Unsupported attribute type: {annotation.GetType().Name}. Expected {nameof(PlaywrightBrowserConfigAttribute)}.");
    }

    private static void ConfigureBasicProperties(PlaywrightBrowserConfiguration config,
        PlaywrightBrowserConfigAttribute attribute)
    {
        SetStringProperty(config, c => c.StartPageUrl, attribute.StartPage, "start page");
        SetStringProperty(config, c => c.BrowserType, attribute.Type, "browser type");
        SetStringProperty(config, c => c.Channel, attribute.Channel, "browser channel");
        SetStringProperty(config, c => c.ExecutablePath, attribute.ExecutablePath, "executable path");
        SetStringProperty(config, c => c.UserAgent, attribute.UserAgent, "user agent");
        SetStringProperty(config, c => c.Locale, attribute.Locale, "locale");
        SetStringProperty(config, c => c.TimezoneId, attribute.TimezoneId, "timezone");
        SetStringProperty(config, c => c.TraceDir, attribute.TraceDir, "trace directory");

        config.Headless = attribute.Headless;
        config.IgnoreHttpsErrors = attribute.IgnoreHttpsErrors;
        config.JavaScript = attribute.JavaScript;
        config.Offline = attribute.Offline;
        config.AcceptDownloads = attribute.AcceptDownloads;
        config.RecordTrace = attribute.RecordTrace;
        config.TestIsolation = attribute.TestIsolation;
        config.TestIsolationMode = attribute.TestIsolationMode;

        Logger.LogDebug("Basic properties configured");
    }

    private void ConfigureViewportAndTimeouts(PlaywrightBrowserConfiguration config,
        PlaywrightBrowserConfigAttribute attribute)
    {
        if (attribute is { ViewportWidth: > 0, ViewportHeight: > 0 })
        {
            config.Viewport = new ViewportSize { Width = attribute.ViewportWidth, Height = attribute.ViewportHeight };
            Logger.LogDebug("Set viewport: {Width}x{Height}", attribute.ViewportWidth, attribute.ViewportHeight);
        }

        if (attribute.DefaultTimeout > 0)
        {
            config.DefaultTimeout = attribute.DefaultTimeout;
            Logger.LogDebug("Set default timeout: {Timeout}ms", attribute.DefaultTimeout);
        }

        if (attribute.DefaultNavigationTimeout > 0)
        {
            config.DefaultNavigationTimeout = attribute.DefaultNavigationTimeout;
            Logger.LogDebug("Set navigation timeout: {Timeout}ms", attribute.DefaultNavigationTimeout);
        }
    }

    private static void ConfigureDeviceSettings(PlaywrightBrowserConfiguration config,
        PlaywrightBrowserConfigAttribute attribute)
    {
        if (attribute.DeviceScaleFactor > 0)
        {
            config.DeviceScaleFactor = attribute.DeviceScaleFactor;
            Logger.LogDebug("Set device scale factor: {DeviceScaleFactor}", attribute.DeviceScaleFactor);
        }

        config.HasTouch = attribute.HasTouch;
        config.IsMobile = attribute.IsMobile;
        Logger.LogDebug("Device settings configured - HasTouch: {HasTouch}, IsMobile: {IsMobile}",
            attribute.HasTouch, attribute.IsMobile);
    }

    private static void ConfigureLocationAndPermissions(PlaywrightBrowserConfiguration config,
        PlaywrightBrowserConfigAttribute attribute)
    {
        if (attribute is { GeolocationLatitude: not null, GeolocationLongitude: not null })
        {
            config.Geolocation = new Geolocation
            {
                Latitude = attribute.GeolocationLatitude.Value,
                Longitude = attribute.GeolocationLongitude.Value,
                Accuracy = attribute.GeolocationAccuracy
            };
            Logger.LogDebug("Set geolocation: {Latitude}, {Longitude}",
                attribute.GeolocationLatitude.Value, attribute.GeolocationLongitude.Value);
        }

        if (!string.IsNullOrWhiteSpace(attribute.Permissions))
        {
            config.Permissions = attribute.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();
            Logger.LogDebug("Set permissions: {Permissions}", string.Join(", ", config.Permissions));
        }
    }

    private static void ConfigurePreferences(PlaywrightBrowserConfiguration config,
        PlaywrightBrowserConfigAttribute attribute)
    {
        SetEnumProperty(config, c => c.ColorScheme, attribute.ColorScheme, "color scheme");
        SetEnumProperty(config, c => c.ReducedMotion, attribute.ReducedMotion, "reduced motion");
    }

    private void ConfigureCredentialsAndHeaders(PlaywrightBrowserConfiguration config,
        PlaywrightBrowserConfigAttribute attribute)
    {
        if (!string.IsNullOrWhiteSpace(attribute.HttpCredentialsUsername))
        {
            config.HttpCredentials = new HttpCredentials
            {
                Username = attribute.HttpCredentialsUsername,
                Password = attribute.HttpCredentialsPassword
            };
            Logger.LogDebug("Set HTTP credentials for user: {Username}", attribute.HttpCredentialsUsername);
        }

        if (!string.IsNullOrWhiteSpace(attribute.ExtraHttpHeaders))
        {
            config.ExtraHttpHeaders = ParseHttpHeaders(attribute.ExtraHttpHeaders);
            Logger.LogDebug("Set extra HTTP headers: {Headers}", string.Join(", ", config.ExtraHttpHeaders.Keys));
        }
    }

    private static void ConfigureRecording(PlaywrightBrowserConfiguration config,
        PlaywrightBrowserConfigAttribute attribute)
    {
        if (!string.IsNullOrWhiteSpace(attribute.VideoDir))
        {
            config.VideoDir = attribute.VideoDir;
            Logger.LogDebug("Set video directory: {VideoDir}", attribute.VideoDir);

            if (attribute is { VideoWidth: > 0, VideoHeight: > 0 })
            {
                config.VideoSize = new RecordVideoSize { Width = attribute.VideoWidth, Height = attribute.VideoHeight };
                Logger.LogDebug("Set video size: {Width}x{Height}", attribute.VideoWidth, attribute.VideoHeight);
            }
        }
    }

    private static void ConfigureEventListeners(PlaywrightBrowserConfiguration config,
        PlaywrightBrowserConfigAttribute attribute, IReferenceResolver referenceResolver)
    {
        if (attribute.PageEventListeners.Length > 0)
        {
            var pageHandlers = attribute.PageEventListeners
                .Select(referenceResolver.Resolve<Action<IPage>>)
                .ToList();
            config.PageEventHandlers = pageHandlers;
            Logger.LogDebug("Set page event listeners: {Listeners}", string.Join(", ", attribute.PageEventListeners));
        }

        if (attribute.ContextEventListeners.Length > 0)
        {
            var contextHandlers = attribute.ContextEventListeners
                .Select(referenceResolver.Resolve<Action<IBrowserContext>>)
                .ToList();
            config.ContextEventHandlers = contextHandlers;
            Logger.LogDebug("Set context event listeners: {Listeners}",
                string.Join(", ", attribute.ContextEventListeners));
        }
    }

    private static void SetStringProperty<T>(T config, Expression<Func<T, string>> propertyExpression, string value,
        string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var property = (PropertyInfo)((MemberExpression)propertyExpression.Body).Member;
        property.SetValue(config, value);
        Logger.LogDebug("Set {PropertyName}: {Value}", propertyName, value);
    }

    private static void SetEnumProperty<TEnum>(PlaywrightBrowserConfiguration config,
        Expression<Func<PlaywrightBrowserConfiguration, TEnum>> propertyExpression, string value, string propertyName)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (Enum.TryParse<TEnum>(value, true, out var enumValue))
        {
            var property = (PropertyInfo)((MemberExpression)propertyExpression.Body).Member;
            property.SetValue(config, enumValue);
            Logger.LogDebug("Set {PropertyName}: {Value}", propertyName, enumValue);
        }
        else
        {
            Logger.LogWarning("Invalid {PropertyName}: {Value}", propertyName, value);
        }
    }

    private static Dictionary<string, string> ParseHttpHeaders(string headerString)
    {
        var headers = new Dictionary<string, string>();
        var headerPairs = headerString.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var headerPair in headerPairs)
        {
            var parts = headerPair.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                headers[parts[0].Trim()] = parts[1].Trim();
            }
        }

        return headers;
    }
}
