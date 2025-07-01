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
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Events;

namespace Agenix.Selenium.Endpoint;

/// <summary>
///     Builder class for creating and configuring SeleniumBrowser instances.
///     Provides a fluent API for setting various browser configuration options.
/// </summary>
public class SeleniumBrowserBuilder : AbstractEndpointBuilder<SeleniumBrowser>
{
    /// <summary>
    ///     The endpoint target being built
    /// </summary>
    private readonly SeleniumBrowser _endpoint = new();

    /// <summary>
    ///     Gets the endpoint being built
    /// </summary>
    /// <returns>The SeleniumBrowser instance</returns>
    protected override SeleniumBrowser GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    ///     Sets the browser type
    /// </summary>
    /// <param name="type">Browser type (e.g., "firefox", "chrome", "htmlunit")</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder Type(string type)
    {
        _endpoint.EndpointConfiguration.BrowserType = type;
        return this;
    }

    /// <summary>
    ///     Sets the Firefox profile
    /// </summary>
    /// <param name="profile">Firefox profile configuration</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder Profile(FirefoxProfile profile)
    {
        _endpoint.EndpointConfiguration.FirefoxProfile = profile;
        return this;
    }

    /// <summary>
    ///     Sets the JavaScript enabled flag
    /// </summary>
    /// <param name="enabled">True to enable JavaScript, false to disable</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder JavaScript(bool enabled)
    {
        _endpoint.EndpointConfiguration.JavaScript = enabled;
        return this;
    }

    /// <summary>
    ///     Sets the remote server URL
    /// </summary>
    /// <param name="url">Remote Selenium server URL</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder RemoteServer(string url)
    {
        _endpoint.EndpointConfiguration.RemoteServerUrl = url;
        return this;
    }

    /// <summary>
    ///     Sets a custom web driver instance
    /// </summary>
    /// <param name="driver">WebDriver instance to use</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder WebDriver(IWebDriver driver)
    {
        _endpoint.EndpointConfiguration.WebDriver = driver;
        return this;
    }

    /// <summary>
    ///     Sets the event handlers for the browser
    /// </summary>
    /// <param name="handlers">List of event handler actions</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder EventHandlers(List<Action<EventFiringWebDriver>> handlers)
    {
        _endpoint.EndpointConfiguration.EventHandlers = handlers;
        return this;
    }

    /// <summary>
    ///     Adds a single event handler
    /// </summary>
    /// <param name="handler">Event handler action to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder AddEventHandler(Action<EventFiringWebDriver> handler)
    {
        _endpoint.EndpointConfiguration.EventHandlers.Add(handler);
        return this;
    }

    /// <summary>
    ///     Sets the browser version
    /// </summary>
    /// <param name="version">Browser version string</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder Version(string version)
    {
        _endpoint.EndpointConfiguration.Version = version;
        return this;
    }

    /// <summary>
    ///     Sets the start page URL
    /// </summary>
    /// <param name="url">Initial page URL to navigate to</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder StartPage(string url)
    {
        _endpoint.EndpointConfiguration.StartPageUrl = url;
        return this;
    }

    /// <summary>
    ///     Sets the default timeout
    /// </summary>
    /// <param name="timeout">Timeout value in milliseconds</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder Timeout(long timeout)
    {
        _endpoint.EndpointConfiguration.Timeout = timeout;
        return this;
    }

    /// <summary>
    ///     Convenience method to add a navigation event handler
    /// </summary>
    /// <param name="beforeNavigate">Action to execute before navigation</param>
    /// <param name="afterNavigate">Action to execute after navigation</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder OnNavigation(
        Action<object, WebDriverNavigationEventArgs>? beforeNavigate = null,
        Action<object, WebDriverNavigationEventArgs>? afterNavigate = null)
    {
        _endpoint.EndpointConfiguration.AddNavigationHandler(beforeNavigate, afterNavigate);
        return this;
    }

    /// <summary>
    ///     Convenience method to add an element interaction event handler
    /// </summary>
    /// <param name="beforeClick">Action to execute before element click</param>
    /// <param name="afterClick">Action to execute after element click</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder OnElementInteraction(
        Action<object, WebElementEventArgs>? beforeClick = null,
        Action<object, WebElementEventArgs>? afterClick = null)
    {
        _endpoint.EndpointConfiguration.AddElementHandler(beforeClick, afterClick);
        return this;
    }

    /// <summary>
    ///     Convenience method to add an exception handler
    /// </summary>
    /// <param name="onException">Action to execute when an exception occurs</param>
    /// <returns>This builder instance for method chaining</returns>
    public SeleniumBrowserBuilder OnException(Action<object, WebDriverExceptionEventArgs> onException)
    {
        _endpoint.EndpointConfiguration.AddExceptionHandler(onException);
        return this;
    }
}
