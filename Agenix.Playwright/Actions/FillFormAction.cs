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
///     Playwright action for filling out forms with multiple field values.
///     This action delegates to existing actions like SetInputAction, CheckInputAction, and SetInputFilesAction
///     for reliable and consistent form field handling. Optionally submits the form after filling.
/// </summary>
public class FillFormAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of form field types.
    /// </summary>
    public enum FormFieldType
    {
        /// <summary>
        ///     Automatically determine a field type.
        /// </summary>
        AUTO,

        /// <summary>
        ///     Text input field.
        /// </summary>
        TEXT,

        /// <summary>
        ///     Checkbox input field.
        /// </summary>
        CHECKBOX,

        /// <summary>
        ///     Radio button input field.
        /// </summary>
        RADIO,

        /// <summary>
        ///     File input field.
        /// </summary>
        FILE,

        /// <summary>
        ///     Select the dropdown field.
        /// </summary>
        SELECT
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(FillFormAction));

    private readonly Dictionary<string, FormFieldConfig> _fieldConfigs;
    private readonly bool _force;
    private readonly FormFieldConfig? _submitButtonConfig;
    private readonly int? _timeout;

    /// <summary>
    ///     Initializes a new instance of the FillFormAction class with the specified builder configuration.
    /// </summary>
    /// <param name="builder">The builder containing the configuration for filling form fields</param>
    public FillFormAction(Builder builder) : base("fill-form", builder)
    {
        _fieldConfigs = new Dictionary<string, FormFieldConfig>(builder.FieldConfigs);
        _submitButtonConfig = builder.SubmitButtonConfig;
        _force = builder.Force;
        _timeout = builder.Timeout;

        if (_fieldConfigs.Count == 0)
        {
            throw new ArgumentException("At least one field must be configured", nameof(builder));
        }
    }

    /// <summary>
    ///     Executes the fill form action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing fill form action with {FieldCount} fields", _fieldConfigs.Count);

            var page = browser.GetCurrentPage();
            if (page == null)
            {
                throw new InvalidOperationException("No active page available");
            }

            var successCount = 0;
            var errorCount = 0;
            var errors = new List<string>();

            // Fill all form fields
            foreach (var fieldConfig in _fieldConfigs)
            {
                try
                {
                    await FillField(fieldConfig.Key, fieldConfig.Value, browser, context);
                    successCount++;
                    Logger.LogDebug("Successfully filled field: {FieldName}", fieldConfig.Key);
                }
                catch (Exception ex)
                {
                    errorCount++;
                    var errorMessage = $"Field '{fieldConfig.Key}': {ex.Message}";
                    errors.Add(errorMessage);
                    Logger.LogError(ex, "Failed to fill field: {FieldName}", fieldConfig.Key);
                }
            }

            Logger.LogInformation("Fill form action completed. Success: {SuccessCount}, Errors: {ErrorCount}",
                successCount, errorCount);

            if (errorCount > 0)
            {
                var combinedErrors = string.Join("; ", errors);
                throw new AgenixSystemException(
                    $"Failed to fill {errorCount} out of {_fieldConfigs.Count} form fields. Errors: {combinedErrors}");
            }

            // Submit the form if the submitted button is configured
            if (_submitButtonConfig != null)
            {
                await SubmitForm(browser, context);
            }
        }
        catch (Exception ex) when (ex is not AgenixSystemException)
        {
            Logger.LogError(ex, "Failed to execute fill form action");
            throw new AgenixSystemException("Failed to execute fill form action", ex);
        }
    }

    /// <summary>
    ///     Submits the form by clicking the submitted button.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    private async Task SubmitForm(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Submitting form using configured submit button");

            var clickActionBuilder = new ClickAction.Builder().WithForce(_force);

            if (_timeout.HasValue)
            {
                clickActionBuilder.WithTimeout(_timeout.Value);
            }

            if (_submitButtonConfig != null)
            {
                ConfigureFieldBuilder(ref clickActionBuilder, _submitButtonConfig);
            }

            var clickAction = clickActionBuilder.WithBrowser(browser).Build();
            await clickAction.ExecuteAsync(context);

            Logger.LogInformation("Form submitted successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to submit form");
            throw new AgenixSystemException("Failed to submit form", ex);
        }
    }

    /// <summary>
    ///     Fills a single form field using the appropriate existing action.
    /// </summary>
    /// <param name="fieldKey">The field configuration key</param>
    /// <param name="fieldConfig">The field configuration</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    private async Task FillField(string fieldKey, FormFieldConfig fieldConfig, PlaywrightBrowser browser,
        TestContext context)
    {
        // Replace dynamic content in string values
        var processedValue = fieldConfig.Value is string stringValue
            ? context.ReplaceDynamicContentInString(stringValue)
            : fieldConfig.Value;

        Logger.LogDebug("Filling field {FieldKey} with value: {Value}", fieldKey, processedValue);

        // Determine a field type if not explicitly set
        var fieldType = fieldConfig.FieldType;
        if (fieldType == FormFieldType.AUTO)
        {
            fieldType = await DetermineFieldType(fieldConfig, browser);
        }

        // Handle field based on type
        switch (fieldType)
        {
            case FormFieldType.CHECKBOX:
            case FormFieldType.RADIO:
                await HandleCheckboxOrRadio(fieldConfig, processedValue, browser, context);
                break;
            case FormFieldType.FILE:
                if (processedValue != null)
                {
                    await HandleFileInput(fieldConfig, processedValue, context);
                }

                break;
            case FormFieldType.SELECT:
                if (processedValue != null)
                {
                    await HandleSelect(fieldConfig, processedValue, browser, context);
                }

                break;
            case FormFieldType.TEXT:
                if (processedValue != null)
                {
                    await HandleTextInput(fieldConfig, processedValue, browser, context);
                }

                break;
        }
    }

    /// <summary>
    ///     Determines the field type automatically by inspecting the element.
    /// </summary>
    /// <param name="fieldConfig">The field configuration</param>
    /// <param name="browser">The browser instance</param>
    /// <returns>The determined field type</returns>
    private static async Task<FormFieldType> DetermineFieldType(FormFieldConfig fieldConfig, PlaywrightBrowser browser)
    {
        try
        {
            var page = browser.GetCurrentPage();
            if (page != null)
            {
                var locator = CreateLocatorFromConfig(page, fieldConfig);
                var element = locator.First;

                var inputType = await element.GetAttributeAsync("type");
                var tagName = await element.EvaluateAsync<string>("el => el.tagName.toLowerCase()");

                return (inputType?.ToLower(), tagName.ToLower()) switch
                {
                    ("checkbox", _) => FormFieldType.CHECKBOX,
                    ("radio", _) => FormFieldType.RADIO,
                    ("file", _) => FormFieldType.FILE,
                    (_, "select") => FormFieldType.SELECT,
                    _ => FormFieldType.TEXT
                };
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to determine field type, defaulting to Text");
            return FormFieldType.TEXT;
        }

        return FormFieldType.AUTO;
    }

    /// <summary>
    ///     Creates a locator from the field configuration.
    /// </summary>
    /// <param name="page">The page instance</param>
    /// <param name="fieldConfig">The field configuration</param>
    /// <returns>The locator for the field</returns>
    private static ILocator CreateLocatorFromConfig(IPage page, FormFieldConfig fieldConfig)
    {
        // Use the same logic as LocatingElementAction would use
        if (!string.IsNullOrEmpty(fieldConfig.Id))
        {
            return page.Locator($"#{fieldConfig.Id}");
        }

        if (!string.IsNullOrEmpty(fieldConfig.Name))
        {
            return page.Locator($"[name='{fieldConfig.Name}']");
        }

        if (!string.IsNullOrEmpty(fieldConfig.Css))
        {
            return page.Locator(fieldConfig.Css);
        }

        if (!string.IsNullOrEmpty(fieldConfig.XPath))
        {
            return page.Locator($"xpath={fieldConfig.XPath}");
        }

        if (!string.IsNullOrEmpty(fieldConfig.Text))
        {
            return page.GetByText(fieldConfig.Text);
        }

        return !string.IsNullOrEmpty(fieldConfig.Role) ? page.GetByRole(Enum.Parse<AriaRole>(fieldConfig.Role, true)) : throw new InvalidOperationException("No valid locator strategy found in field configuration");
    }

    /// <summary>
    ///     Configures a LocatingElementAction builder with the field configuration.
    /// </summary>
    /// <typeparam name="T">The builder type</typeparam>
    /// <param name="builder">The builder instance</param>
    /// <param name="fieldConfig">The field configuration</param>
    /// <returns>The configured builder</returns>
    private static void ConfigureFieldBuilder<T>(ref T builder, FormFieldConfig fieldConfig)
        where T : LocatingElementAction.Builder
    {
        if (!string.IsNullOrEmpty(fieldConfig.Id))
        {
            builder.WithId(fieldConfig.Id);
        }
        else if (!string.IsNullOrEmpty(fieldConfig.Name))
        {
            builder.WithName(fieldConfig.Name);
        }
        else if (!string.IsNullOrEmpty(fieldConfig.Css))
        {
            builder.WithCss(fieldConfig.Css);
        }
        else if (!string.IsNullOrEmpty(fieldConfig.XPath))
        {
            builder.WithXPath(fieldConfig.XPath);
        }
        else if (!string.IsNullOrEmpty(fieldConfig.Text))
        {
            builder.WithText(fieldConfig.Text);
        }
        else if (!string.IsNullOrEmpty(fieldConfig.Role))
        {
            builder.WithRole(Enum.Parse<AriaRole>(fieldConfig.Role, true));
        }
        else
        {
            throw new InvalidOperationException("No valid locator strategy found in field configuration");
        }
    }

    /// <summary>
    ///     Handles checkbox and radio button inputs using CheckInputAction.
    /// </summary>
    /// <param name="fieldConfig">The field configuration</param>
    /// <param name="value">The value (boolean or string)</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    private async Task HandleCheckboxOrRadio(FormFieldConfig fieldConfig, object? value, PlaywrightBrowser browser,
        TestContext context)
    {
        var shouldCheck = value switch
        {
            bool boolValue => boolValue,
            string stringValue => bool.TryParse(stringValue, out var result)
                ? result
                : !string.IsNullOrEmpty(stringValue),
            _ => value != null
        };

        var builder = new CheckInputAction.Builder()
            .SetChecked(shouldCheck)
            .WithForce(_force);

        if (_timeout.HasValue)
        {
            builder.WithTimeout(_timeout.Value);
        }

        ConfigureFieldBuilder(ref builder, fieldConfig);

        var checkAction = builder.WithBrowser(browser).Build();

        await checkAction.ExecuteAsync(context);
    }

    /// <summary>
    ///     Handles text input fields using SetInputAction.
    /// </summary>
    /// <param name="fieldConfig">The field configuration</param>
    /// <param name="value">The text value</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    private async Task HandleTextInput(FormFieldConfig fieldConfig, object value, PlaywrightBrowser browser,
        TestContext context)
    {
        var textValue = value.ToString() ?? string.Empty;

        var builder = new SetInputAction.Builder()
            .Fill(textValue)
            .WithForce(_force);

        if (_timeout.HasValue)
        {
            builder.WithTimeout(_timeout.Value);
        }

        ConfigureFieldBuilder(ref builder, fieldConfig);

        var setInputAction = builder.WithBrowser(browser).Build();

        await setInputAction.ExecuteAsync(context);
    }

    /// <summary>
    ///     Handles file input fields using SetInputFilesAction.
    /// </summary>
    /// <param name="fieldConfig">The field configuration</param>
    /// <param name="value">The file path(s)</param>
    /// <param name="context">The test context</param>
    private async Task HandleFileInput(FormFieldConfig fieldConfig, object value, TestContext context)
    {
        var builder = new SetInputFilesAction.Builder();

        if (_timeout.HasValue)
        {
            builder.WithTimeout(_timeout.Value);
        }

        ConfigureFieldBuilder(ref builder, fieldConfig);

        switch (value)
        {
            case string[] fileArray:
                builder.WithFiles(fileArray);
                break;
            case IEnumerable<string> fileEnumerable:
                builder.WithFiles(fileEnumerable);
                break;
            case string singleFile:
                builder.WithFile(singleFile);
                break;
            case null:
                builder.ClearFiles();
                break;
            default:
                throw new ArgumentException(
                    $"Invalid file input value type: {value.GetType().Name}. Expected string, string[], or IEnumerable<string>");
        }

        var setInputFilesAction = builder.Build();
        await setInputFilesAction.ExecuteAsync(context);
    }

    /// <summary>
    ///     Handles select dropdown fields using DropdownSelectAction.
    /// </summary>
    /// <param name="fieldConfig">The field configuration</param>
    /// <param name="value">The value to select</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    private async Task HandleSelect(FormFieldConfig fieldConfig, object value, PlaywrightBrowser browser,
        TestContext context)
    {
        var builder = new DropdownSelectAction.Builder().WithForce(_force);

        if (_timeout.HasValue)
        {
            builder.WithTimeout(_timeout.Value);
        }

        ConfigureFieldBuilder(ref builder, fieldConfig);

        switch (value)
        {
            case string[] stringArray:
                foreach (var item in stringArray)
                {
                    builder.SelectByValue(item);
                }

                break;
            case IEnumerable<string> stringEnumerable:
                foreach (var item in stringEnumerable)
                {
                    builder.SelectByValue(item);
                }
                break;
            case string singleValue:
                builder.SelectByValue(singleValue);
                break;
            default:
                builder.SelectByValue(value.ToString() ?? string.Empty);
                break;
        }

        var dropdownAction = builder.WithBrowser(browser).Build();
        await dropdownAction.ExecuteAsync(context);
    }

    /// <summary>
    ///     Configuration for a form field including locator strategy and value.
    /// </summary>
    public class FormFieldConfig
    {
        /// <summary>
        ///     Gets or sets the value to be assigned to the form field. This value represents the input
        ///     that will be used to populate the associated field during form processing.
        /// </summary>
        /// <remarks>
        ///     The value can be of any type, such as a string, file, or any other object type
        ///     required to correctly configure the field input. Ensure the value corresponds
        ///     to the expected format of the target form field.
        /// </remarks>
        public object? Value { get; set; }

        /// <summary>
        ///     Gets or sets the type of the form field to be processed. This determines the behavior
        ///     for interacting with the field during the form-filling process, such as text input,
        ///     checkbox selection, file uploading, or other supported actions.
        /// </summary>
        /// <remarks>
        ///     The field type is defined by the <c>FormFieldType</c> enumeration, allowing options
        ///     such as AUTO, TEXT, CHECKBOX, RADIO, FILE, and SELECT. When the value is set to
        ///     AUTO, the appropriate type will be inferred based on the field's characteristics
        ///     during runtime.
        /// </remarks>
        public FormFieldType FieldType { get; set; } = FormFieldType.AUTO;

        /// <summary>
        ///     Gets or sets the identifier of the form field.
        /// </summary>
        /// <remarks>
        ///     The identifier is typically a unique string used to locate
        ///     the exact element in the form. This can correlate to the
        ///     `id` attribute of the HTML element representing the field.
        ///     Ensure this value is distinct and matches the expected format
        ///     for proper field identification.
        /// </remarks>
        public string? Id { get; set; }

        /// <summary>
        ///     Gets or sets the name attribute of the form field used for identification purposes.
        /// </summary>
        /// <remarks>
        ///     The name is a string representing the `name` attribute of the form field in the HTML document.
        ///     It is used as a locator strategy to target the appropriate field during form interactions.
        /// </remarks>
        public string? Name { get; set; }

        /// <summary>
        ///     Gets or sets the CSS selector used to locate the form field on the page.
        /// </summary>
        /// <remarks>
        ///     This property allows you to specify a CSS selector as the locator strategy
        ///     to uniquely identify and interact with a specific form field element.
        ///     Ensure that the CSS selector provided is valid and matches the desired element on the webpage.
        /// </remarks>
        public string? Css { get; set; }

        /// <summary>
        ///     Gets or sets the XPath expression used to locate the form field within the document.
        /// </summary>
        /// <remarks>
        ///     The XPath property specifies the XPath query to identify the target element for interaction.
        ///     This is particularly useful for locating elements when other selectors like ID, name, or CSS are either unavailable
        ///     or not applicable.
        ///     Ensure that the XPath provided is valid and matches the desired element in the document's DOM structure.
        /// </remarks>
        public string? XPath { get; set; }

        /// <summary>
        ///     Gets or sets the text value used as a locator strategy for identifying an element on the page.
        /// </summary>
        /// <remarks>
        ///     The Text property specifies the visible text content of the target element.
        ///     This property can be used as a fallback or primary method for locating an element
        ///     when other locator strategies such as Id, Name, Css, or XPath are not suitable.
        ///     Ensure that the text value is unique or descriptive enough to reliably identify the element.
        /// </remarks>
        public string? Text { get; set; }

        /// <summary>
        ///     Gets or sets the ARIA role used to identify the target element in the user interface.
        /// </summary>
        /// <remarks>
        ///     The role property specifies the ARIA role attribute to locate an element during the form interaction.
        ///     Valid values should match one of the predefined ARIA roles as defined in the WAI-ARIA specification.
        ///     Ensure the role corresponds accurately to the intended element for accessibility and functionality purposes.
        /// </remarks>
        public string? Role { get; set; }
    }

    /// <summary>
    ///     Builder class for creating FillFormAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<FillFormAction, Builder>
    {
        internal Dictionary<string, FormFieldConfig> FieldConfigs { get; } = new();
        internal FormFieldConfig? SubmitButtonConfig { get; private set; }
        internal bool Force { get; private set; }
        internal int? Timeout { get; private set; }

        /// <summary>
        ///     Adds a field by ID with automatic type detection.
        /// </summary>
        /// <param name="id">The field ID</param>
        /// <param name="value">The value to set</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFieldById(string id, object value)
        {
            var config = new FormFieldConfig { Id = id, Value = value };
            FieldConfigs[id] = config;
            return this;
        }

        /// <summary>
        ///     Adds a field by name with automatic type detection.
        /// </summary>
        /// <param name="name">The field name</param>
        /// <param name="value">The value to set</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFieldByName(string name, object value)
        {
            var config = new FormFieldConfig { Name = name, Value = value };
            FieldConfigs[name] = config;
            return this;
        }

        /// <summary>
        ///     Adds a field by CSS selector with automatic type detection.
        /// </summary>
        /// <param name="css">The CSS selector</param>
        /// <param name="value">The value to set</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFieldByCss(string css, object value)
        {
            var config = new FormFieldConfig { Css = css, Value = value };
            FieldConfigs[css] = config;
            return this;
        }

        /// <summary>
        ///     Adds a field by XPath with automatic type detection.
        /// </summary>
        /// <param name="xpath">The XPath expression</param>
        /// <param name="value">The value to set</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFieldByXPath(string xpath, object value)
        {
            var config = new FormFieldConfig { XPath = xpath, Value = value };
            FieldConfigs[xpath] = config;
            return this;
        }

        /// <summary>
        ///     Adds a field by text content with automatic type detection.
        /// </summary>
        /// <param name="text">The text content</param>
        /// <param name="value">The value to set</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFieldByText(string text, object value)
        {
            var config = new FormFieldConfig { Text = text, Value = value };
            FieldConfigs[text] = config;
            return this;
        }

        /// <summary>
        ///     Adds a field by ARIA role with automatic type detection.
        /// </summary>
        /// <param name="role">The ARIA role</param>
        /// <param name="value">The value to set</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFieldByRole(string role, object value)
        {
            var config = new FormFieldConfig { Role = role, Value = value };
            FieldConfigs[role] = config;
            return this;
        }

        /// <summary>
        ///     Adds a field with explicit type specification.
        /// </summary>
        /// <param name="fieldKey">The field key for tracking</param>
        /// <param name="config">The complete field configuration</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithField(string fieldKey, FormFieldConfig config)
        {
            FieldConfigs[fieldKey] = config;
            return this;
        }

        /// <summary>
        ///     Configures the submit button to click after filling the form (by ID).
        /// </summary>
        /// <param name="id">The submit button ID</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSubmitButtonById(string id)
        {
            SubmitButtonConfig = new FormFieldConfig { Id = id };
            return this;
        }

        /// <summary>
        ///     Configures the submit button to click after filling the form (by name).
        /// </summary>
        /// <param name="name">The submit button name</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSubmitButtonByName(string name)
        {
            SubmitButtonConfig = new FormFieldConfig { Name = name };
            return this;
        }

        /// <summary>
        ///     Configures the submit button to click after filling the form (by CSS selector).
        /// </summary>
        /// <param name="css">The CSS selector</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSubmitButtonByCss(string css)
        {
            SubmitButtonConfig = new FormFieldConfig { Css = css };
            return this;
        }

        /// <summary>
        ///     Configures the submit button to click after filling the form (by XPath).
        /// </summary>
        /// <param name="xpath">The XPath expression</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSubmitButtonByXPath(string xpath)
        {
            SubmitButtonConfig = new FormFieldConfig { XPath = xpath };
            return this;
        }

        /// <summary>
        ///     Configures the submit button to click after filling the form (by text content).
        /// </summary>
        /// <param name="text">The button text</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSubmitButtonByText(string text)
        {
            SubmitButtonConfig = new FormFieldConfig { Text = text };
            return this;
        }

        /// <summary>
        ///     Configures the submit button to click after filling the form (by ARIA role).
        /// </summary>
        /// <param name="role">The ARIA role</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSubmitButtonByRole(string role)
        {
            SubmitButtonConfig = new FormFieldConfig { Role = role };
            return this;
        }

        /// <summary>
        ///     Configures a custom submit button configuration.
        /// </summary>
        /// <param name="config">The submit button configuration</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSubmitButton(FormFieldConfig config)
        {
            SubmitButtonConfig = config;
            return this;
        }

        /// <summary>
        ///     Sets whether to force actions even if elements are not actionable.
        /// </summary>
        /// <param name="force">True to force actions</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithForce(bool force = true)
        {
            Force = force;
            return this;
        }

        /// <summary>
        ///     Sets the timeout for each field operation.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(int timeoutMs)
        {
            Timeout = timeoutMs;
            return this;
        }

        /// <summary>
        ///     Builds the FillFormAction instance.
        /// </summary>
        /// <returns>A new FillFormAction instance</returns>
        public override FillFormAction Build()
        {
            return new FillFormAction(this);
        }
    }
}
