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
///     Playwright action for selecting options from dropdown/select elements.
///     Supports selection by value, label/text, index, and multiple selections.
///     Uses Playwright's built-in SelectOptionAsync method for reliable dropdown handling.
/// </summary>
public class DropdownSelectAction : LocatingElementAction
{
    /// <summary>
    ///     Enumeration of selection types for dropdown options.
    /// </summary>
    public enum SelectionType
    {
        /// <summary>
        ///     Select by option value attribute.
        /// </summary>
        VALUE,

        /// <summary>
        ///     Select by option text/label.
        /// </summary>
        LABEL,

        /// <summary>
        ///     Select by option index (0-based).
        /// </summary>
        INDEX
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(DropdownSelectAction));
    private readonly bool _force;

    private readonly SelectionType _selectionType;
    private readonly List<string> _selectionValues;
    private readonly int? _timeout;

    /// <summary>
    ///     Initializes a new instance of the DropdownSelectAction class with the specified builder configuration.
    /// </summary>
    /// <param name="builder">The builder containing the configuration for dropdown selection.</param>
    public DropdownSelectAction(Builder builder) : this("dropdown-select", builder)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the DropdownSelectAction class with the specified builder configuration.
    /// </summary>
    /// <param name="name">The name identifier for the dropdown select action.</param>
    /// <param name="builder">The builder containing the configuration for dropdown selection.</param>
    public DropdownSelectAction(string name, Builder builder) : base(name, builder)
    {
        _selectionType = builder.SelectionType;
        _selectionValues = builder.SelectionValues.ToList();
        _force = builder.Force;
        _timeout = builder.Timeout;

        if (_selectionValues.Count == 0)
        {
            throw new ArgumentException("At least one selection value must be provided", nameof(builder));
        }
    }

    /// <summary>
    ///     Executes the dropdown selection action after locating the element.
    /// </summary>
    /// <param name="locator">The located dropdown element</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing dropdown select action, type: {SelectionType}, values: {Values}",
                _selectionType, string.Join(", ", _selectionValues));

            // Create options for the select operation
            var options = new LocatorSelectOptionOptions { Force = _force };

            if (_timeout.HasValue)
            {
                options.Timeout = _timeout.Value;
            }

            // Perform selection based on type
            var selectedValues = await PerformSelection(locator, options);

            Logger.LogInformation("Dropdown selection completed successfully. Selected values: {SelectedValues}",
                string.Join(", ", selectedValues));

            // Store selected values in context for potential verification
            context.SetVariable("LAST_SELECTED_VALUES", selectedValues);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute dropdown select action");
            throw new AgenixSystemException("Failed to execute dropdown select action", ex);
        }
    }

    /// <summary>
    ///     Performs the selection based on the selection type.
    /// </summary>
    /// <param name="locator">The dropdown locator</param>
    /// <param name="options">The selection options</param>
    /// <returns>Array of selected values</returns>
    private async Task<IReadOnlyList<string>> PerformSelection(ILocator locator, LocatorSelectOptionOptions options)
    {
        return _selectionType switch
        {
            SelectionType.VALUE => await locator.SelectOptionAsync(_selectionValues.ToArray(), options),
            SelectionType.LABEL => await SelectByLabel(locator, options),
            SelectionType.INDEX => await SelectByIndex(locator, options),
            _ => throw new ArgumentException($"Unsupported selection type: {_selectionType}")
        };
    }

    /// <summary>
    ///     Selects options by their label/text.
    /// </summary>
    /// <param name="locator">The dropdown locator</param>
    /// <param name="options">The selection options</param>
    /// <returns>Array of selected values</returns>
    private async Task<IReadOnlyList<string>> SelectByLabel(ILocator locator, LocatorSelectOptionOptions options)
    {
        Logger.LogDebug("Selecting by label: {Labels}", string.Join(", ", _selectionValues));

        var selectOptions = _selectionValues.Select(label => new SelectOptionValue { Label = label }).ToArray();
        return await locator.SelectOptionAsync(selectOptions, options);
    }

    /// <summary>
    ///     Selects options by their index.
    /// </summary>
    /// <param name="locator">The dropdown locator</param>
    /// <param name="options">The selection options</param>
    /// <returns>Array of selected values</returns>
    private async Task<IReadOnlyList<string>> SelectByIndex(ILocator locator, LocatorSelectOptionOptions options)
    {
        Logger.LogDebug("Selecting by index: {Indices}", string.Join(", ", _selectionValues));

        var selectOptions = new List<SelectOptionValue>();

        foreach (var indexStr in _selectionValues)
        {
            if (!int.TryParse(indexStr, out var index))
            {
                throw new ArgumentException($"Invalid index value: {indexStr}. Index must be a number.");
            }

            if (index < 0)
            {
                throw new ArgumentException($"Index cannot be negative: {index}");
            }

            selectOptions.Add(new SelectOptionValue { Index = index });
        }

        return await locator.SelectOptionAsync(selectOptions.ToArray(), options);
    }

    /// <summary>
    ///     Builder class for creating DropdownSelectAction instances with fluent API.
    /// </summary>
    public new class Builder : LocatingElementAction.Builder
    {
        internal SelectionType SelectionType { get; private set; } = SelectionType.VALUE;
        internal List<string> SelectionValues { get; private set; } = [];
        internal bool Force { get; private set; }
        internal int? Timeout { get; private set; }

        /// <summary>
        ///     Sets the selection to be by option value.
        /// </summary>
        /// <param name="values">The option values to select</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SelectByValue(params string[] values)
        {
            SelectionType = SelectionType.VALUE;
            SelectionValues = values.ToList();
            return this;
        }

        /// <summary>
        ///     Sets the selection to be by option value.
        /// </summary>
        /// <param name="values">The option values to select</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SelectByValue(IEnumerable<string> values)
        {
            SelectionType = SelectionType.VALUE;
            SelectionValues = values.ToList();
            return this;
        }

        /// <summary>
        ///     Sets the selection to be by option label/text.
        /// </summary>
        /// <param name="labels">The option labels to select</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SelectByLabel(params string[] labels)
        {
            SelectionType = SelectionType.LABEL;
            SelectionValues = labels.ToList();
            return this;
        }

        /// <summary>
        ///     Sets the selection to be by option label/text.
        /// </summary>
        /// <param name="labels">The option labels to select</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SelectByLabel(IEnumerable<string> labels)
        {
            SelectionType = SelectionType.LABEL;
            SelectionValues = labels.ToList();
            return this;
        }

        /// <summary>
        ///     Sets the selection to be by option index (0-based).
        /// </summary>
        /// <param name="indices">The option indices to select</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SelectByIndex(params int[] indices)
        {
            SelectionType = SelectionType.INDEX;
            SelectionValues = indices.Select(i => i.ToString()).ToList();
            return this;
        }

        /// <summary>
        ///     Sets the selection to be by option index (0-based).
        /// </summary>
        /// <param name="indices">The option indices to select</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SelectByIndex(IEnumerable<int> indices)
        {
            SelectionType = SelectionType.INDEX;
            SelectionValues = indices.Select(i => i.ToString()).ToList();
            return this;
        }

        /// <summary>
        ///     Adds a value to select (for multiple selections).
        /// </summary>
        /// <param name="value">The additional value to select</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder AddValue(string value)
        {
            SelectionValues.Add(value);
            return this;
        }

        /// <summary>
        ///     Adds an index to select (for multiple selections by index).
        /// </summary>
        /// <param name="index">The additional index to select</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder AddIndex(int index)
        {
            if (SelectionType != SelectionType.INDEX)
            {
                throw new InvalidOperationException("Cannot add index when selection type is not INDEX");
            }

            SelectionValues.Add(index.ToString());
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
        ///     Builds the DropdownSelectAction instance.
        /// </summary>
        /// <returns>A new DropdownSelectAction instance</returns>
        public override DropdownSelectAction Build()
        {
            return new DropdownSelectAction(this);
        }
    }
}
