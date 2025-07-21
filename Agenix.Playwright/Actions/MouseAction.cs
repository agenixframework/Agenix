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
///     Playwright action for performing mouse operations using page.Mouse API.
///     Supports clicking, moving, dragging, scrolling, and button press/release operations.
/// </summary>
public class MouseAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of mouse operations.
    /// </summary>
    public enum MouseOperation
    {
        /// <summary>
        ///     Click using Mouse.ClickAsync().
        /// </summary>
        CLICK,

        /// <summary>
        ///     Double-click using Mouse.DblClickAsync().
        /// </summary>
        DBL_CLICK,

        /// <summary>
        ///     Mouse the button down using Mouse.DownAsync().
        /// </summary>
        DOWN,

        /// <summary>
        ///     Mouse button up using Mouse.UpAsync().
        /// </summary>
        UP,

        /// <summary>
        ///     Move mouse using Mouse.MoveAsync().
        /// </summary>
        MOVE,

        /// <summary>
        ///     Mouse wheel scroll using Mouse.WheelAsync().
        /// </summary>
        WHEEL
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(MouseAction));
    private readonly MouseButton _button;
    private readonly int _clickCount;
    private readonly int _delay;
    private readonly float _deltaX;
    private readonly float _deltaY;

    private readonly MouseOperation _operation;
    private readonly int _steps;
    private readonly float? _targetX;
    private readonly float? _targetY;
    private readonly float _x;
    private readonly float _y;

    /// <summary>
    ///     Represents a Playwright action for performing mouse operations using page. Mouse API.
    /// </summary>
    public MouseAction(Builder builder) : this("mouse", builder)
    {
    }

    /// <summary>
    ///     Playwright action for performing mouse operations using page.Mouse API.
    ///     Supports clicking, moving, dragging, scrolling, and button press/release operations.
    /// </summary>
    public MouseAction(string name, Builder builder) : base(name, builder)
    {
        _operation = builder.Operation;
        _x = builder.X;
        _y = builder.Y;
        _targetX = builder.TargetX;
        _targetY = builder.TargetY;
        _button = builder.Button;
        _clickCount = builder.ClickCount;
        _delay = builder.Delay;
        _deltaX = builder.DeltaX;
        _deltaY = builder.DeltaY;
        _steps = builder.Steps;
    }

    /// <summary>
    ///     Executes the mouse action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing mouse action: {Operation}", _operation);

            var page = browser.GetCurrentPage();
            if (page == null)
            {
                throw new InvalidOperationException("No active page available");
            }

            switch (_operation)
            {
                case MouseOperation.CLICK:
                    await ExecuteClick(page);
                    break;

                case MouseOperation.DBL_CLICK:
                    await ExecuteDblClick(page);
                    break;

                case MouseOperation.DOWN:
                    await ExecuteDown(page);
                    break;

                case MouseOperation.UP:
                    await ExecuteUp(page);
                    break;

                case MouseOperation.MOVE:
                    await ExecuteMove(page);
                    break;

                case MouseOperation.WHEEL:
                    await ExecuteWheel(page);
                    break;

                default:
                    throw new ArgumentException($"Unsupported mouse operation: {_operation}");
            }

            Logger.LogInformation("Mouse action completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute mouse action");
            throw new AgenixSystemException("Failed to execute mouse action", ex);
        }
    }

    /// <summary>
    ///     Executes click operation using page.Mouse.ClickAsync().
    /// </summary>
    private async Task ExecuteClick(IPage page)
    {
        var options = new MouseClickOptions { Button = _button, ClickCount = _clickCount > 0 ? _clickCount : 1 };

        if (_delay > 0)
        {
            options.Delay = _delay;
        }

        await page.Mouse.ClickAsync(_x, _y, options);
        Logger.LogDebug("Clicked at position ({X}, {Y}) with button {Button}, count: {ClickCount}",
            _x, _y, _button, options.ClickCount);
    }

    /// <summary>
    ///     Executes double click operation using page.Mouse.DblClickAsync().
    /// </summary>
    private async Task ExecuteDblClick(IPage page)
    {
        var options = new MouseDblClickOptions { Button = _button };

        if (_delay > 0)
        {
            options.Delay = _delay;
        }

        await page.Mouse.DblClickAsync(_x, _y, options);
        Logger.LogDebug("Double-clicked at position ({X}, {Y}) with button {Button}", _x, _y, _button);
    }

    /// <summary>
    ///     Executes mouse down operation using page.Mouse.DownAsync().
    /// </summary>
    private async Task ExecuteDown(IPage page)
    {
        var options = new MouseDownOptions { Button = _button, ClickCount = _clickCount > 0 ? _clickCount : 1 };

        await page.Mouse.DownAsync(options);
        Logger.LogDebug("Mouse down with button {Button}, count: {ClickCount}", _button, options.ClickCount);
    }

    /// <summary>
    ///     Executes mouse up operation using page.Mouse.UpAsync().
    /// </summary>
    private async Task ExecuteUp(IPage page)
    {
        var options = new MouseUpOptions { Button = _button, ClickCount = _clickCount > 0 ? _clickCount : 1 };

        await page.Mouse.UpAsync(options);
        Logger.LogDebug("Mouse up with button {Button}, count: {ClickCount}", _button, options.ClickCount);
    }

    /// <summary>
    ///     Executes mouse move operation using page.Mouse.MoveAsync().
    /// </summary>
    private async Task ExecuteMove(IPage page)
    {
        var options = new MouseMoveOptions();

        if (_steps > 0)
        {
            options.Steps = _steps;
        }

        // If target coordinates are specified, move to target; otherwise move to x,y
        var targetX = _targetX ?? _x;
        var targetY = _targetY ?? _y;

        await page.Mouse.MoveAsync(targetX, targetY, options);
        Logger.LogDebug("Moved mouse to position ({X}, {Y}) with {Steps} steps", targetX, targetY, options.Steps);
    }

    /// <summary>
    ///     Executes mouse wheel operation using page.Mouse.WheelAsync().
    /// </summary>
    private async Task ExecuteWheel(IPage page)
    {
        await page.Mouse.WheelAsync(_deltaX, _deltaY);
        Logger.LogDebug("Mouse wheel scrolled by delta ({DeltaX}, {DeltaY})", _deltaX, _deltaY);
    }

    /// <summary>
    ///     Builder class for creating MouseAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<MouseAction, Builder>
    {
        internal MouseOperation Operation { get; private set; }
        internal float X { get; private set; }
        internal float Y { get; private set; }
        internal float? TargetX { get; private set; }
        internal float? TargetY { get; private set; }
        internal MouseButton Button { get; private set; } = MouseButton.Left;
        internal int ClickCount { get; private set; } = 1;
        internal int Delay { get; private set; }
        internal float DeltaX { get; private set; }
        internal float DeltaY { get; private set; }
        internal int Steps { get; private set; } = 1;

        /// <summary>
        ///     Configures the action to click using Mouse.ClickAsync().
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Click(float x, float y)
        {
            Operation = MouseOperation.CLICK;
            X = x;
            Y = y;
            return this;
        }

        /// <summary>
        ///     Configures the action to double-click using Mouse.DblClickAsync().
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder DblClick(float x, float y)
        {
            Operation = MouseOperation.DBL_CLICK;
            X = x;
            Y = y;
            return this;
        }

        /// <summary>
        ///     Configures the action to press mouse button down using Mouse.DownAsync().
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Down()
        {
            Operation = MouseOperation.DOWN;
            return this;
        }

        /// <summary>
        ///     Configures the action to release mouse button up using Mouse.UpAsync().
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Up()
        {
            Operation = MouseOperation.UP;
            return this;
        }

        /// <summary>
        ///     Configures the action to move mouse using Mouse.MoveAsync().
        /// </summary>
        /// <param name="x">Target X coordinate</param>
        /// <param name="y">Target Y coordinate</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Move(float x, float y)
        {
            Operation = MouseOperation.MOVE;
            TargetX = x;
            TargetY = y;
            return this;
        }

        /// <summary>
        ///     Configures the action to scroll mouse wheel using Mouse.WheelAsync().
        /// </summary>
        /// <param name="deltaX">Horizontal scroll delta</param>
        /// <param name="deltaY">Vertical scroll delta</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Wheel(float deltaX, float deltaY)
        {
            Operation = MouseOperation.WHEEL;
            DeltaX = deltaX;
            DeltaY = deltaY;
            return this;
        }

        /// <summary>
        ///     Sets the mouse button to use.
        /// </summary>
        /// <param name="button">The mouse button</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithButton(MouseButton button)
        {
            Button = button;
            return this;
        }

        /// <summary>
        ///     Sets the click count for click operations.
        /// </summary>
        /// <param name="count">Number of clicks</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithClickCount(int count)
        {
            ClickCount = count;
            return this;
        }

        /// <summary>
        ///     Sets the delay between mouse operations in milliseconds.
        /// </summary>
        /// <param name="delayMs">Delay in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithDelay(int delayMs)
        {
            Delay = delayMs;
            return this;
        }

        /// <summary>
        ///     Sets the number of steps for move operations.
        /// </summary>
        /// <param name="steps">Number of intermediate steps</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSteps(int steps)
        {
            Steps = steps;
            return this;
        }

        /// <summary>
        ///     Convenience method to set left mouse button.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder LeftButton()
        {
            Button = MouseButton.Left;
            return this;
        }

        /// <summary>
        ///     Convenience method to set right mouse button.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder RightButton()
        {
            Button = MouseButton.Right;
            return this;
        }

        /// <summary>
        ///     Convenience method to set middle mouse button.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder MiddleButton()
        {
            Button = MouseButton.Middle;
            return this;
        }

        /// <summary>
        ///     Builds the MouseAction instance.
        /// </summary>
        /// <returns>A new MouseAction instance</returns>
        public override MouseAction Build()
        {
            return new MouseAction(this);
        }
    }
}
