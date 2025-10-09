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

using System.Text.RegularExpressions;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for making expectations about locator elements.
///     This action extends LocatingElementAction to leverage its locator capabilities
///     and adds Playwright's built-in locator assertions for reliable testing.
/// </summary>
public class ExpectLocatorAction : LocatingElementAction
{
    /// <summary>
    ///     Enumeration of locator expectation types.
    /// </summary>
    public enum ExpectedType
    {
        /// <summary>
        ///     Expect the locator to be visible.
        /// </summary>
        TO_BE_VISIBLE,

        /// <summary>
        ///     Expect the locator to be hidden.
        /// </summary>
        TO_BE_HIDDEN,

        /// <summary>
        ///     Expect the locator to be enabled.
        /// </summary>
        TO_BE_ENABLED,

        /// <summary>
        ///     Expect the locator to be disabled.
        /// </summary>
        TO_BE_DISABLED,

        /// <summary>
        ///     Expect the locator to be checked.
        /// </summary>
        TO_BE_CHECKED,

        /// <summary>
        ///     Expect the locator to be focused.
        /// </summary>
        TO_BE_FOCUSED,

        /// <summary>
        ///     Expect the locator to be editable.
        /// </summary>
        TO_BE_EDITABLE,

        /// <summary>
        ///     Expect the locator to be attached to the DOM.
        /// </summary>
        TO_BE_ATTACHED,

        /// <summary>
        ///     Expect the locator to be empty.
        /// </summary>
        TO_BE_EMPTY,

        /// <summary>
        ///     Expect the locator to be in viewport.
        /// </summary>
        TO_BE_IN_VIEWPORT,

        /// <summary>
        ///     Expect the locator to have specific text.
        /// </summary>
        TO_HAVE_TEXT,

        /// <summary>
        ///     Expect the locator to contain specific text.
        /// </summary>
        TO_CONTAIN_TEXT,

        /// <summary>
        ///     Expect the locator to have a specific value.
        /// </summary>
        TO_HAVE_VALUE,

        /// <summary>
        ///     Expect the locator to have specific values (for multi-select elements).
        /// </summary>
        TO_HAVE_VALUES,

        /// <summary>
        ///     Expect the locator to have a specific attribute.
        /// </summary>
        TO_HAVE_ATTRIBUTE,

        /// <summary>
        ///     Expect the locator to have a specific count.
        /// </summary>
        TO_HAVE_COUNT,

        /// <summary>
        ///     Expect the locator to have a specific class.
        /// </summary>
        TO_HAVE_CLASS,

        /// <summary>
        ///     Expect the locator to contain a specific class.
        /// </summary>
        TO_CONTAIN_CLASS,

        /// <summary>
        ///     Expect the locator to have a specific CSS property.
        /// </summary>
        TO_HAVE_CSS,

        /// <summary>
        ///     Expect the locator to have a specific ID.
        /// </summary>
        TO_HAVE_ID,

        /// <summary>
        ///     Expect the locator to have a specific JavaScript property.
        /// </summary>
        TO_HAVE_JS_PROPERTY,

        /// <summary>
        ///     Expect the locator to have a specific ARIA role.
        /// </summary>
        TO_HAVE_ROLE,

        /// <summary>
        ///     Expect the locator to have a specific accessible name.
        /// </summary>
        TO_HAVE_ACCESSIBLE_NAME,

        /// <summary>
        ///     Expect the locator to have a specific accessible description.
        /// </summary>
        TO_HAVE_ACCESSIBLE_DESCRIPTION,

        /// <summary>
        ///     Expect the locator to have a specific accessible error message.
        /// </summary>
        TO_HAVE_ACCESSIBLE_ERROR_MESSAGE,

        /// <summary>
        ///     Expect the locator to match a specific ARIA snapshot.
        /// </summary>
        TO_MATCH_ARIA_SNAPSHOT
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(ExpectLocatorAction));

    private readonly List<ExpectationDefinition> _expectations;

    /// <summary>
    ///     Provides a Playwright action for making expectations about locator elements.
    /// </summary>
    public ExpectLocatorAction(Builder builder) : this("expect-locator", builder) { }

    /// <summary>
    ///     Provides a Playwright action for making expectations about locator elements.
    /// </summary>
    public ExpectLocatorAction(string name, Builder builder) : base(name, builder)
    {
        _expectations = builder.GetExpectations();
        ContinueOnFailure = builder.ContinueOnFailure;
    }

    /// <summary>
    ///     Gets the type of expectation being performed.
    /// </summary>
    public IReadOnlyList<ExpectationDefinition> Expectations => _expectations;

    public bool ContinueOnFailure { get; }

    /// <summary>
    ///     Executes the expectation on the located element.
    ///     This method is called after the element is located by the base LocatingElementAction.
    /// </summary>
    /// <param name="locator">The located element</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    protected override async Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        var failures = new List<Exception>();

        Logger.LogInformation(
            "Executing {ExpectationCount} locator expectations with ContinueOnFailure={ContinueOnFailure}",
            _expectations.Count, ContinueOnFailure);

        foreach (var expectation in _expectations)
        {
            try
            {
                Logger.LogDebug("Executing expectation: {ExpectationType}", expectation.Type);
                await ExecuteExpectation(locator, expectation, context);
                Logger.LogDebug("Expectation {ExpectationType} completed successfully", expectation.Type);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to execute expectation: {ExpectationType}", expectation.Type);

                if (ContinueOnFailure)
                {
                    failures.Add(ex);
                    Logger.LogWarning("Continuing with next expectation due to ContinueOnFailure=true");
                    continue;
                }

                throw new AgenixSystemException($"Failed to execute locator expectation: {expectation.Type}", ex);
            }
        }

        if (failures.Count != 0)
        {
            var aggregateException = new AggregateException(
                $"One or more expectations failed ({failures.Count} of {_expectations.Count})",
                failures);

            Logger.LogError(aggregateException, "Soft assertions completed with {FailureCount} failures",
                failures.Count);
            throw new AgenixSystemException("Soft assertions failed", aggregateException);
        }

        Logger.LogInformation("All locator expectations completed successfully");
    }

    private async Task ExecuteExpectation(ILocator locator, ExpectationDefinition expectation, TestContext context)
    {
        var locatorAssertions = Expect(locator);

        switch (expectation.Type)
        {
            case ExpectedType.TO_BE_VISIBLE:
                await ExpectToBeVisible(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_BE_HIDDEN:
                await ExpectToBeHidden(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_BE_ENABLED:
                await ExpectToBeEnabled(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_BE_DISABLED:
                await ExpectToBeDisabled(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_BE_CHECKED:
                await ExpectToBeChecked(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_BE_FOCUSED:
                await ExpectToBeFocused(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_BE_EDITABLE:
                await ExpectToBeEditable(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_BE_ATTACHED:
                await ExpectToBeAttached(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_BE_EMPTY:
                await ExpectToBeEmpty(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_BE_IN_VIEWPORT:
                await ExpectToBeInViewport(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_HAVE_TEXT:
                await ExpectToHaveText(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_CONTAIN_TEXT:
                await ExpectToContainText(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_VALUE:
                await ExpectToHaveValue(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_VALUES:
                await ExpectToHaveValues(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_ATTRIBUTE:
                await ExpectToHaveAttribute(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_COUNT:
                await ExpectToHaveCount(locatorAssertions, expectation);
                break;

            case ExpectedType.TO_HAVE_CLASS:
                await ExpectToHaveClass(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_CONTAIN_CLASS:
                await ExpectToContainClass(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_CSS:
                await ExpectToHaveCss(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_ID:
                await ExpectToHaveId(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_JS_PROPERTY:
                await ExpectToHaveJsProperty(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_ROLE:
                await ExpectToHaveRole(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_ACCESSIBLE_NAME:
                await ExpectToHaveAccessibleName(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_ACCESSIBLE_DESCRIPTION:
                await ExpectToHaveAccessibleDescription(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_HAVE_ACCESSIBLE_ERROR_MESSAGE:
                await ExpectToHaveAccessibleErrorMessage(locatorAssertions, expectation, context);
                break;

            case ExpectedType.TO_MATCH_ARIA_SNAPSHOT:
                await ExpectToMatchAriaSnapshot(locatorAssertions, expectation, context);
                break;

            default:
                throw new InvalidOperationException($"Unknown expectation type: {expectation.Type}");
        }
    }

    private async Task ExpectToBeVisible(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeVisibleOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeVisibleAsync(options);
    }

    private async Task ExpectToBeHidden(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeHiddenOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeHiddenAsync(options);
    }

    private async Task ExpectToBeEnabled(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeEnabledOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeEnabledAsync(options);
    }

    private async Task ExpectToBeDisabled(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeDisabledOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeDisabledAsync(options);
    }

    private async Task ExpectToBeChecked(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeCheckedOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeCheckedAsync(options);
    }

    private async Task ExpectToBeFocused(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeFocusedOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeFocusedAsync(options);
    }

    private async Task ExpectToBeEditable(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeEditableOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeEditableAsync(options);
    }

    private async Task ExpectToBeAttached(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeAttachedOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeAttachedAsync(options);
    }

    private async Task ExpectToBeEmpty(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeEmptyOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeEmptyAsync(options);
    }

    private async Task ExpectToBeInViewport(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        var options = new LocatorAssertionsToBeInViewportOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToBeInViewportAsync(options);
    }

    private async Task ExpectToHaveText(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        var options = new LocatorAssertionsToHaveTextOptions
        {
            Timeout = expectation.TimeoutMs,
            IgnoreCase = expectation.IgnoreCase
        };

        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToHaveTextAsync(expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected text value: {Original} -> {Resolved}", expectation.Value, resolvedValue);
            await assertion.ToHaveTextAsync(resolvedValue, options);
        }
        else if (expectation.Values != null)
        {
            var resolvedValues = context.ResolveDynamicValuesInArray(expectation.Values);
            Logger.LogDebug("Resolved expected text values: {Count} values", resolvedValues.Length);
            await assertion.ToHaveTextAsync(resolvedValues, options);
        }
        else
        {
            throw new InvalidOperationException(
                "Expected value, regex, or values must be specified for text expectation");
        }
    }

    private async Task ExpectToContainText(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        var options = new LocatorAssertionsToContainTextOptions
        {
            Timeout = expectation.TimeoutMs,
            IgnoreCase = expectation.IgnoreCase
        };

        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToContainTextAsync(expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected contain text value: {Original} -> {Resolved}", expectation.Value,
                resolvedValue);
            await assertion.ToContainTextAsync(resolvedValue, options);
        }
        else if (expectation.Values != null)
        {
            var resolvedValues = context.ResolveDynamicValuesInArray(expectation.Values);
            Logger.LogDebug("Resolved expected contain text values: {Count} values", resolvedValues.Length);
            await assertion.ToContainTextAsync(resolvedValues, options);
        }
        else
        {
            throw new InvalidOperationException(
                "Expected value, regex, or values must be specified for contain text expectation");
        }
    }

    private async Task ExpectToHaveValue(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        var options = new LocatorAssertionsToHaveValueOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToHaveValueAsync(expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected value: {Original} -> {Resolved}", expectation.Value, resolvedValue);
            await assertion.ToHaveValueAsync(resolvedValue, options);
        }
        else
        {
            throw new InvalidOperationException("Expected value or regex must be specified for value expectation");
        }
    }

    private async Task ExpectToHaveValues(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        var options = new LocatorAssertionsToHaveValuesOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Values != null)
        {
            var resolvedValues = context.ResolveDynamicValuesInArray(expectation.Values);
            Logger.LogDebug("Resolved expected values: {Count} values", resolvedValues.Length);
            await assertion.ToHaveValuesAsync(resolvedValues, options);
        }
        else
        {
            throw new InvalidOperationException("Expected values must be specified for values expectation");
        }
    }

    private async Task ExpectToHaveAttribute(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        if (string.IsNullOrEmpty(expectation.AttributeName))
        {
            throw new InvalidOperationException("Attribute name must be specified for attribute expectation");
        }

        var resolvedAttributeName = context.ReplaceDynamicContentInString(expectation.AttributeName);
        Logger.LogDebug("Resolved attribute name: {Original} -> {Resolved}", expectation.AttributeName,
            resolvedAttributeName);

        var options = new LocatorAssertionsToHaveAttributeOptions
        {
            Timeout = expectation.TimeoutMs,
            IgnoreCase = expectation.IgnoreCase
        };

        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToHaveAttributeAsync(resolvedAttributeName, expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected attribute value: {Original} -> {Resolved}", expectation.Value,
                resolvedValue);
            await assertion.ToHaveAttributeAsync(resolvedAttributeName, resolvedValue, options);
        }
    }

    private async Task ExpectToHaveCount(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation)
    {
        if (!expectation.Count.HasValue)
        {
            throw new InvalidOperationException("Count must be specified for count expectation");
        }

        var options = new LocatorAssertionsToHaveCountOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToHaveCountAsync(expectation.Count.Value, options);
    }

    private async Task ExpectToHaveClass(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        var options = new LocatorAssertionsToHaveClassOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToHaveClassAsync(expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected class value: {Original} -> {Resolved}", expectation.Value,
                resolvedValue);
            await assertion.ToHaveClassAsync(resolvedValue, options);
        }
        else if (expectation.Values != null)
        {
            var resolvedValues = context.ResolveDynamicValuesInArray(expectation.Values);
            Logger.LogDebug("Resolved expected class values: {Count} values", resolvedValues.Length);
            await assertion.ToHaveClassAsync(resolvedValues, options);
        }
        else
        {
            throw new InvalidOperationException(
                "Expected value, regex, or values must be specified for class expectation");
        }
    }

    private async Task ExpectToContainClass(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        var options = new LocatorAssertionsToContainClassOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected contain class value: {Original} -> {Resolved}", expectation.Value,
                resolvedValue);
            await assertion.ToContainClassAsync(resolvedValue, options);
        }
        else if (expectation.Values != null)
        {
            var resolvedValues = context.ResolveDynamicValuesInArray(expectation.Values);
            Logger.LogDebug("Resolved expected contain class values: {Count} values", resolvedValues.Length);
            await assertion.ToContainClassAsync(resolvedValues, options);
        }
        else
        {
            throw new InvalidOperationException(
                "Expected value or values must be specified for contain class expectation");
        }
    }

    private async Task ExpectToHaveCss(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        if (string.IsNullOrEmpty(expectation.AttributeName))
        {
            throw new InvalidOperationException("CSS property name must be specified for CSS expectation");
        }

        var resolvedPropertyName = context.ReplaceDynamicContentInString(expectation.AttributeName);
        Logger.LogDebug("Resolved CSS property name: {Original} -> {Resolved}", expectation.AttributeName,
            resolvedPropertyName);

        var options = new LocatorAssertionsToHaveCSSOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToHaveCSSAsync(resolvedPropertyName, expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected CSS value: {Original} -> {Resolved}", expectation.Value, resolvedValue);
            await assertion.ToHaveCSSAsync(resolvedPropertyName, resolvedValue, options);
        }
        else
        {
            throw new InvalidOperationException("Expected value or regex must be specified for CSS expectation");
        }
    }

    private async Task ExpectToHaveId(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        var options = new LocatorAssertionsToHaveIdOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToHaveIdAsync(expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected ID value: {Original} -> {Resolved}", expectation.Value, resolvedValue);
            await assertion.ToHaveIdAsync(resolvedValue, options);
        }
        else
        {
            throw new InvalidOperationException("Expected value or regex must be specified for ID expectation");
        }
    }

    private async Task ExpectToHaveJsProperty(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        if (string.IsNullOrEmpty(expectation.PropertyName))
        {
            throw new InvalidOperationException("Property name must be specified for JS property expectation");
        }

        var resolvedPropertyName = context.ReplaceDynamicContentInString(expectation.PropertyName);
        Logger.LogDebug("Resolved JS property name: {Original} -> {Resolved}", expectation.PropertyName,
            resolvedPropertyName);

        var options = new LocatorAssertionsToHaveJSPropertyOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected JS property value: {Original} -> {Resolved}", expectation.Value,
                resolvedValue);
            await assertion.ToHaveJSPropertyAsync(resolvedPropertyName, resolvedValue, options);
        }
        else
        {
            throw new InvalidOperationException("Expected value must be specified for JS property expectation");
        }
    }

    private async Task ExpectToHaveRole(ILocatorAssertions locatorAssertions, ExpectationDefinition expectation,
        TestContext context)
    {
        var options = new LocatorAssertionsToHaveRoleOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected role value: {Original} -> {Resolved}", expectation.Value, resolvedValue);

            if (Enum.TryParse<AriaRole>(resolvedValue, true, out var role))
            {
                await assertion.ToHaveRoleAsync(role, options);
            }
            else
            {
                throw new InvalidOperationException($"Invalid ARIA role: {resolvedValue}");
            }
        }
        else
        {
            throw new InvalidOperationException("Expected value must be specified for role expectation");
        }
    }

    private async Task ExpectToHaveAccessibleName(ILocatorAssertions locatorAssertions,
        ExpectationDefinition expectation, TestContext context)
    {
        var options = new LocatorAssertionsToHaveAccessibleNameOptions
        {
            Timeout = expectation.TimeoutMs,
            IgnoreCase = expectation.IgnoreCase
        };

        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToHaveAccessibleNameAsync(expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected accessible name: {Original} -> {Resolved}", expectation.Value,
                resolvedValue);
            await assertion.ToHaveAccessibleNameAsync(resolvedValue, options);
        }
        else
        {
            throw new InvalidOperationException(
                "Expected value or regex must be specified for accessible name expectation");
        }
    }

    private async Task ExpectToHaveAccessibleDescription(ILocatorAssertions locatorAssertions,
        ExpectationDefinition expectation, TestContext context)
    {
        var options = new LocatorAssertionsToHaveAccessibleDescriptionOptions
        {
            Timeout = expectation.TimeoutMs,
            IgnoreCase = expectation.IgnoreCase
        };

        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToHaveAccessibleDescriptionAsync(expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected accessible description: {Original} -> {Resolved}", expectation.Value,
                resolvedValue);
            await assertion.ToHaveAccessibleDescriptionAsync(resolvedValue, options);
        }
        else
        {
            throw new InvalidOperationException(
                "Expected value or regex must be specified for accessible description expectation");
        }
    }

    private async Task ExpectToHaveAccessibleErrorMessage(ILocatorAssertions locatorAssertions,
        ExpectationDefinition expectation, TestContext context)
    {
        var options = new LocatorAssertionsToHaveAccessibleErrorMessageOptions
        {
            Timeout = expectation.TimeoutMs,
            IgnoreCase = expectation.IgnoreCase
        };

        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;

        if (expectation.Regex != null)
        {
            await assertion.ToHaveAccessibleErrorMessageAsync(expectation.Regex, options);
        }
        else if (expectation.Value != null)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(expectation.Value);
            Logger.LogDebug("Resolved expected accessible error message: {Original} -> {Resolved}", expectation.Value,
                resolvedValue);
            await assertion.ToHaveAccessibleErrorMessageAsync(resolvedValue, options);
        }
        else
        {
            throw new InvalidOperationException(
                "Expected value or regex must be specified for accessible error message expectation");
        }
    }

    private async Task ExpectToMatchAriaSnapshot(ILocatorAssertions locatorAssertions,
        ExpectationDefinition expectation, TestContext context)
    {
        if (string.IsNullOrEmpty(expectation.AriaSnapshot))
        {
            throw new InvalidOperationException("ARIA snapshot must be specified for ARIA snapshot matching");
        }

        var resolvedSnapshot = context.ReplaceDynamicContentInString(expectation.AriaSnapshot);
        Logger.LogDebug("Resolved ARIA snapshot: {Original} -> {Resolved}", expectation.AriaSnapshot, resolvedSnapshot);

        var options = new LocatorAssertionsToMatchAriaSnapshotOptions { Timeout = expectation.TimeoutMs };
        var assertion = expectation.IsNot ? locatorAssertions.Not : locatorAssertions;
        await assertion.ToMatchAriaSnapshotAsync(resolvedSnapshot, options);
    }

    public class ExpectationDefinition
    {
        public ExpectedType Type { get; set; }
        public string? Value { get; set; }
        public Regex? Regex { get; set; }
        public string[]? Values { get; set; }
        public bool IsNot { get; set; }
        public int TimeoutMs { get; set; } = 5000;
        public bool IgnoreCase { get; set; }
        public int? Count { get; set; }
        public string? AttributeName { get; set; }
        public string? PropertyName { get; set; }
        public string? AriaSnapshot { get; set; }
    }

    public new class Builder : Builder<ExpectLocatorAction, Builder>
    {
        private readonly List<ExpectationDefinition> _expectations = [];
        private ExpectationDefinition? _currentExpectation;
        internal bool ContinueOnFailure { get; private set; }

        /// <summary>
        ///     Sets the expectation to check if the locator is visible.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeVisible()
        {
            AddExpectation(ExpectedType.TO_BE_VISIBLE);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator is hidden.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeHidden()
        {
            AddExpectation(ExpectedType.TO_BE_HIDDEN);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator is enabled.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeEnabled()
        {
            AddExpectation(ExpectedType.TO_BE_ENABLED);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator is disabled.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeDisabled()
        {
            AddExpectation(ExpectedType.TO_BE_DISABLED);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator is checked.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeChecked()
        {
            AddExpectation(ExpectedType.TO_BE_CHECKED);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator is focused.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeFocused()
        {
            AddExpectation(ExpectedType.TO_BE_FOCUSED);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator is editable.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeEditable()
        {
            AddExpectation(ExpectedType.TO_BE_EDITABLE);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator is attached to the DOM.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeAttached()
        {
            AddExpectation(ExpectedType.TO_BE_ATTACHED);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator is empty.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeEmpty()
        {
            AddExpectation(ExpectedType.TO_BE_EMPTY);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator is in viewport.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToBeInViewport()
        {
            AddExpectation(ExpectedType.TO_BE_IN_VIEWPORT);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the locator's text content.
        /// </summary>
        /// <param name="expectedText">The expected text value (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveText(string expectedText)
        {
            AddExpectation(ExpectedType.TO_HAVE_TEXT);
            _currentExpectation!.Value = expectedText ?? throw new ArgumentNullException(nameof(expectedText));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the locator's text content using a regex pattern.
        /// </summary>
        /// <param name="textRegex">The regex patterns to match against the text</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveText(Regex textRegex)
        {
            AddExpectation(ExpectedType.TO_HAVE_TEXT);
            _currentExpectation!.Regex = textRegex ?? throw new ArgumentNullException(nameof(textRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the locator's text content against multiple values.
        /// </summary>
        /// <param name="expectedTexts">The expected text values (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveText(string[] expectedTexts)
        {
            AddExpectation(ExpectedType.TO_HAVE_TEXT);
            _currentExpectation!.Values = expectedTexts ?? throw new ArgumentNullException(nameof(expectedTexts));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator contains specific text.
        /// </summary>
        /// <param name="expectedText">The expected text value (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToContainText(string expectedText)
        {
            AddExpectation(ExpectedType.TO_CONTAIN_TEXT);
            _currentExpectation!.Value = expectedText ?? throw new ArgumentNullException(nameof(expectedText));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator contains text matching a regex pattern.
        /// </summary>
        /// <param name="textRegex">The regex patterns to match against the text</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToContainText(Regex textRegex)
        {
            AddExpectation(ExpectedType.TO_CONTAIN_TEXT);
            _currentExpectation!.Regex = textRegex ?? throw new ArgumentNullException(nameof(textRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator contains any of the specified texts.
        /// </summary>
        /// <param name="expectedTexts">The expected text values (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToContainText(string[] expectedTexts)
        {
            AddExpectation(ExpectedType.TO_CONTAIN_TEXT);
            _currentExpectation!.Values = expectedTexts ?? throw new ArgumentNullException(nameof(expectedTexts));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the locator's value.
        /// </summary>
        /// <param name="expectedValue">The expected value (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveValue(string expectedValue)
        {
            AddExpectation(ExpectedType.TO_HAVE_VALUE);
            _currentExpectation!.Value = expectedValue ?? throw new ArgumentNullException(nameof(expectedValue));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the locator's values (for multi-select elements).
        /// </summary>
        /// <param name="expectedValues">The expected values (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveValue(Regex valueRegex)
        {
            AddExpectation(ExpectedType.TO_HAVE_VALUE);
            _currentExpectation!.Regex = valueRegex ?? throw new ArgumentNullException(nameof(valueRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the locator's value using a regex pattern.
        /// </summary>
        /// <param name="valueRegex">The regex pattern to match against the value</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveValues(string[] expectedValues)
        {
            AddExpectation(ExpectedType.TO_HAVE_VALUES);
            _currentExpectation!.Values = expectedValues ?? throw new ArgumentNullException(nameof(expectedValues));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name (supports dynamic values)</param>
        /// <param name="expectedValue">The expected attribute value (supports dynamic values, optional)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveAttribute(string attributeName, string? expectedValue = null)
        {
            AddExpectation(ExpectedType.TO_HAVE_ATTRIBUTE);
            _currentExpectation!.AttributeName =
                attributeName ?? throw new ArgumentNullException(nameof(attributeName));
            _currentExpectation!.Value = expectedValue;
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific attribute with a regex pattern.
        /// </summary>
        /// <param name="attributeName">The attribute name (supports dynamic values)</param>
        /// <param name="valueRegex">The regex pattern to match against the attribute value</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveAttribute(string attributeName, Regex valueRegex)
        {
            AddExpectation(ExpectedType.TO_HAVE_ATTRIBUTE);
            _currentExpectation!.AttributeName =
                attributeName ?? throw new ArgumentNullException(nameof(attributeName));
            _currentExpectation!.Regex = valueRegex ?? throw new ArgumentNullException(nameof(valueRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check the count of matching locators.
        /// </summary>
        /// <param name="expectedCount">The expected count</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveCount(int expectedCount)
        {
            AddExpectation(ExpectedType.TO_HAVE_COUNT);
            _currentExpectation!.Count = expectedCount;
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific class.
        /// </summary>
        /// <param name="expectedClass">The expected class name (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveClass(string expectedClass)
        {
            AddExpectation(ExpectedType.TO_HAVE_CLASS);
            _currentExpectation!.Value = expectedClass ?? throw new ArgumentNullException(nameof(expectedClass));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a class matching a regex pattern.
        /// </summary>
        /// <param name="classRegex">The regex pattern to match against the class</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveClass(Regex classRegex)
        {
            AddExpectation(ExpectedType.TO_HAVE_CLASS);
            _currentExpectation!.Regex = classRegex ?? throw new ArgumentNullException(nameof(classRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has any of the specified classes.
        /// </summary>
        /// <param name="expectedClasses">The expected class names (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveClass(string[] expectedClasses)
        {
            AddExpectation(ExpectedType.TO_HAVE_CLASS);
            _currentExpectation!.Values = expectedClasses ?? throw new ArgumentNullException(nameof(expectedClasses));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific CSS property.
        /// </summary>
        /// <param name="propertyName">The CSS property name (supports dynamic values)</param>
        /// <param name="expectedValue">The expected property value (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToContainClass(string expectedClass)
        {
            AddExpectation(ExpectedType.TO_CONTAIN_CLASS);
            _currentExpectation!.Value = expectedClass ?? throw new ArgumentNullException(nameof(expectedClass));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator contains a specific class.
        /// </summary>
        /// <param name="expectedClass">The expected class name (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToContainClass(string[] expectedClasses)
        {
            AddExpectation(ExpectedType.TO_CONTAIN_CLASS);
            _currentExpectation!.Values = expectedClasses ?? throw new ArgumentNullException(nameof(expectedClasses));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator contains a class matching a regex pattern.
        /// </summary>
        /// <param name="classRegex">The regex pattern to match against the class</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveCss(string propertyName, string expectedValue)
        {
            AddExpectation(ExpectedType.TO_HAVE_CSS);
            _currentExpectation!.AttributeName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _currentExpectation!.Value = expectedValue ?? throw new ArgumentNullException(nameof(expectedValue));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator contains any of the specified classes.
        /// </summary>
        /// <param name="expectedClasses">The expected class names (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveCss(string propertyName, Regex valueRegex)
        {
            AddExpectation(ExpectedType.TO_HAVE_CSS);
            _currentExpectation!.AttributeName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _currentExpectation!.Regex = valueRegex ?? throw new ArgumentNullException(nameof(valueRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific ID.
        /// </summary>
        /// <param name="expectedId">The expected ID value (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveId(string expectedId)
        {
            AddExpectation(ExpectedType.TO_HAVE_ID);
            _currentExpectation!.Value = expectedId ?? throw new ArgumentNullException(nameof(expectedId));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has an ID matching a regex pattern.
        /// </summary>
        /// <param name="idRegex">The regex pattern to match against the ID</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveId(Regex idRegex)
        {
            AddExpectation(ExpectedType.TO_HAVE_ID);
            _currentExpectation!.Regex = idRegex ?? throw new ArgumentNullException(nameof(idRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific JavaScript property.
        /// </summary>
        /// <param name="propertyName">The JavaScript property name (supports dynamic values)</param>
        /// <param name="expectedValue">The expected property value (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveJsProperty(string propertyName, string expectedValue)
        {
            AddExpectation(ExpectedType.TO_HAVE_JS_PROPERTY);
            _currentExpectation!.PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _currentExpectation!.Value = expectedValue ?? throw new ArgumentNullException(nameof(expectedValue));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific ARIA role.
        /// </summary>
        /// <param name="expectedRole">The expected ARIA role (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveRole(AriaRole expectedRole)
        {
            AddExpectation(ExpectedType.TO_HAVE_ROLE);
            _currentExpectation!.Value = nameof(expectedRole);
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific accessible name.
        /// </summary>
        /// <param name="expectedName">The expected accessible name (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveAccessibleName(string expectedName)
        {
            AddExpectation(ExpectedType.TO_HAVE_ACCESSIBLE_NAME);
            _currentExpectation!.Value = expectedName ?? throw new ArgumentNullException(nameof(expectedName));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has an accessible name matching a regex pattern.
        /// </summary>
        /// <param name="nameRegex">The regex pattern to match against the accessible name</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveAccessibleName(Regex nameRegex)
        {
            AddExpectation(ExpectedType.TO_HAVE_ACCESSIBLE_NAME);
            _currentExpectation!.Regex = nameRegex ?? throw new ArgumentNullException(nameof(nameRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific accessible description.
        /// </summary>
        /// <param name="expectedDescription">The expected accessible description (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveAccessibleDescription(string expectedDescription)
        {
            AddExpectation(ExpectedType.TO_HAVE_ACCESSIBLE_DESCRIPTION);
            _currentExpectation!.Value =
                expectedDescription ?? throw new ArgumentNullException(nameof(expectedDescription));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has an accessible description matching a regex pattern.
        /// </summary>
        /// <param name="descriptionRegex">The regex pattern to match against the accessible description</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveAccessibleDescription(Regex descriptionRegex)
        {
            AddExpectation(ExpectedType.TO_HAVE_ACCESSIBLE_DESCRIPTION);
            _currentExpectation!.Regex = descriptionRegex ?? throw new ArgumentNullException(nameof(descriptionRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has a specific accessible error message.
        /// </summary>
        /// <param name="expectedErrorMessage">The expected accessible error message (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveAccessibleErrorMessage(string expectedErrorMessage)
        {
            AddExpectation(ExpectedType.TO_HAVE_ACCESSIBLE_ERROR_MESSAGE);
            _currentExpectation!.Value =
                expectedErrorMessage ?? throw new ArgumentNullException(nameof(expectedErrorMessage));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator has an accessible error message matching a regex pattern.
        /// </summary>
        /// <param name="errorMessageRegex">The regex pattern to match against the accessible error message</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToHaveAccessibleErrorMessage(Regex errorMessageRegex)
        {
            AddExpectation(ExpectedType.TO_HAVE_ACCESSIBLE_ERROR_MESSAGE);
            _currentExpectation!.Regex =
                errorMessageRegex ?? throw new ArgumentNullException(nameof(errorMessageRegex));
            return this;
        }

        /// <summary>
        ///     Sets the expectation to check if the locator matches a specific ARIA snapshot.
        /// </summary>
        /// <param name="ariaSnapshot">The ARIA snapshot to match against (supports dynamic values)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ToMatchAriaSnapshot(string ariaSnapshot)
        {
            AddExpectation(ExpectedType.TO_MATCH_ARIA_SNAPSHOT);
            _currentExpectation!.AriaSnapshot = ariaSnapshot ?? throw new ArgumentNullException(nameof(ariaSnapshot));
            return this;
        }

        public Builder Not()
        {
            if (_currentExpectation != null)
            {
                _currentExpectation.IsNot = true;
            }

            return this;
        }

        public Builder And()
        {
            // Just return this for fluent chaining - no operation needed
            return this;
        }

        public Builder WithTimeout(int timeoutMs)
        {
            if (_currentExpectation != null)
            {
                _currentExpectation.TimeoutMs = timeoutMs;
            }

            return this;
        }

        public Builder IgnoreCase(bool ignoreCase = true)
        {
            if (_currentExpectation != null)
            {
                _currentExpectation.IgnoreCase = ignoreCase;
            }

            return this;
        }

        public Builder WithContinueOnFailure()
        {
            ContinueOnFailure = true;
            return this;
        }

        private void AddExpectation(ExpectedType type)
        {
            _currentExpectation = new ExpectationDefinition { Type = type };
            _expectations.Add(_currentExpectation);
        }

        internal List<ExpectationDefinition> GetExpectations()
        {
            return _expectations;
        }

        /// <summary>
        ///     Builds the ExpectLocatorAction instance.
        /// </summary>
        /// <returns>A new ExpectLocatorAction instance</returns>
        public override ExpectLocatorAction Build()
        {
            return new ExpectLocatorAction(this);
        }
    }
}
