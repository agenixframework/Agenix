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
///     Playwright action for performing keyboard operations using page.Keyboard API.
///     Supports typing text, pressing keys, key combinations, and modifier key operations.
/// </summary>
public class KeyboardAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of keyboard operations.
    /// </summary>
    public enum KeyboardOperation
    {
        /// <summary>
        ///     Type text using Keyboard.TypeAsync().
        /// </summary>
        TYPE,

        /// <summary>
        ///     Press a key using Keyboard.PressAsync().
        /// </summary>
        PRESS,

        /// <summary>
        ///     Press the key down using Keyboard.DownAsync().
        /// </summary>
        DOWN,

        /// <summary>
        ///     Release key up using Keyboard.UpAsync().
        /// </summary>
        UP,

        /// <summary>
        ///     Insert text using Keyboard.InsertTextAsync().
        /// </summary>
        INSERT_TEXT,

        /// <summary>
        ///     Press multiple keys in combination.
        /// </summary>
        COMBINATION
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(KeyboardAction));
    private readonly int _delay;
    private readonly string? _key;
    private readonly List<string> _keys;

    private readonly KeyboardOperation _operation;
    private readonly string? _text;

    /// <summary>
    ///     Represents a Playwright action for performing keyboard operations using page.Keyboard API.
    /// </summary>
    public KeyboardAction(Builder builder) : this("keyboard", builder)
    {
    }

    /// <summary>
    ///     Playwright action for performing keyboard operations using page.Keyboard API.
    ///     Supports typing text, pressing keys, key combinations, and modifier key operations.
    /// </summary>
    public KeyboardAction(string name, Builder builder) : base(name, builder)
    {
        _operation = builder.Operation;
        _text = builder.Text;
        _key = builder.Key;
        _keys = builder.Keys;
        _delay = builder.Delay;
    }

    /// <summary>
    ///     Executes the keyboard action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing keyboard action: {Operation}", _operation);

            var page = browser.GetCurrentPage();
            if (page == null)
            {
                throw new InvalidOperationException("No active page available");
            }

            switch (_operation)
            {
                case KeyboardOperation.TYPE:
                    await ExecuteType(page, context);
                    break;

                case KeyboardOperation.PRESS:
                    await ExecutePress(page, context);
                    break;

                case KeyboardOperation.DOWN:
                    await ExecuteDown(page, context);
                    break;

                case KeyboardOperation.UP:
                    await ExecuteUp(page, context);
                    break;

                case KeyboardOperation.INSERT_TEXT:
                    await ExecuteInsertText(page, context);
                    break;

                case KeyboardOperation.COMBINATION:
                    await ExecuteCombination(page, context);
                    break;

                default:
                    throw new ArgumentException($"Unsupported keyboard operation: {_operation}");
            }

            Logger.LogInformation("Keyboard action completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute keyboard action");
            throw new AgenixSystemException("Failed to execute keyboard action", ex);
        }
    }

    /// <summary>
    ///     Executes type operation using page.Keyboard.TypeAsync().
    /// </summary>
    private async Task ExecuteType(IPage page, TestContext context)
    {
        if (string.IsNullOrEmpty(_text))
        {
            throw new InvalidOperationException("Text must be specified for type operation");
        }

        var resolvedText = context.ReplaceDynamicContentInString(_text);

        var options = new KeyboardTypeOptions();
        if (_delay > 0)
        {
            options.Delay = _delay;
        }

        await page.Keyboard.TypeAsync(resolvedText, options);
        Logger.LogDebug("Typed text: {Text}", resolvedText);
    }

    /// <summary>
    ///     Executes press operation using page.Keyboard.PressAsync().
    /// </summary>
    private async Task ExecutePress(IPage page, TestContext context)
    {
        if (string.IsNullOrEmpty(_key))
        {
            throw new InvalidOperationException("Key must be specified for press operation");
        }

        var resolvedKey = context.ReplaceDynamicContentInString(_key);

        var options = new KeyboardPressOptions();
        if (_delay > 0)
        {
            options.Delay = _delay;
        }

        await page.Keyboard.PressAsync(resolvedKey, options);
        Logger.LogDebug("Pressed key: {Key}", resolvedKey);
    }

    /// <summary>
    ///     Executes key down operation using page.Keyboard.DownAsync().
    /// </summary>
    private async Task ExecuteDown(IPage page, TestContext context)
    {
        if (string.IsNullOrEmpty(_key))
        {
            throw new InvalidOperationException("Key must be specified for down operation");
        }

        var resolvedKey = context.ReplaceDynamicContentInString(_key);
        await page.Keyboard.DownAsync(resolvedKey);
        Logger.LogDebug("Key down: {Key}", resolvedKey);
    }

    /// <summary>
    ///     Executes key up operation using page.Keyboard.UpAsync().
    /// </summary>
    private async Task ExecuteUp(IPage page, TestContext context)
    {
        if (string.IsNullOrEmpty(_key))
        {
            throw new InvalidOperationException("Key must be specified for up operation");
        }

        var resolvedKey = context.ReplaceDynamicContentInString(_key);
        await page.Keyboard.UpAsync(resolvedKey);
        Logger.LogDebug("Key up: {Key}", resolvedKey);
    }

    /// <summary>
    ///     Executes insert text operation using page.Keyboard.InsertTextAsync().
    /// </summary>
    private async Task ExecuteInsertText(IPage page, TestContext context)
    {
        if (string.IsNullOrEmpty(_text))
        {
            throw new InvalidOperationException("Text must be specified for insert text operation");
        }

        var resolvedText = context.ReplaceDynamicContentInString(_text);
        await page.Keyboard.InsertTextAsync(resolvedText);
        Logger.LogDebug("Inserted text: {Text}", resolvedText);
    }

    /// <summary>
    ///     Executes a key combination by pressing keys down in sequence and releasing them in reverse order.
    /// </summary>
    private async Task ExecuteCombination(IPage page, TestContext context)
    {
        if (_keys == null || _keys.Count == 0)
        {
            throw new InvalidOperationException("Keys must be specified for combination operation");
        }

        var resolvedKeys = _keys.Select(k => context.ReplaceDynamicContentInString(k)).ToList();

        try
        {
            // Press all keys down in order
            foreach (var key in resolvedKeys)
            {
                await page.Keyboard.DownAsync(key);
                Logger.LogDebug("Key down: {Key}", key);
            }

            // Small delay to ensure the combination is registered
            if (_delay > 0)
            {
                await Task.Delay(_delay);
            }
        }
        finally
        {
            // Release all keys in reverse order
            for (var i = resolvedKeys.Count - 1; i >= 0; i--)
            {
                await page.Keyboard.UpAsync(resolvedKeys[i]);
                Logger.LogDebug("Key up: {Key}", resolvedKeys[i]);
            }
        }

        Logger.LogDebug("Executed key combination: {Keys}", string.Join("+", resolvedKeys));
    }

    /// <summary>
    ///     Builder class for creating KeyboardAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<KeyboardAction, Builder>
    {
        internal KeyboardOperation Operation { get; private set; }
        internal string? Text { get; private set; }
        internal string? Key { get; private set; }
        internal List<string> Keys { get; private set; } = new();
        internal int Delay { get; private set; }

        /// <summary>
        ///     Configures the action to type text using Keyboard.TypeAsync().
        /// </summary>
        /// <param name="text">The text to type</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Type(string text)
        {
            Operation = KeyboardOperation.TYPE;
            Text = text;
            return this;
        }

        /// <summary>
        ///     Configures the action to press a key using Keyboard.PressAsync().
        /// </summary>
        /// <param name="key">The key to press (e.g., "Enter", "Escape", "Control+a")</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Press(string key)
        {
            Operation = KeyboardOperation.PRESS;
            Key = key;
            return this;
        }

        /// <summary>
        ///     Configures the action to press a key down using Keyboard.DownAsync().
        /// </summary>
        /// <param name="key">The key to press down</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Down(string key)
        {
            Operation = KeyboardOperation.DOWN;
            Key = key;
            return this;
        }

        /// <summary>
        ///     Configures the action to release a key up using Keyboard.UpAsync().
        /// </summary>
        /// <param name="key">The key to release</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Up(string key)
        {
            Operation = KeyboardOperation.UP;
            Key = key;
            return this;
        }

        /// <summary>
        ///     Configures the action to insert text using Keyboard.InsertTextAsync().
        /// </summary>
        /// <param name="text">The text to insert</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder InsertText(string text)
        {
            Operation = KeyboardOperation.INSERT_TEXT;
            Text = text;
            return this;
        }

        /// <summary>
        ///     Configures the action to press a key combination.
        /// </summary>
        /// <param name="keys">The keys to press in combination (e.g., "Control", "a")</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Combination(params string[] keys)
        {
            Operation = KeyboardOperation.COMBINATION;
            Keys = keys.ToList();
            return this;
        }

        /// <summary>
        ///     Sets the delay between key presses in milliseconds.
        /// </summary>
        /// <param name="delayMs">Delay in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithDelay(int delayMs)
        {
            Delay = delayMs;
            return this;
        }

        /// <summary>
        ///     Builds the KeyboardAction instance.
        /// </summary>
        /// <returns>A new KeyboardAction instance</returns>
        public override KeyboardAction Build()
        {
            return new KeyboardAction(this);
        }
    }
}
