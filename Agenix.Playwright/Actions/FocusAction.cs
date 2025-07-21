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
///     Playwright action for focusing on web elements.
///     Inherits from LocatingElementAction to leverage element location capabilities.
///     Supports Playwright focus options, including timeout configuration.
/// </summary>
public class FocusAction : LocatingElementAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(FocusAction));

    private readonly LocatorFocusOptions? _focusOptions;

    /// <summary>
    ///     Represents a Playwright action for focusing on web elements.
    ///     Builds upon <see cref="LocatingElementAction" /> for locating elements in the DOM.
    ///     Provides support for Playwright focus capabilities, including timeout settings.
    /// </summary>
    public FocusAction(Builder builder) : this("focus", builder) { }

    /// <summary>
    ///     Represents a Playwright action for focusing on web elements.
    ///     Inherits from <see cref="LocatingElementAction" /> to use web element locating capabilities.
    ///     Provides support for Playwright focus options, including timeout configuration.
    /// </summary>
    public FocusAction(string name, Builder builder) : base(name, builder)
    {
        _focusOptions = builder.FocusOptions;
    }

    /// <summary>
    ///     Executes the focus action on the located element.
    /// </summary>
    /// <param name="locator">The located element to focus on</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    protected override async Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogDebug("Focusing on element with options: {Options}", _focusOptions?.ToString() ?? "default");

            await locator.FocusAsync(_focusOptions);

            Logger.LogInformation("Successfully focused on element");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to focus on element");
            throw new AgenixSystemException("Failed to focus on element", ex);
        }
    }

    /// <summary>
    ///     Builder class for creating FocusAction instances with fluent API.
    ///     Inherits from LocatingElementAction.Builder to support all locator strategies.
    /// </summary>
    public new class Builder : LocatingElementAction.Builder
    {
        internal LocatorFocusOptions? FocusOptions { get; private set; }

        /// <summary>
        ///     Sets the focus options directly.
        /// </summary>
        /// <param name="focusOptions">The focus options to use</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFocusOptions(LocatorFocusOptions focusOptions)
        {
            FocusOptions = focusOptions;
            return this;
        }

        /// <summary>
        ///     Configures focus options using a fluent builder pattern.
        /// </summary>
        /// <param name="configureOptions">Action to configure the focus options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFocusOptions(Action<FocusOptionsBuilder> configureOptions)
        {
            var optionsBuilder = new FocusOptionsBuilder();
            configureOptions(optionsBuilder);
            FocusOptions = optionsBuilder.Build();
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the focus operation.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(float timeout)
        {
            FocusOptions ??= new LocatorFocusOptions();
            FocusOptions.Timeout = timeout;
            return this;
        }

        /// <summary>
        ///     Builds the FocusAction instance.
        /// </summary>
        /// <returns>A new FocusAction instance</returns>
        public override FocusAction Build()
        {
            return new FocusAction(this);
        }
    }

    /// <summary>
    ///     Builder class for configuring focus options using a fluent API.
    /// </summary>
    public class FocusOptionsBuilder
    {
        private readonly LocatorFocusOptions _options = new();

        /// <summary>
        ///     Sets the timeout for the focus operation.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public FocusOptionsBuilder WithTimeout(float timeout)
        {
            _options.Timeout = timeout;
            return this;
        }

        /// <summary>
        ///     Builds the LocatorFocusOptions instance.
        /// </summary>
        /// <returns>A new LocatorFocusOptions instance</returns>
        internal LocatorFocusOptions Build()
        {
            return _options;
        }
    }
}
