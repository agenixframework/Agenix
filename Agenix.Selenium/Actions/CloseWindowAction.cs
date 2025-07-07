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
using Agenix.Selenium.Endpoint;
using Microsoft.Extensions.Logging;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Close opened window by name.
/// </summary>
public class CloseWindowAction : AbstractSeleniumAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(CloseWindowAction));

    /// <summary>
    ///     Window name variable in context
    /// </summary>
    private readonly string _windowName;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public CloseWindowAction(Builder builder) : base("close-window", builder)
    {
        _windowName = builder.WindowName;
    }

    /// <summary>
    ///     Gets the window name
    /// </summary>
    public string WindowName => _windowName;

    /// <summary>
    ///     Executes the action of closing a specified browser window within the current Selenium session.
    /// </summary>
    /// <param name="browser">The Selenium browser instance that provides access to the WebDriver.</param>
    /// <param name="context">The test context containing variables and execution details.</param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown if the window handle associated with the specified window is not found in the context,
    ///     or if the handle is not found in the current Selenium browser session.
    /// </exception>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        if (!context.GetVariables().ContainsKey(_windowName))
        {
            throw new AgenixSystemException($"Failed to find window handle for window {_windowName}");
        }

        var handles = browser.WebDriver.WindowHandles;
        var windowHandle = context.GetVariable(_windowName);

        if (!handles.Contains(windowHandle))
        {
            throw new AgenixSystemException($"Failed to find window for handle {windowHandle}");
        }

        Logger.LogInformation("Current window: {WebDriverCurrentWindowHandle}", browser.WebDriver.CurrentWindowHandle);
        Logger.LogInformation("Window to close: {WindowHandle}", windowHandle);

        if (browser.WebDriver.CurrentWindowHandle.Equals(windowHandle))
        {
            // Closing the active window
            browser.WebDriver.Close();

            Logger.LogInformation("Switch back to main window!");

            if (context.GetVariables().ContainsKey(SeleniumHeaders.SeleniumLastWindow))
            {
                var lastWindow = context.GetVariable(SeleniumHeaders.SeleniumLastWindow);
                browser.WebDriver.SwitchTo().Window(lastWindow);
                context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, lastWindow);
            }
            else
            {
                browser.WebDriver.SwitchTo().DefaultContent();
                context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, browser.WebDriver.CurrentWindowHandle);
            }
        }
        else
        {
            // Closing a different window
            var activeWindow = browser.WebDriver.CurrentWindowHandle;

            browser.WebDriver.SwitchTo().Window(windowHandle);
            browser.WebDriver.Close();

            if (context.GetVariables().ContainsKey(SeleniumHeaders.SeleniumActiveWindow))
            {
                var targetWindow = context.GetVariable(SeleniumHeaders.SeleniumActiveWindow);
                browser.WebDriver.SwitchTo().Window(targetWindow);
            }
            else
            {
                browser.WebDriver.SwitchTo().Window(activeWindow);
                context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, activeWindow);
            }
        }
    }

    /// <summary>
    ///     Action builder for CloseWindowAction
    /// </summary>
    public class Builder : Builder<CloseWindowAction, Builder>
    {
        /// <summary>
        ///     Represents the name of the window that is targeted for specific actions.
        /// </summary>
        public string WindowName { get; private set; } = SeleniumHeaders.SeleniumActiveWindow;

        /// <summary>
        ///     Set window name variable to close
        /// </summary>
        public Builder SetWindow(string name)
        {
            WindowName = name;
            return this;
        }

        /// <summary>
        ///     Builds and returns an instance of the CloseWindowAction.
        /// </summary>
        /// <returns>An instance of CloseWindowAction.</returns>
        public override CloseWindowAction Build()
        {
            return new CloseWindowAction(this);
        }
    }
}
