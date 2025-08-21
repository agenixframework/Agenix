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

using Agenix.Playwright.Actions;
using Agenix.Screenplay.Abilities;
using Agenix.Screenplay.Annotations;
using Microsoft.Playwright;

namespace Agenix.Screenplay.Playwright.Interactions;

/// <summary>
/// A performable that clicks on a target element.
/// This is a high-level abstraction that integrates with the Screenplay pattern.
/// </summary>
public class Click : IPerformable
{
    private readonly Target _target;
    private readonly LocatorClickOptions? _clickOptions;

    private Click(Target target, LocatorClickOptions? clickOptions = null)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _clickOptions = clickOptions;
    }

    /// <summary>
    /// Creates a Click performable for the specified target.
    /// </summary>
    /// <param name="target">The target element to click on.</param>
    /// <returns>A Click performable.</returns>
    public static Click On(Target target)
    {
        return new Click(target);
    }

    /// <summary>
    /// Creates a Click performable with specific click options.
    /// </summary>
    /// <param name="target">The target element to click on.</param>
    /// <param name="options">The click options to use.</param>
    /// <returns>A Click performable with options.</returns>
    public static Click On(Target target, LocatorClickOptions options)
    {
        return new Click(target, options);
    }

    /// <summary>
    /// Creates a Click performable with a specific mouse button.
    /// </summary>
    /// <param name="target">The target element to click on.</param>
    /// <param name="button">The mouse button to use for clicking.</param>
    /// <returns>A Click performable with the specified button.</returns>
    public static Click On(Target target, MouseButton button)
    {
        var options = new LocatorClickOptions { Button = button };
        return new Click(target, options);
    }

    /// <summary>
    /// Adds click options to this Click performable.
    /// </summary>
    /// <param name="options">The click options to apply.</param>
    /// <returns>A new Click performable with the specified options.</returns>
    public Click WithOptions(LocatorClickOptions options)
    {
        return new Click(_target, options);
    }

    /// <summary>
    /// Specifies the mouse button to use for clicking.
    /// </summary>
    /// <param name="button">The mouse button to use.</param>
    /// <returns>A new Click performable with the specified button.</returns>
    public Click WithButton(MouseButton button)
    {
        var newOptions = _clickOptions != null
            ? CloneOptions(_clickOptions)
            : new LocatorClickOptions();
        newOptions.Button = button;
        return new Click(_target, newOptions);
    }

    /// <summary>
    /// Specifies the number of clicks to perform.
    /// </summary>
    /// <param name="clickCount">The number of clicks (default is 1).</param>
    /// <returns>A new Click performable with the specified click count.</returns>
    public Click WithClickCount(int clickCount)
    {
        if (clickCount <= 0)
            throw new ArgumentException("Click count must be positive", nameof(clickCount));

        var newOptions = _clickOptions != null
            ? CloneOptions(_clickOptions)
            : new LocatorClickOptions();
        newOptions.ClickCount = clickCount;
        return new Click(_target, newOptions);
    }

    /// <summary>
    /// Specifies the delay between mouse down and mouse up.
    /// </summary>
    /// <param name="delay">The delay in milliseconds.</param>
    /// <returns>A new Click performable with the specified delay.</returns>
    public Click WithDelay(float delay)
    {
        if (delay < 0)
            throw new ArgumentException("Delay cannot be negative", nameof(delay));

        var newOptions = _clickOptions != null
            ? CloneOptions(_clickOptions)
            : new LocatorClickOptions();
        newOptions.Delay = delay;
        return new Click(_target, newOptions);
    }

    /// <summary>
    /// Forces the click action even if the element is not actionable.
    /// </summary>
    /// <param name="force">Whether to force the click.</param>
    /// <returns>A new Click performable with force option.</returns>
    public Click WithForce(bool force = true)
    {
        var newOptions = _clickOptions != null
            ? CloneOptions(_clickOptions)
            : new LocatorClickOptions();
        newOptions.Force = force;
        return new Click(_target, newOptions);
    }

    /// <summary>
    /// Specifies keyboard modifiers to hold during the click.
    /// </summary>
    /// <param name="modifiers">The keyboard modifiers to hold.</param>
    /// <returns>A new Click performable with the specified modifiers.</returns>
    public Click WithModifiers(params KeyboardModifier[] modifiers)
    {
        var newOptions = _clickOptions != null
            ? CloneOptions(_clickOptions)
            : new LocatorClickOptions();
        newOptions.Modifiers = modifiers;
        return new Click(_target, newOptions);
    }

    /// <summary>
    /// Specifies a custom position to click within the element.
    /// </summary>
    /// <param name="x">The x coordinate relative to the element.</param>
    /// <param name="y">The y coordinate relative to the element.</param>
    /// <returns>A new Click performable with the specified position.</returns>
    public Click AtPosition(float x, float y)
    {
        var newOptions = _clickOptions != null
            ? CloneOptions(_clickOptions)
            : new LocatorClickOptions();
        newOptions.Position = new Position { X = x, Y = y };
        return new Click(_target, newOptions);
    }

    /// <summary>
    /// Specifies a timeout for the click action.
    /// </summary>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <returns>A new Click performable with the specified timeout.</returns>
    public Click WithTimeout(float timeout)
    {
        if (timeout < 0)
            throw new ArgumentException("Timeout cannot be negative", nameof(timeout));

        var newOptions = _clickOptions != null
            ? CloneOptions(_clickOptions)
            : new LocatorClickOptions();
        newOptions.Timeout = timeout;
        return new Click(_target, newOptions);
    }

    /// <summary>
    /// Performs the click action using the provided actor.
    /// </summary>
    /// <typeparam name="T">The type of actor performing the action.</typeparam>
    /// <param name="actor">The actor that will perform the click action.</param>
    [Step("{0} clicks on #target")]
    public void PerformAs<T>(T actor) where T : Actor
    {
        // Build the ClickAction using the utility
        var clickActionBuilder = TargetApplier.ApplyTarget(new ClickAction.Builder(), _target);

        // Apply click options if provided
        if (_clickOptions != null)
        {
            clickActionBuilder.WithClickOptions(_clickOptions);
        }

        UseTheGherkinTestActionRunner.As(actor).TestCaseRunner.When(clickActionBuilder.Build());
    }

    /// <summary>
    /// Returns a string representation of this Click performable.
    /// </summary>
    public override string ToString()
    {
        var optionsText = _clickOptions != null ? " (with options)" : "";
        return $"Click on {_target.Name}{optionsText}";
    }

    /// <summary>
    /// Creates a copy of the click options.
    /// </summary>
    private static LocatorClickOptions CloneOptions(LocatorClickOptions original)
    {
        return new LocatorClickOptions
        {
            Button = original.Button,
            ClickCount = original.ClickCount,
            Delay = original.Delay,
            Force = original.Force,
            Modifiers = original.Modifiers,
            Position = original.Position,
            Timeout = original.Timeout,
            Trial = original.Trial
        };
    }
}
