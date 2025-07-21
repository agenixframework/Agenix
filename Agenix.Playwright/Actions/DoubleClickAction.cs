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
///     Playwright action for double-clicking web elements.
///     Inherits from LocatingElementAction to leverage element location capabilities.
///     Supports all Playwright double-click options, including position, button, modifiers, and timing.
/// </summary>
public class DoubleClickAction : LocatingElementAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(DoubleClickAction));

    private readonly LocatorDblClickOptions? _doubleClickOptions;

    /// <summary>
    ///     Represents a Playwright action for double-clicking web elements.
    ///     Builds upon <see cref="LocatingElementAction" /> for locating elements in the DOM.
    ///     Provides full support for Playwright double-click capabilities, including position, button, modifiers, force, and
    ///     timeout settings.
    /// </summary>
    public DoubleClickAction(Builder builder) : this("doubleClick", builder) { }

    /// <summary>
    ///     Represents a Playwright action for double-clicking web elements.
    ///     Inherits from <see cref="LocatingElementAction" /> to use web element locating capabilities.
    ///     Provides support for all Playwright double-click options, including position, button, modifiers, and timing.
    /// </summary>
    public DoubleClickAction(string name, Builder builder) : base(name, builder)
    {
        _doubleClickOptions = builder.DoubleClickOptions;
    }

    /// <summary>
    ///     Executes the double-click action on the located element.
    /// </summary>
    /// <param name="locator">The located element to double-click</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    protected override async Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogDebug("Double-clicking element with options: {Options}",
                _doubleClickOptions?.ToString() ?? "default");

            await locator.DblClickAsync(_doubleClickOptions);

            Logger.LogInformation("Successfully double-clicked element");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to double-click element");
            throw new AgenixSystemException("Failed to double-click element", ex);
        }
    }

    /// <summary>
    ///     Builder class for creating DoubleClickAction instances with fluent API.
    ///     Inherits from LocatingElementAction.Builder to support all locator strategies.
    /// </summary>
    public new class Builder : LocatingElementAction.Builder
    {
        internal LocatorDblClickOptions? DoubleClickOptions { get; private set; }

        /// <summary>
        ///     Sets the double-click options directly.
        /// </summary>
        /// <param name="doubleClickOptions">The double-click options to use</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithDoubleClickOptions(LocatorDblClickOptions doubleClickOptions)
        {
            DoubleClickOptions = doubleClickOptions;
            return this;
        }

        /// <summary>
        ///     Configures double-click options using a fluent builder pattern.
        /// </summary>
        /// <param name="configureOptions">Action to configure the double-click options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithDoubleClickOptions(Action<DoubleClickOptionsBuilder> configureOptions)
        {
            var optionsBuilder = new DoubleClickOptionsBuilder();
            configureOptions(optionsBuilder);
            DoubleClickOptions = optionsBuilder.Build();
            return this;
        }

        /// <summary>
        ///     Sets the mouse button to use for double-clicking.
        /// </summary>
        /// <param name="button">The mouse button to use</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithButton(MouseButton button)
        {
            DoubleClickOptions ??= new LocatorDblClickOptions();
            DoubleClickOptions.Button = button;
            return this;
        }

        /// <summary>
        ///     Sets the delay between the two clicks in milliseconds.
        /// </summary>
        /// <param name="delay">The delay between clicks in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithDelay(float delay)
        {
            DoubleClickOptions ??= new LocatorDblClickOptions();
            DoubleClickOptions.Delay = delay;
            return this;
        }

        /// <summary>
        ///     Sets whether to force the double click even if the element is not visible.
        /// </summary>
        /// <param name="force">Whether to force the double click</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithForce(bool force = true)
        {
            DoubleClickOptions ??= new LocatorDblClickOptions();
            DoubleClickOptions.Force = force;
            return this;
        }

        /// <summary>
        ///     Sets modifier keys to hold during the double-click.
        /// </summary>
        /// <param name="modifiers">The modifier keys to hold</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithModifiers(IEnumerable<KeyboardModifier> modifiers)
        {
            DoubleClickOptions ??= new LocatorDblClickOptions();
            DoubleClickOptions.Modifiers = modifiers;
            return this;
        }

        /// <summary>
        ///     Sets the position to double-click relative to the element.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithPosition(float x, float y)
        {
            DoubleClickOptions ??= new LocatorDblClickOptions();
            DoubleClickOptions.Position = new Position { X = x, Y = y };
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the double-click operation.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(float timeout)
        {
            DoubleClickOptions ??= new LocatorDblClickOptions();
            DoubleClickOptions.Timeout = timeout;
            return this;
        }

        /// <summary>
        ///     Sets whether this is a trial run.
        /// </summary>
        /// <param name="trial">Whether this is a trial run</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTrial(bool trial = true)
        {
            DoubleClickOptions ??= new LocatorDblClickOptions();
            DoubleClickOptions.Trial = trial;
            return this;
        }

        /// <summary>
        ///     Builds the DoubleClickAction instance.
        /// </summary>
        /// <returns>A new DoubleClickAction instance</returns>
        public override DoubleClickAction Build()
        {
            return new DoubleClickAction(this);
        }
    }

    /// <summary>
    ///     Builder class for configuring double-click options using a fluent API.
    /// </summary>
    public class DoubleClickOptionsBuilder
    {
        private readonly LocatorDblClickOptions _options = new();

        /// <summary>
        ///     Sets the mouse button to use for double-clicking.
        /// </summary>
        /// <param name="button">The mouse button to use</param>
        /// <returns>The builder instance for method chaining</returns>
        public DoubleClickOptionsBuilder WithButton(MouseButton button)
        {
            _options.Button = button;
            return this;
        }

        /// <summary>
        ///     Sets the delay between the two clicks in milliseconds.
        /// </summary>
        /// <param name="delay">The delay between clicks in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public DoubleClickOptionsBuilder WithDelay(float delay)
        {
            _options.Delay = delay;
            return this;
        }

        /// <summary>
        ///     Sets whether to force the double click.
        /// </summary>
        /// <param name="force">Whether to force the double click</param>
        /// <returns>The builder instance for method chaining</returns>
        public DoubleClickOptionsBuilder WithForce(bool force = true)
        {
            _options.Force = force;
            return this;
        }

        /// <summary>
        ///     Sets modifier keys to hold during the double click.
        /// </summary>
        /// <param name="modifiers">The modifier keys to hold</param>
        /// <returns>The builder instance for method chaining</returns>
        public DoubleClickOptionsBuilder WithModifiers(IEnumerable<KeyboardModifier> modifiers)
        {
            _options.Modifiers = modifiers;
            return this;
        }

        /// <summary>
        ///     Sets the position to double-click relative to the element.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public DoubleClickOptionsBuilder WithPosition(float x, float y)
        {
            _options.Position = new Position { X = x, Y = y };
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the double-click operation.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public DoubleClickOptionsBuilder WithTimeout(float timeout)
        {
            _options.Timeout = timeout;
            return this;
        }

        /// <summary>
        ///     Sets whether this is a trial run.
        /// </summary>
        /// <param name="trial">Whether this is a trial run</param>
        /// <returns>The builder instance for method chaining</returns>
        public DoubleClickOptionsBuilder WithTrial(bool trial = true)
        {
            _options.Trial = trial;
            return this;
        }

        /// <summary>
        ///     Builds the LocatorDblClickOptions instance.
        /// </summary>
        /// <returns>A new LocatorDblClickOptions instance</returns>
        internal LocatorDblClickOptions Build()
        {
            return _options;
        }
    }
}
