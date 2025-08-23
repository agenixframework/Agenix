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
///     Playwright action for setting text values in input elements.
///     Supports various input methods including fill, clear and type, type only, and press sequence.
///     Uses Playwright's built-in input methods for reliable text entry.
/// </summary>
public class SetInputAction : LocatingElementAction
{
    /// <summary>
    ///     Enumeration of input methods for setting text values.
    /// </summary>
    public enum InputMethod
    {
        /// <summary>
        ///     Fill the input atomically - clears and sets value in one operation.
        ///     This is the fastest and most reliable method for most cases.
        /// </summary>
        FILL,

        /// <summary>
        ///     Clear the input first, then type the text.
        ///     Useful when you need to ensure the field is completely cleared.
        /// </summary>
        CLEAR_AND_TYPE,

        /// <summary>
        ///     Type the text without clearing first.
        ///     Appends to existing content in the input field.
        /// </summary>
        TYPE,

        /// <summary>
        ///     Press keys sequentially character by character.
        ///     Simulates real user typing with configurable delays.
        /// </summary>
        PRESS_SEQUENTIALLY
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(SetInputAction));
    private readonly int? _delay;
    private readonly bool _force;
    private readonly InputMethod _inputMethod;

    private readonly string _text;
    private readonly int? _timeout;

    /// <summary>
    ///     Represents a Playwright action for setting text in input elements using reliable and configurable methods.
    ///     Uses different techniques including filling, clearing and typing, typing only, or pressing keys sequentially.
    /// </summary>
    public SetInputAction(Builder builder) : this("set-input", builder) { }

    /// <summary>
    ///     Represents a Playwright action designed to set text in input elements using various input methods.
    /// </summary>
    public SetInputAction(string name, Builder builder) : base(name, builder)
    {
        _text = builder.Text ?? string.Empty;
        _inputMethod = builder.InputMethod;
        _force = builder.Force;
        _timeout = builder.Timeout;
        _delay = builder.Delay;
    }

    /// <summary>
    ///     Executes the set input action after locating the element.
    /// </summary>
    /// <param name="locator">The located input element</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            var displayText = _text.Length > 50 ? $"{_text[..50]}..." : _text;
            Logger.LogInformation("Executing set input action, method: {InputMethod}, text: {Text}",
                _inputMethod, displayText);

            await PerformInputAction(locator);

            Logger.LogInformation("Set input action completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute set input action");
            throw new AgenixSystemException("Failed to execute set input action", ex);
        }
    }

    /// <summary>
    ///     Performs the input action based on the specified method.
    /// </summary>
    /// <param name="locator">The input element locator</param>
    private async Task PerformInputAction(ILocator locator)
    {
        switch (_inputMethod)
        {
            case InputMethod.FILL:
                await PerformFill(locator);
                break;
            case InputMethod.CLEAR_AND_TYPE:
                await PerformClearAndType(locator);
                break;
            case InputMethod.TYPE:
                await PerformType(locator);
                break;
            case InputMethod.PRESS_SEQUENTIALLY:
                await PerformPressSequentially(locator);
                break;
            default:
                throw new ArgumentException($"Unsupported input method: {_inputMethod}");
        }
    }

    /// <summary>
    ///     Performs fill action - clears and sets the value atomically.
    /// </summary>
    /// <param name="locator">The input element locator</param>
    private async Task PerformFill(ILocator locator)
    {
        Logger.LogDebug("Filling element with text using fill method");

        var options = new LocatorFillOptions { Force = _force };

        if (_timeout.HasValue)
        {
            options.Timeout = _timeout.Value;
        }

        await locator.FillAsync(_text, options);
        Logger.LogDebug("Element filled successfully");
    }

    /// <summary>
    ///     Performs clear and type action - clears the field then types the text.
    /// </summary>
    /// <param name="locator">The input element locator</param>
    private async Task PerformClearAndType(ILocator locator)
    {
        Logger.LogDebug("Clearing element and typing text");

        // First, clear the element
        await locator.ClearAsync(new LocatorClearOptions { Force = _force, Timeout = _timeout });

        // Then type the text
        await PerformType(locator);
        Logger.LogDebug("Element cleared and text typed successfully");
    }

    /// <summary>
    ///     Performs type action - types text without clearing first.
    /// </summary>
    /// <param name="locator">The input element locator</param>
    private async Task PerformType(ILocator locator)
    {
        Logger.LogDebug("Typing text into element");

        var options = new LocatorFillOptions();

        if (_timeout.HasValue)
        {
            options.Timeout = _timeout.Value;
        }

        await locator.FillAsync(_text, options);
        Logger.LogDebug("Text typed successfully");
    }

    /// <summary>
    ///     Performs press sequentially action - types text character by character.
    /// </summary>
    /// <param name="locator">The input element locator</param>
    private async Task PerformPressSequentially(ILocator locator)
    {
        Logger.LogDebug("Pressing keys sequentially");

        var options = new LocatorPressSequentiallyOptions();

        if (_timeout.HasValue)
        {
            options.Timeout = _timeout.Value;
        }

        if (_delay.HasValue)
        {
            options.Delay = _delay.Value;
        }

        await locator.PressSequentiallyAsync(_text, options);
        Logger.LogDebug("Keys pressed sequentially successfully");
    }

    /// <summary>
    ///     Builder class for creating SetInputAction instances with fluent API.
    /// </summary>
    public new class Builder : LocatingElementAction.Builder
    {
        internal string? Text { get; private set; }
        internal InputMethod InputMethod { get; private set; } = InputMethod.FILL;
        internal bool Force { get; private set; }
        internal int? Timeout { get; private set; }
        internal int? Delay { get; private set; }

        /// <summary>
        ///     Sets the text to input into the element.
        /// </summary>
        /// <param name="text">The text to input</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithText(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            return this;
        }

        /// <summary>
        ///     Sets the text and uses the fill method (default).
        /// </summary>
        /// <param name="text">The text to fill</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Fill(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            InputMethod = InputMethod.FILL;
            return this;
        }

        /// <summary>
        ///     Sets the text and uses clear and type method.
        /// </summary>
        /// <param name="text">The text to type after clearing</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ClearAndType(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            InputMethod = InputMethod.CLEAR_AND_TYPE;
            return this;
        }

        /// <summary>
        ///     Sets the text and uses type method (appends to existing content).
        /// </summary>
        /// <param name="text">The text to type</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Type(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            InputMethod = InputMethod.TYPE;
            return this;
        }

        /// <summary>
        ///     Sets the text and uses press sequential method.
        /// </summary>
        /// <param name="text">The text to press sequentially</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder PressSequentially(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            InputMethod = InputMethod.PRESS_SEQUENTIALLY;
            return this;
        }

        /// <summary>
        ///     Sets the input method to use.
        /// </summary>
        /// <param name="method">The input method</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithInputMethod(InputMethod method)
        {
            InputMethod = method;
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
        ///     Sets the delay between key presses (for TYPE and PRESS_SEQUENTIALLY methods).
        /// </summary>
        /// <param name="delayMs">Delay in milliseconds between key presses</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithDelay(int delayMs)
        {
            Delay = delayMs;
            return this;
        }

        /// <summary>
        ///     Builds the SetInputAction instance.
        /// </summary>
        /// <returns>A new SetInputAction instance</returns>
        public override SetInputAction Build()
        {
            if (string.IsNullOrEmpty(Text))
            {
                throw new InvalidOperationException("Text must be specified for SetInputAction");
            }

            return new SetInputAction(this);
        }
    }
}
