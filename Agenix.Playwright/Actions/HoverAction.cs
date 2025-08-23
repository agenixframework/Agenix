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
///     Playwright action for hovering over web elements.
///     Inherits from LocatingElementAction to leverage element location capabilities.
///     Supports all Playwright hover options, including position, modifiers, and timing.
/// </summary>
public class HoverAction : LocatingElementAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(HoverAction));

    private readonly LocatorHoverOptions? _hoverOptions;

    /// <summary>
    ///     Represents a Playwright action for hovering over web elements.
    ///     Builds upon <see cref="LocatingElementAction" /> for locating elements in the DOM.
    ///     Provides full support for Playwright hover capabilities, including position, modifiers, force, and timeout
    ///     settings.
    /// </summary>
    public HoverAction(Builder builder) : this("hover", builder) { }

    /// <summary>
    ///     Represents a Playwright action for hovering over web elements.
    ///     Inherits from <see cref="LocatingElementAction" /> to use web element locating capabilities.
    ///     Provides support for all Playwright hover options, including position, modifiers, and timing.
    /// </summary>
    public HoverAction(string name, Builder builder) : base(name, builder)
    {
        _hoverOptions = builder.HoverOptions;
    }


    /// <summary>
    ///     Executes the hover action on the located element.
    /// </summary>
    /// <param name="locator">The located element to hover over</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    protected override async Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogDebug("Hovering over element with options: {Options}", _hoverOptions?.ToString() ?? "default");

            await locator.HoverAsync(_hoverOptions);

            Logger.LogInformation("Successfully hovered over element");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to hover over element");
            throw new AgenixSystemException("Failed to hover over element", ex);
        }
    }

    /// <summary>
    ///     Builder class for creating HoverAction instances with fluent API.
    ///     Inherits from LocatingElementAction.Builder to support all locator strategies.
    /// </summary>
    public new class Builder : LocatingElementAction.Builder
    {
        internal LocatorHoverOptions? HoverOptions { get; private set; }

        /// <summary>
        ///     Sets the hover options directly.
        /// </summary>
        /// <param name="hoverOptions">The hover options to use</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithHoverOptions(LocatorHoverOptions hoverOptions)
        {
            HoverOptions = hoverOptions;
            return this;
        }

        /// <summary>
        ///     Configures hover options using a fluent builder pattern.
        /// </summary>
        /// <param name="configureOptions">Action to configure the hover options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithHoverOptions(Action<HoverOptionsBuilder> configureOptions)
        {
            var optionsBuilder = new HoverOptionsBuilder();
            configureOptions(optionsBuilder);
            HoverOptions = optionsBuilder.Build();
            return this;
        }

        /// <summary>
        ///     Sets whether to force the hover even if the element is not visible.
        /// </summary>
        /// <param name="force">Whether to force the hover</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithForce(bool force = true)
        {
            HoverOptions ??= new LocatorHoverOptions();
            HoverOptions.Force = force;
            return this;
        }

        /// <summary>
        ///     Sets modifier keys to hold during the hover.
        /// </summary>
        /// <param name="modifiers">The modifier keys to hold</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithModifiers(IEnumerable<KeyboardModifier> modifiers)
        {
            HoverOptions ??= new LocatorHoverOptions();
            HoverOptions.Modifiers = modifiers;
            return this;
        }

        /// <summary>
        ///     Sets the position to hover relative to the element.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithPosition(float x, float y)
        {
            HoverOptions ??= new LocatorHoverOptions();
            HoverOptions.Position = new Position { X = x, Y = y };
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the hover operation.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(float timeout)
        {
            HoverOptions ??= new LocatorHoverOptions();
            HoverOptions.Timeout = timeout;
            return this;
        }

        /// <summary>
        ///     Sets whether this is a trial run.
        /// </summary>
        /// <param name="trial">Whether this is a trial run</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTrial(bool trial = true)
        {
            HoverOptions ??= new LocatorHoverOptions();
            HoverOptions.Trial = trial;
            return this;
        }

        /// <summary>
        ///     Builds the HoverAction instance.
        /// </summary>
        /// <returns>A new HoverAction instance</returns>
        public override HoverAction Build()
        {
            return new HoverAction(this);
        }
    }

    /// <summary>
    ///     Builder class for configuring hover options using a fluent API.
    /// </summary>
    public class HoverOptionsBuilder
    {
        private readonly LocatorHoverOptions _options = new();

        /// <summary>
        ///     Sets whether to force the hover.
        /// </summary>
        /// <param name="force">Whether to force the hover</param>
        /// <returns>The builder instance for method chaining</returns>
        public HoverOptionsBuilder WithForce(bool force = true)
        {
            _options.Force = force;
            return this;
        }

        /// <summary>
        ///     Sets modifier keys to hold during the hover.
        /// </summary>
        /// <param name="modifiers">The modifier keys to hold</param>
        /// <returns>The builder instance for method chaining</returns>
        public HoverOptionsBuilder WithModifiers(IEnumerable<KeyboardModifier> modifiers)
        {
            _options.Modifiers = modifiers;
            return this;
        }

        /// <summary>
        ///     Sets the position to hover relative to the element.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public HoverOptionsBuilder WithPosition(float x, float y)
        {
            _options.Position = new Position { X = x, Y = y };
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the hover operation.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public HoverOptionsBuilder WithTimeout(float timeout)
        {
            _options.Timeout = timeout;
            return this;
        }

        /// <summary>
        ///     Sets whether this is a trial run.
        /// </summary>
        /// <param name="trial">Whether this is a trial run</param>
        /// <returns>The builder instance for method chaining</returns>
        public HoverOptionsBuilder WithTrial(bool trial = true)
        {
            _options.Trial = trial;
            return this;
        }

        /// <summary>
        ///     Builds the LocatorHoverOptions instance.
        /// </summary>
        /// <returns>A new LocatorHoverOptions instance</returns>
        internal LocatorHoverOptions Build()
        {
            return _options;
        }
    }
}
