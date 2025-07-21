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
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for checking/unchecking input elements (checkboxes and radio buttons).
///     This action uses Playwright's built-in CheckAsync() and UncheckAsync() methods which
///     automatically handle the element state and provide better reliability.
/// </summary>
public class CheckInputAction : LocatingElementAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(CheckInputAction));

    private readonly bool _checked;
    private readonly bool _force;
    private readonly Position? _position;
    private readonly int? _timeout;
    private readonly bool _trial;


    /// <summary>
    ///     Represents an action to check or uncheck input elements, such as checkboxes and radio buttons,
    ///     leveraging Playwright's CheckAsync and UncheckAsync methods for consistency and reliability.
    /// </summary>
    public CheckInputAction(Builder builder) : this("(un)check-input", builder) { }

    /// <summary>
    ///     Represents an action to check or uncheck input elements, such as checkboxes and radio buttons,
    ///     leveraging Playwright's CheckAsync and UncheckAsync methods for reliability.
    /// </summary>
    public CheckInputAction(string name, Builder builder) : base(name, builder)
    {
        _checked = builder.Checked;
        _force = builder.Force;
        _timeout = builder.Timeout;
        _position = builder.Position;
        _trial = builder.Trial;
    }

    /// <summary>
    ///     Executes the check input action after locating the element.
    /// </summary>
    /// <param name="locator">The located element</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing check input action, checked: {Checked}", _checked);

            // Perform the check or uncheck operation
            if (_checked)
            {
                Logger.LogDebug("Checking element");

                var checkOptions = new LocatorCheckOptions { Force = _force, Trial = _trial };

                if (_timeout.HasValue)
                {
                    checkOptions.Timeout = _timeout.Value;
                }

                if (_position != null)
                {
                    checkOptions.Position = _position;
                }

                await locator.CheckAsync(checkOptions);
                Logger.LogDebug("Element checked successfully");
            }
            else
            {
                Logger.LogDebug("Unchecking element");

                var uncheckOptions = new LocatorUncheckOptions { Force = _force, Trial = _trial };

                if (_timeout.HasValue)
                {
                    uncheckOptions.Timeout = _timeout.Value;
                }

                if (_position != null)
                {
                    uncheckOptions.Position = _position;
                }

                await locator.UncheckAsync(uncheckOptions);
                Logger.LogDebug("Element unchecked successfully");
            }

            Logger.LogInformation("Check input action completed successfully. Element is now {State}",
                _checked ? "checked" : "unchecked");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute check input action");
            throw new AgenixSystemException("Failed to execute check input action", ex);
        }
    }

    /// <summary>
    ///     Builder class for creating CheckInputAction instances with fluent API.
    /// </summary>
    public new class Builder : LocatingElementAction.Builder
    {
        internal bool Checked { get; private set; } = true;
        internal bool Force { get; private set; }
        internal int? Timeout { get; private set; }
        internal Position? Position { get; private set; }
        internal bool Trial { get; private set; }

        /// <summary>
        ///     Sets whether to check or uncheck the element.
        /// </summary>
        /// <param name="check">True to check the element, false to uncheck it</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SetChecked(bool check = true)
        {
            Checked = check;
            return this;
        }

        /// <summary>
        ///     Sets the action to check the element (default behavior).
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Check()
        {
            Checked = true;
            return this;
        }

        /// <summary>
        ///     Sets the action to uncheck the element.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Uncheck()
        {
            Checked = false;
            return this;
        }

        /// <summary>
        ///     Sets whether to force the action even if the element is not actionable.
        /// </summary>
        /// <param name="force">True to force the action</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithForce(bool force = true)
        {
            Force = force;
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the action.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(int timeoutMs)
        {
            Timeout = timeoutMs;
            return this;
        }

        /// <summary>
        ///     Sets the position where to click relative to the element.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithPosition(float x, float y)
        {
            Position = new Position { X = x, Y = y };
            return this;
        }

        /// <summary>
        ///     Sets whether to perform a trial run (validate action without executing).
        /// </summary>
        /// <param name="trial">True for trial mode</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTrial(bool trial = true)
        {
            Trial = trial;
            return this;
        }

        /// <summary>
        ///     Builds the CheckInputAction instance.
        /// </summary>
        /// <returns>A new CheckInputAction instance</returns>
        public override CheckInputAction Build()
        {
            return new CheckInputAction(this);
        }
    }
}
