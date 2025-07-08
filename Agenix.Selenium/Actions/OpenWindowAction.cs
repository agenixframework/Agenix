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
using OpenQA.Selenium;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Opens a new browser window and switches to it.
///     Stores window handles for future reference.
/// </summary>
public class OpenWindowAction : AbstractSeleniumAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(OpenWindowAction));

    /// <summary>
    ///     Window name to store the window handle under
    /// </summary>
    private readonly string _windowName;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public OpenWindowAction(Builder builder) : base("open-window", builder)
    {
        _windowName = builder.WindowName;
    }

    /// <summary>
    ///     Gets the window name
    /// </summary>
    public string WindowName => _windowName;

    /// <summary>
    ///     Executes the action of opening a new browser window and switching context to it.
    /// </summary>
    /// <param name="browser">The SeleniumBrowser instance used to interact with the web driver.</param>
    /// <param name="context">The TestContext that provides runtime information and manages test variables.</param>
    /// <exception cref="AgenixSystemException">Thrown when the WebDriver does not support JavaScript execution.</exception>
    /// <exception cref="AgenixSystemException">Thrown when a new window fails to open, and no new window handle is detected.</exception>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        // Get current window handles before opening a new window
        var windowHandles = browser.WebDriver.WindowHandles.ToHashSet();
        var currentWindow = browser.WebDriver.CurrentWindowHandle;

        // Store the last active window
        context.SetVariable(SeleniumHeaders.SeleniumLastWindow, currentWindow);

        // Open a new window using JavaScript
        if (browser.WebDriver is IJavaScriptExecutor jsExecutor)
        {
            jsExecutor.ExecuteScript("window.open();");
        }
        else
        {
            throw new AgenixSystemException("WebDriver does not support JavaScript execution - cannot open new window");
        }

        // Get window handles after opening a new window
        var newWindowHandles = browser.WebDriver.WindowHandles.ToHashSet();

        // Find the newly opened window
        var newWindow = newWindowHandles.FirstOrDefault(window => !windowHandles.Contains(window));

        if (!string.IsNullOrEmpty(newWindow))
        {
            // Switch to the new window
            browser.WebDriver.SwitchTo().Window(newWindow);
            Logger.LogInformation("Opened and switched to new window: {NewWindow}", newWindow);

            // Store window handles in context
            context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, newWindow);
            context.SetVariable(_windowName, newWindow);
        }
        else
        {
            throw new AgenixSystemException("Failed to open new window - no new window handle detected");
        }
    }

    /// <summary>
    ///     Action builder for OpenWindowAction
    /// </summary>
    public class Builder : Builder<OpenWindowAction, Builder>
    {
        public string WindowName { get; private set; } = SeleniumHeaders.SeleniumActiveWindow;

        /// <summary>
        ///     Set the window name for storing the window handle
        /// </summary>
        public Builder SetWindow(string name)
        {
            WindowName = name;
            return self;
        }

        /// <summary>
        ///     Builds an instance of the OpenWindowAction class using the builder pattern.
        /// </summary>
        /// <returns>An instance of the OpenWindowAction class.</returns>
        public override OpenWindowAction Build()
        {
            return new OpenWindowAction(this);
        }
    }
}
