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
///     Playwright action for working with frame locators to locate elements within iframes using page.FrameLocator() API.
///     Supports various locator strategies and chaining operations within iframe contexts.
/// </summary>
public class FrameLocatorAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of frame locator operations.
    /// </summary>
    public enum FrameLocatorOperation
    {
        /// <summary>
        ///     Create a locator using frameLocator.Locator().
        /// </summary>
        LOCATOR,

        /// <summary>
        ///     Create a locator using frameLocator.GetByRole().
        /// </summary>
        GET_BY_ROLE,

        /// <summary>
        ///     Create a locator using frameLocator.GetByText().
        /// </summary>
        GET_BY_TEXT,

        /// <summary>
        ///     Create a locator using frameLocator.GetByLabel().
        /// </summary>
        GET_BY_LABEL,

        /// <summary>
        ///     Create a locator using frameLocator.GetByPlaceholder().
        /// </summary>
        GET_BY_PLACEHOLDER,

        /// <summary>
        ///     Create a locator using frameLocator.GetByTitle().
        /// </summary>
        GET_BY_TITLE,

        /// <summary>
        ///     Create a locator using frameLocator.GetByAltText().
        /// </summary>
        GET_BY_ALT_TEXT,

        /// <summary>
        ///     Create a locator using frameLocator.GetByTestId().
        /// </summary>
        GET_BY_TEST_ID,

        /// <summary>
        ///     Create a first locator using frameLocator.Locator().First.
        /// </summary>
        FIRST,

        /// <summary>
        ///     Create a last locator using frameLocator.Locator().Last.
        /// </summary>
        LAST,

        /// <summary>
        ///     Create a nth locator using frameLocator.Locator().Nth().
        /// </summary>
        NTH
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(FrameLocatorAction));
    private readonly string? _altText;
    private readonly bool _exact;

    private readonly string _frameSelector;
    private readonly int? _index;
    private readonly string? _label;
    private readonly FrameLocatorOperation _operation;
    private readonly string? _placeholder;
    private readonly AriaRole? _role;
    private readonly string? _selector;
    private readonly string? _testId;
    private readonly string? _text;
    private readonly string? _title;

    /// <summary>
    ///     Represents a Playwright action for working with frame locators to locate elements within iframes.
    /// </summary>
    public FrameLocatorAction(Builder builder) : this("frameLocator", builder)
    {
    }

    /// <summary>
    ///     Playwright action for working with frame locators to locate elements within iframes using page.FrameLocator() API.
    ///     Supports various locator strategies and chaining operations within iframe contexts.
    /// </summary>
    public FrameLocatorAction(string name, Builder builder) : base(name, builder)
    {
        _frameSelector = builder.FrameSelector ?? throw new ArgumentException("Frame selector is required");
        _operation = builder.Operation;
        _selector = builder.Selector;
        _text = builder.Text;
        _label = builder.Label;
        _placeholder = builder.Placeholder;
        _title = builder.Title;
        _altText = builder.AltText;
        _testId = builder.TestId;
        _role = builder.Role;
        _index = builder.Index;
        _exact = builder.Exact;
    }

    /// <summary>
    ///     Executes the frame locator action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing frame locator action: {Operation} in frame: {FrameSelector}", _operation,
                _frameSelector);

            var page = browser.GetCurrentPage();
            if (page == null)
            {
                throw new InvalidOperationException("No active page available");
            }

            var frameSelector = context.ReplaceDynamicContentInString(_frameSelector);
            var frameLocator = page.FrameLocator(frameSelector);

            await Task.Run(() => ExecuteOperation(frameLocator, context));

            Logger.LogInformation("Frame locator action completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute frame locator action");
            throw new AgenixSystemException("Failed to execute frame locator action", ex);
        }
    }

    /// <summary>
    ///     Executes the specific frame locator operation and returns the resulting locator.
    /// </summary>
    private ILocator ExecuteOperation(IFrameLocator frameLocator, TestContext context)
    {
        return _operation switch
        {
            FrameLocatorOperation.LOCATOR => ExecuteLocator(frameLocator, context),
            FrameLocatorOperation.GET_BY_ROLE => ExecuteGetByRole(frameLocator, context),
            FrameLocatorOperation.GET_BY_TEXT => ExecuteGetByText(frameLocator, context),
            FrameLocatorOperation.GET_BY_LABEL => ExecuteGetByLabel(frameLocator, context),
            FrameLocatorOperation.GET_BY_PLACEHOLDER => ExecuteGetByPlaceholder(frameLocator, context),
            FrameLocatorOperation.GET_BY_TITLE => ExecuteGetByTitle(frameLocator, context),
            FrameLocatorOperation.GET_BY_ALT_TEXT => ExecuteGetByAltText(frameLocator, context),
            FrameLocatorOperation.GET_BY_TEST_ID => ExecuteGetByTestId(frameLocator, context),
            FrameLocatorOperation.FIRST => ExecuteFirst(frameLocator, context),
            FrameLocatorOperation.LAST => ExecuteLast(frameLocator, context),
            FrameLocatorOperation.NTH => ExecuteNth(frameLocator, context),
            _ => throw new ArgumentException($"Unsupported frame locator operation: {_operation}")
        };
    }

    /// <summary>
    ///     Executes locator operation using frameLocator.Locator().
    /// </summary>
    private ILocator ExecuteLocator(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_selector))
        {
            throw new InvalidOperationException("Selector must be specified for Locator operation");
        }

        var selector = context.ReplaceDynamicContentInString(_selector);
        var locator = frameLocator.Locator(selector);

        Logger.LogDebug("Created locator with selector: {Selector}", selector);
        return locator;
    }

    /// <summary>
    ///     Executes GetByRole operation using frameLocator.GetByRole().
    /// </summary>
    private ILocator ExecuteGetByRole(IFrameLocator frameLocator, TestContext context)
    {
        if (_role == null)
        {
            throw new InvalidOperationException("Role must be specified for GetByRole operation");
        }

        var options = new FrameLocatorGetByRoleOptions();

        if (!string.IsNullOrEmpty(_text))
        {
            options.Name = context.ReplaceDynamicContentInString(_text);
            options.Exact = _exact;
        }

        var locator = frameLocator.GetByRole(_role.Value, options);

        Logger.LogDebug("Created GetByRole locator with role: {Role}, name: {Name}", _role, options.Name);
        return locator;
    }

    /// <summary>
    ///     Executes GetByText operation using frameLocator.GetByText().
    /// </summary>
    private ILocator ExecuteGetByText(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_text))
        {
            throw new InvalidOperationException("Text must be specified for GetByText operation");
        }

        var text = context.ReplaceDynamicContentInString(_text);
        var options = new FrameLocatorGetByTextOptions { Exact = _exact };
        var locator = frameLocator.GetByText(text, options);

        Logger.LogDebug("Created GetByText locator with text: {Text}, exact: {Exact}", text, _exact);
        return locator;
    }

    /// <summary>
    ///     Executes the GetByLabel operation using frameLocator.GetByLabel().
    /// </summary>
    private ILocator ExecuteGetByLabel(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_label))
        {
            throw new InvalidOperationException("Label must be specified for GetByLabel operation");
        }

        var label = context.ReplaceDynamicContentInString(_label);
        var options = new FrameLocatorGetByLabelOptions { Exact = _exact };
        var locator = frameLocator.GetByLabel(label, options);

        Logger.LogDebug("Created GetByLabel locator with label: {Label}, exact: {Exact}", label, _exact);
        return locator;
    }

    /// <summary>
    ///     Executes GetByPlaceholder operation using frameLocator.GetByPlaceholder().
    /// </summary>
    private ILocator ExecuteGetByPlaceholder(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_placeholder))
        {
            throw new InvalidOperationException("Placeholder must be specified for GetByPlaceholder operation");
        }

        var placeholder = context.ReplaceDynamicContentInString(_placeholder);
        var options = new FrameLocatorGetByPlaceholderOptions { Exact = _exact };
        var locator = frameLocator.GetByPlaceholder(placeholder, options);

        Logger.LogDebug("Created GetByPlaceholder locator with placeholder: {Placeholder}, exact: {Exact}", placeholder,
            _exact);
        return locator;
    }

    /// <summary>
    ///     Executes GetByTitle operation using frameLocator.GetByTitle().
    /// </summary>
    private ILocator ExecuteGetByTitle(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_title))
        {
            throw new InvalidOperationException("Title must be specified for GetByTitle operation");
        }

        var title = context.ReplaceDynamicContentInString(_title);
        var options = new FrameLocatorGetByTitleOptions { Exact = _exact };
        var locator = frameLocator.GetByTitle(title, options);

        Logger.LogDebug("Created GetByTitle locator with title: {Title}, exact: {Exact}", title, _exact);
        return locator;
    }

    /// <summary>
    ///     Executes GetByAltText operation using frameLocator.GetByAltText().
    /// </summary>
    private ILocator ExecuteGetByAltText(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_altText))
        {
            throw new InvalidOperationException("Alt text must be specified for GetByAltText operation");
        }

        var altText = context.ReplaceDynamicContentInString(_altText);
        var options = new FrameLocatorGetByAltTextOptions { Exact = _exact };
        var locator = frameLocator.GetByAltText(altText, options);

        Logger.LogDebug("Created GetByAltText locator with alt text: {AltText}, exact: {Exact}", altText, _exact);
        return locator;
    }

    /// <summary>
    ///     Executes GetByTestId operation using frameLocator.GetByTestId().
    /// </summary>
    private ILocator ExecuteGetByTestId(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_testId))
        {
            throw new InvalidOperationException("Test ID must be specified for GetByTestId operation");
        }

        var testId = context.ReplaceDynamicContentInString(_testId);
        var locator = frameLocator.GetByTestId(testId);

        Logger.LogDebug("Created GetByTestId locator with test ID: {TestId}", testId);
        return locator;
    }

    /// <summary>
    ///     Executes First operation using frameLocator.First().
    /// </summary>
    private ILocator ExecuteFirst(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_selector))
        {
            throw new InvalidOperationException("Selector must be specified for First operation");
        }

        var selector = context.ReplaceDynamicContentInString(_selector);
        var locator = frameLocator.Locator(selector).First;

        Logger.LogDebug("Created First locator with selector: {Selector}", selector);
        return locator;
    }

    /// <summary>
    ///     Executes Last operation using frameLocator.Last().
    /// </summary>
    private ILocator ExecuteLast(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_selector))
        {
            throw new InvalidOperationException("Selector must be specified for Last operation");
        }

        var selector = context.ReplaceDynamicContentInString(_selector);
        var locator = frameLocator.Locator(selector).Last;

        Logger.LogDebug("Created Last locator with selector: {Selector}", selector);
        return locator;
    }

    /// <summary>
    ///     Executes Nth operation using frameLocator.Nth().
    /// </summary>
    private ILocator ExecuteNth(IFrameLocator frameLocator, TestContext context)
    {
        if (string.IsNullOrEmpty(_selector))
        {
            throw new InvalidOperationException("Selector must be specified for Nth operation");
        }

        if (_index == null)
        {
            throw new InvalidOperationException("Index must be specified for Nth operation");
        }

        var selector = context.ReplaceDynamicContentInString(_selector);
        var locator = frameLocator.Locator(selector).Nth(_index.Value);

        Logger.LogDebug("Created Nth locator with selector: {Selector}, index: {Index}", selector, _index);
        return locator;
    }

    /// <summary>
    ///     Builder class for creating FrameLocatorAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<FrameLocatorAction, Builder>
    {
        internal string? FrameSelector { get; private set; }
        internal FrameLocatorOperation Operation { get; private set; }
        internal string? Selector { get; private set; }
        internal string? Text { get; private set; }
        internal string? Label { get; private set; }
        internal string? Placeholder { get; private set; }
        internal string? Title { get; private set; }
        internal string? AltText { get; private set; }
        internal string? TestId { get; private set; }
        internal AriaRole? Role { get; private set; }
        internal int? Index { get; private set; }
        internal bool Exact { get; private set; }

        /// <summary>
        ///     Sets the frame selector for the iframe.
        /// </summary>
        /// <param name="frameSelector">Selector for the iframe</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFrame(string frameSelector)
        {
            FrameSelector = frameSelector;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a locator using frameLocator.Locator().
        /// </summary>
        /// <param name="selector">Element selector</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Locator(string selector)
        {
            Operation = FrameLocatorOperation.LOCATOR;
            Selector = selector;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a locator using frameLocator.GetByRole().
        /// </summary>
        /// <param name="role">ARIA role</param>
        /// <param name="name">Accessible name (optional)</param>
        /// <param name="exact">Exact match for name (default: false)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GetByRole(AriaRole role, string? name = null, bool exact = false)
        {
            Operation = FrameLocatorOperation.GET_BY_ROLE;
            Role = role;
            Text = name;
            Exact = exact;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a locator using frameLocator.GetByText().
        /// </summary>
        /// <param name="text">Text content to locate</param>
        /// <param name="exact">Exact match (default: false)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GetByText(string text, bool exact = false)
        {
            Operation = FrameLocatorOperation.GET_BY_TEXT;
            Text = text;
            Exact = exact;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a locator using frameLocator.GetByLabel().
        /// </summary>
        /// <param name="label">Label text</param>
        /// <param name="exact">Exact match (default: false)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GetByLabel(string label, bool exact = false)
        {
            Operation = FrameLocatorOperation.GET_BY_LABEL;
            Label = label;
            Exact = exact;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a locator using frameLocator.GetByPlaceholder().
        /// </summary>
        /// <param name="placeholder">Placeholder text</param>
        /// <param name="exact">Exact match (default: false)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GetByPlaceholder(string placeholder, bool exact = false)
        {
            Operation = FrameLocatorOperation.GET_BY_PLACEHOLDER;
            Placeholder = placeholder;
            Exact = exact;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a locator using frameLocator.GetByTitle().
        /// </summary>
        /// <param name="title">Title attribute value</param>
        /// <param name="exact">Exact match (default: false)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GetByTitle(string title, bool exact = false)
        {
            Operation = FrameLocatorOperation.GET_BY_TITLE;
            Title = title;
            Exact = exact;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a locator using frameLocator.GetByAltText().
        /// </summary>
        /// <param name="altText">Alt attribute value</param>
        /// <param name="exact">Exact match (default: false)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GetByAltText(string altText, bool exact = false)
        {
            Operation = FrameLocatorOperation.GET_BY_ALT_TEXT;
            AltText = altText;
            Exact = exact;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a locator using frameLocator.GetByTestId().
        /// </summary>
        /// <param name="testId">Test ID attribute value</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GetByTestId(string testId)
        {
            Operation = FrameLocatorOperation.GET_BY_TEST_ID;
            TestId = testId;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a first locator using frameLocator.Locator().First.
        /// </summary>
        /// <param name="selector">Element selector</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder First(string selector)
        {
            Operation = FrameLocatorOperation.FIRST;
            Selector = selector;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a last locator using frameLocator.Locator().Last.
        /// </summary>
        /// <param name="selector">Element selector</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Last(string selector)
        {
            Operation = FrameLocatorOperation.LAST;
            Selector = selector;
            return this;
        }

        /// <summary>
        ///     Configures the action to create a nth locator using frameLocator.Locator().Nth().
        /// </summary>
        /// <param name="selector">Element selector</param>
        /// <param name="index">Zero-based index</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Nth(string selector, int index)
        {
            Operation = FrameLocatorOperation.NTH;
            Selector = selector;
            Index = index;
            return this;
        }

        /// <summary>
        ///     Builds the FrameLocatorAction instance.
        /// </summary>
        /// <returns>A new FrameLocatorAction instance</returns>
        public override FrameLocatorAction Build()
        {
            return new FrameLocatorAction(this);
        }
    }
}
