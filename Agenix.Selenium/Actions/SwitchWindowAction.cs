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
///     Selenium action for switching between browser windows.
/// </summary>
public class SwitchWindowAction : AbstractSeleniumAction
{
    /// <summary>
    ///     Logger instance used to log information, warnings, errors, or debug messages for the
    ///     <see cref="SwitchWindowAction" /> class.
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(SwitchWindowAction));

    /// <summary>
    ///     The window name to switch to.
    /// </summary>
    private readonly string _windowName;

    /// <summary>
    ///     Default constructor.
    /// </summary>
    /// <param name="builder">The action builder</param>
    public SwitchWindowAction(Builder builder) : base("switch-window", builder)
    {
        _windowName = builder.WindowName;
    }

    /// <summary>
    ///     Gets the window name.
    /// </summary>
    public string WindowName => _windowName;

    /// <summary>
    ///     Executes the switch window action.
    /// </summary>
    /// <param name="browser">The Selenium browser instance</param>
    /// <param name="context">The test context</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        if (!context.GetVariables().ContainsKey(_windowName))
        {
            throw new AgenixSystemException($"Failed to find window handle for window {_windowName}");
        }

        var targetWindow = context.GetVariable(_windowName);
        var handles = browser.WebDriver.WindowHandles;

        if (!handles.Contains(targetWindow))
        {
            throw new AgenixSystemException($"Failed to find window for handle {targetWindow}");
        }

        var lastWindow = browser.WebDriver.CurrentWindowHandle;

        if (!lastWindow.Equals(targetWindow))
        {
            context.SetVariable(SeleniumHeaders.SeleniumLastWindow, lastWindow);

            browser.WebDriver.SwitchTo().Window(targetWindow);
            Logger.LogInformation("Switch window focus to {WindowName}", _windowName);

            context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, targetWindow);
        }
        else
        {
            Logger.LogInformation("Skip switch window action as window is already focused");
        }
    }

    /// <summary>
    ///     Builder for creating SwitchWindowAction instances.
    /// </summary>
    public class Builder : Builder<SwitchWindowAction, Builder>
    {
        internal string WindowName { get; private set; } = SeleniumHeaders.SeleniumActiveWindow;

        /// <summary>
        ///     Sets the window name to switch to.
        /// </summary>
        /// <param name="name">The window name</param>
        /// <returns>This builder instance</returns>
        public Builder Window(string name)
        {
            WindowName = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        /// <summary>
        ///     Builds the SwitchWindowAction instance.
        /// </summary>
        /// <returns>A configured SwitchWindowAction</returns>
        public override SwitchWindowAction Build()
        {
            return new SwitchWindowAction(this);
        }
    }
}
