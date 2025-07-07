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
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Events;

namespace Agenix.Selenium.Endpoint;

/// <summary>
///     Provides configuration settings for a Selenium-based browser instance.
///     This class allows specifying browser type, JavaScript support, start page URL,
///     remote server URL, browser version, custom event handlers, and WebDriver properties.
///     Inherits from <see cref="AbstractEndpointConfiguration" />.
/// </summary>
public class SeleniumBrowserConfiguration : AbstractEndpointConfiguration
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SeleniumBrowserConfiguration));

    /// <summary>
    ///     Optional Firefox profile
    /// </summary>
    private FirefoxProfile? _firefoxProfile;

    /// <summary>
    ///     Browser type - defaults to HtmlUnit equivalent
    /// </summary>
    public virtual string BrowserType { get; set; } = "chrome";

    /// <summary>
    ///     Enable JavaScript
    /// </summary>
    public bool JavaScript { get; set; } = true;

    /// <summary>
    ///     Start page URL
    /// </summary>
    public virtual string StartPageUrl { get; set; }

    /// <summary>
    ///     Selenium remote server URL
    /// </summary>
    public string RemoteServerUrl { get; set; }

    /// <summary>
    ///     Browser version
    /// </summary>
    public string Version { get; set; } = "FIREFOX";

    /// <summary>
    ///     Web driver event handlers - modern approach using EventFiringWebDriver
    /// </summary>
    public List<Action<EventFiringWebDriver>> EventHandlers { get; set; } = [];

    /// <summary>
    ///     Custom web driver instance
    /// </summary>
    public IWebDriver? WebDriver { get; set; }

    /// <summary>
    ///     Gets or sets the Firefox profile with default settings
    /// </summary>
    public FirefoxProfile FirefoxProfile
    {
        get
        {
            if (_firefoxProfile == null)
            {
                _firefoxProfile = new FirefoxProfile();

                // Set preferences that are not frozen/immutable
                TrySetPreference(_firefoxProfile, "security.tls.insecure_fallback_hosts", "");
                TrySetPreference(_firefoxProfile, "security.tls.unrestricted_rc4_fallback", false);

                // Default download folder, set to 2 to use a custom download folder
                TrySetPreference(_firefoxProfile, "browser.download.folderList", 2);

                // Comma separated a list of MIME types to save without asking
                TrySetPreference(_firefoxProfile, "browser.helperApps.neverAsk.saveToDisk", "text/plain");

                // Try to set download manager preference - this may be frozen in newer Firefox versions
                TrySetPreference(_firefoxProfile, "browser.download.manager.showWhenStarting", false);
            }

            return _firefoxProfile;
        }
        set => _firefoxProfile = value;
    }

    /// <summary>
    ///     Helper method to safely set Firefox preferences, handling frozen/immutable preferences
    /// </summary>
    private static void TrySetPreference(FirefoxProfile profile, string preferenceName, object value)
    {
        try
        {
            switch (value)
            {
                case bool boolValue:
                    profile.SetPreference(preferenceName, boolValue);
                    break;
                case int intValue:
                    profile.SetPreference(preferenceName, intValue);
                    break;
                case string stringValue:
                    profile.SetPreference(preferenceName, stringValue);
                    break;
                default:
                    // For other types, try to set as string
                    profile.SetPreference(preferenceName, value.ToString());
                    break;
            }
        }
        catch (ArgumentException ex) when (ex.Message.Contains("may not be overridden"))
        {
            // Silently ignore frozen preferences
            Log.LogWarning("Warning: Cannot set frozen preference '{PreferenceName}': {ExMessage}", preferenceName, ex.Message);
        }
    }

    /// <summary>
    ///     Helper method to add navigation event handlers
    /// </summary>
    public void AddNavigationHandler(Action<object, WebDriverNavigationEventArgs> beforeNavigate = null,
        Action<object, WebDriverNavigationEventArgs> afterNavigate = null)
    {
        EventHandlers.Add(driver =>
        {
            if (beforeNavigate != null)
            {
                driver.Navigating += (sender, e) => beforeNavigate(sender, e);
            }

            if (afterNavigate != null)
            {
                driver.Navigated += (sender, e) => afterNavigate(sender, e);
            }
        });
    }

    /// <summary>
    ///     Helper method to add element interaction event handlers
    /// </summary>
    public void AddElementHandler(Action<object, WebElementEventArgs> beforeClick = null,
        Action<object, WebElementEventArgs> afterClick = null)
    {
        EventHandlers.Add(driver =>
        {
            if (beforeClick != null)
            {
                driver.ElementClicking += (sender, e) => beforeClick(sender, e);
            }

            if (afterClick != null)
            {
                driver.ElementClicked += (sender, e) => afterClick(sender, e);
            }
        });
    }

    /// <summary>
    ///     Helper method to add exception handler
    /// </summary>
    public void AddExceptionHandler(Action<object, WebDriverExceptionEventArgs> onException)
    {
        EventHandlers.Add(driver =>
        {
            if (onException != null)
            {
                driver.ExceptionThrown += (sender, e) => onException(sender, e);
            }
        });
    }
}
