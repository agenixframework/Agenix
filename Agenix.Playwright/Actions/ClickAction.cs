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
///     Playwright action for clicking on web elements.
///     Inherits from LocatingElementAction to leverage element location capabilities.
///     Supports all Playwright click options including modifiers, position, and timing.
/// </summary>
public class ClickAction : LocatingElementAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(ClickAction));

    private readonly LocatorClickOptions? _clickOptions;


    /// <summary>
    ///     Represents a Playwright action for clicking on web elements.
    ///     Inherits from <see cref="LocatingElementAction" /> to use web element locating capabilities.
    ///     Provides support for all Playwright click options, including modifiers, position, click count, delay, and more.
    /// </summary>
    public ClickAction(Builder builder) : this("click", builder) { }

    /// <summary>
    ///     Playwright action for clicking on web elements.
    ///     Inherits from LocatingElementAction to leverage element location capabilities.
    ///     Supports all Playwright click options including modifiers, position, and timing.
    /// </summary>
    public ClickAction(string name, Builder builder) : base(name, builder)
    {
        _clickOptions = builder.ClickOptions;
    }

    /// <summary>
    ///     Executes the click action on the located element.
    /// </summary>
    /// <param name="locator">The located element to click on</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    protected override async Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogDebug("Clicking on element with options: {Options}", _clickOptions?.ToString() ?? "default");

            await locator.ClickAsync(_clickOptions);

            Logger.LogInformation("Successfully clicked on element");
        }
        catch (Exception ex)
        {
            throw new AgenixSystemException("Failed to click on element", ex);
        }
    }

    /// <summary>
    ///     Builder class for creating ClickAction instances with fluent API.
    ///     Inherits from LocatingElementAction.Builder to support all locator strategies.
    /// </summary>
    public new class Builder : LocatingElementAction.Builder
    {
        internal LocatorClickOptions? ClickOptions { get; private set; }

        /// <summary>
        ///     Sets the click options directly.
        /// </summary>
        /// <param name="clickOptions">The click options to use</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClickOptions(LocatorClickOptions clickOptions)
        {
            ClickOptions = clickOptions;
            return this;
        }

        /// <summary>
        ///     Configures click options using a fluent builder pattern.
        /// </summary>
        /// <param name="configureOptions">Action to configure the click options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClickOptions(Action<ClickOptionsBuilder> configureOptions)
        {
            var optionsBuilder = new ClickOptionsBuilder();
            configureOptions(optionsBuilder);
            ClickOptions = optionsBuilder.Build();
            return this;
        }

        /// <summary>
        ///     Sets the mouse button to use for clicking.
        /// </summary>
        /// <param name="button">The mouse button to use (default is Left)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithButton(MouseButton button = MouseButton.Left)
        {
            ClickOptions ??= new LocatorClickOptions();
            ClickOptions.Button = button;
            return this;
        }

        /// <summary>
        ///     Sets the number of times to click.
        /// </summary>
        /// <param name="count">The number of clicks (default is 1)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClickCount(int count = 1)
        {
            ClickOptions ??= new LocatorClickOptions();
            ClickOptions.ClickCount = count;
            return this;
        }

        /// <summary>
        ///     Sets the delay between mouse down and mouse up in milliseconds.
        /// </summary>
        /// <param name="delay">The delay in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithDelay(float delay)
        {
            ClickOptions ??= new LocatorClickOptions();
            ClickOptions.Delay = delay;
            return this;
        }

        /// <summary>
        ///     Sets whether to force the click even if the element is not visible.
        /// </summary>
        /// <param name="force">Whether to force the click</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithForce(bool force = true)
        {
            ClickOptions ??= new LocatorClickOptions();
            ClickOptions.Force = force;
            return this;
        }

        /// <summary>
        ///     Sets modifier keys to hold during the click.
        /// </summary>
        /// <param name="modifiers">The modifier keys to hold</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithModifiers(IEnumerable<KeyboardModifier> modifiers)
        {
            ClickOptions ??= new LocatorClickOptions();
            ClickOptions.Modifiers = modifiers;
            return this;
        }

        /// <summary>
        ///     Sets the position to click relative to the element.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithPosition(float x, float y)
        {
            ClickOptions ??= new LocatorClickOptions();
            ClickOptions.Position = new Position { X = x, Y = y };
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the click operation.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(float timeout)
        {
            ClickOptions ??= new LocatorClickOptions();
            ClickOptions.Timeout = timeout;
            return this;
        }

        /// <summary>
        ///     Sets whether to wait for the element to be enabled before clicking.
        /// </summary>
        /// <param name="trial">Whether this is a trial run</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTrial(bool trial = true)
        {
            ClickOptions ??= new LocatorClickOptions();
            ClickOptions.Trial = trial;
            return this;
        }

        /// <summary>
        ///     Builds the ClickAction instance.
        /// </summary>
        /// <returns>A new ClickAction instance</returns>
        public override ClickAction Build()
        {
            return new ClickAction(this);
        }
    }

    /// <summary>
    ///     Builder class for configuring click options using a fluent API.
    /// </summary>
    public class ClickOptionsBuilder
    {
        private readonly LocatorClickOptions _options = new();

        /// <summary>
        ///     Sets the mouse button to use for clicking.
        /// </summary>
        /// <param name="button">The mouse button to use</param>
        /// <returns>The builder instance for method chaining</returns>
        public ClickOptionsBuilder WithButton(MouseButton button)
        {
            _options.Button = button;
            return this;
        }

        /// <summary>
        ///     Sets the number of times to click.
        /// </summary>
        /// <param name="count">The number of clicks</param>
        /// <returns>The builder instance for method chaining</returns>
        public ClickOptionsBuilder WithClickCount(int count)
        {
            _options.ClickCount = count;
            return this;
        }

        /// <summary>
        ///     Sets the delay between mouse down and mouse up.
        /// </summary>
        /// <param name="delay">The delay in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public ClickOptionsBuilder WithDelay(float delay)
        {
            _options.Delay = delay;
            return this;
        }

        /// <summary>
        ///     Sets whether to force the click.
        /// </summary>
        /// <param name="force">Whether to force the click</param>
        /// <returns>The builder instance for method chaining</returns>
        public ClickOptionsBuilder WithForce(bool force = true)
        {
            _options.Force = force;
            return this;
        }

        /// <summary>
        ///     Sets modifier keys to hold during the click.
        /// </summary>
        /// <param name="modifiers">The modifier keys to hold</param>
        /// <returns>The builder instance for method chaining</returns>
        public ClickOptionsBuilder WithModifiers(IEnumerable<KeyboardModifier> modifiers)
        {
            _options.Modifiers = modifiers;
            return this;
        }

        /// <summary>
        ///     Sets the position to click relative to the element.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public ClickOptionsBuilder WithPosition(float x, float y)
        {
            _options.Position = new Position { X = x, Y = y };
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the click operation.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public ClickOptionsBuilder WithTimeout(float timeout)
        {
            _options.Timeout = timeout;
            return this;
        }

        /// <summary>
        ///     Sets whether this is a trial run.
        /// </summary>
        /// <param name="trial">Whether this is a trial run</param>
        /// <returns>The builder instance for method chaining</returns>
        public ClickOptionsBuilder WithTrial(bool trial = true)
        {
            _options.Trial = trial;
            return this;
        }

        /// <summary>
        ///     Builds the LocatorClickOptions instance.
        /// </summary>
        /// <returns>A new LocatorClickOptions instance</returns>
        internal LocatorClickOptions Build()
        {
            return _options;
        }
    }
}
