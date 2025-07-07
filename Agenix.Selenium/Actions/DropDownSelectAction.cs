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
using Agenix.Selenium.Endpoint;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Selects dropdown option(s) on form input.
/// </summary>
public class DropDownSelectAction : FindElementAction
{
    /// <summary>
    ///     Single option to select
    /// </summary>
    private readonly string _option;

    /// <summary>
    ///     Multiple options to select
    /// </summary>
    private readonly List<string> _options;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public DropDownSelectAction(Builder builder) : base("dropdown-select", builder)
    {
        _option = builder.Option;
        _options = builder.Options.ToList();
    }

    /// <summary>
    ///     Gets the single option to select
    /// </summary>
    public string Option => _option;

    /// <summary>
    ///     Gets the multiple options to select
    /// </summary>
    public IReadOnlyList<string> Options => _options.AsReadOnly();

    /// <summary>
    ///     Executes the logic to select one or multiple options from a dropdown menu.
    /// </summary>
    /// <param name="webElement">The web element representing the dropdown to be interacted with.</param>
    /// <param name="browser">The browser instance currently being used for testing.</param>
    /// <param name="context">The context object containing test-related data and utilities.</param>
    protected override void Execute(IWebElement webElement, SeleniumBrowser browser, TestContext context)
    {
        base.Execute(webElement, browser, context);

        var dropdown = new SelectElement(webElement);

        if (!string.IsNullOrWhiteSpace(_option))
        {
            var resolvedOption = context.ReplaceDynamicContentInString(_option);
            dropdown.SelectByValue(resolvedOption);
        }

        if (_options != null && _options.Any())
        {
            if (IsInternetExplorer(browser))
            {
                // IE doesn't support multi-select with CTRL key properly
                foreach (var option in _options)
                {
                    var resolvedOption = context.ReplaceDynamicContentInString(option);
                    dropdown.SelectByValue(resolvedOption);
                }
            }
            else
            {
                SelectMultipleOptions(dropdown, browser, context);
            }
        }
    }

    /// <summary>
    ///     Selects multiple options in a dropdown element using the Control key for multi-selection.
    /// </summary>
    /// <param name="dropdown">The dropdown web element represented as a SelectElement.</param>
    /// <param name="browser">The Selenium browser instance that manages the WebDriver actions.</param>
    /// <param name="context">The test execution context, used for dynamic content resolution.</param>
    private void SelectMultipleOptions(SelectElement dropdown, SeleniumBrowser browser, TestContext context)
    {
        var optionElements = dropdown.Options;
        var actions = new OpenQA.Selenium.Interactions.Actions(browser.WebDriver);

        actions.KeyDown(Keys.Control);

        foreach (var optionValue in _options)
        {
            var resolvedValue = context.ReplaceDynamicContentInString(optionValue);

            foreach (var option in optionElements)
            {
                if (!option.Selected && IsSameValue(option, resolvedValue))
                {
                    actions.MoveToElement(option).Click(option);
                }
            }
        }

        actions.KeyUp(Keys.Control);
        actions.Perform();
    }

    /// <summary>
    ///     Determines whether the value of the specified option element matches the given string value.
    /// </summary>
    /// <param name="option">The web element representing the option to be compared.</param>
    /// <param name="value">The string value to compare against the option's text or value attribute.</param>
    /// <returns>True if the option's text or value attribute matches the given string value, otherwise false.</returns>
    private bool IsSameValue(IWebElement option, string value)
    {
        var optionText = option.Text?.Trim();
        return value.Equals(!string.IsNullOrWhiteSpace(optionText) ? optionText : option.GetAttribute("value"));
    }

    /// <summary>
    ///     Determines if the specified browser is Internet Explorer by checking its browser type.
    /// </summary>
    /// <param name="browser">The <see cref="SeleniumBrowser" /> instance whose type is to be checked.</param>
    /// <returns>
    ///     A boolean value indicating whether the browser is Internet Explorer.
    ///     Returns true if the browser type contains "internet" or "ie"; otherwise, false.
    /// </returns>
    private bool IsInternetExplorer(SeleniumBrowser browser)
    {
        return browser.EndpointConfiguration.BrowserType.ToLower().Contains("internet") ||
               browser.EndpointConfiguration.BrowserType.ToLower().Contains("ie");
    }

    /// <summary>
    ///     Action builder for DropDownSelectAction
    /// </summary>
    public new class Builder : FindElementAction.Builder
    {
        public string Option { get; private set; }
        public List<string> Options { get; } = [];

        /// <summary>
        ///     Set single option to select
        /// </summary>
        public Builder SetOption(string option)
        {
            Option = option;
            return this;
        }

        /// <summary>
        ///     Set multiple options to select
        /// </summary>
        public Builder SetOptions(params string[] options)
        {
            return SetOptions(options.ToList());
        }

        /// <summary>
        ///     Set multiple options to select
        /// </summary>
        public Builder SetOptions(IEnumerable<string> options)
        {
            Options.AddRange(options);
            return this;
        }

        /// <summary>
        ///     Add single option to the list of options to select
        /// </summary>
        public Builder AddOption(string option)
        {
            Options.Add(option);
            return this;
        }

        /// <summary>
        ///     Override Element method to return CheckInputAction.Builder for fluent interface
        /// </summary>
        public new Builder Element(By by)
        {
            base.Element(by);
            return this;
        }

        /// <summary>
        ///     Override Element method to return CheckInputAction.Builder for fluent interface
        /// </summary>
        public new Builder Element(string property, string propertyValue)
        {
            base.Element(property, propertyValue);
            return this;
        }

        /// <summary>
        ///     Override WithBrowser method to return CheckInputAction.Builder for fluent interface
        /// </summary>
        public new Builder WithBrowser(SeleniumBrowser seleniumBrowser)
        {
            base.WithBrowser(seleniumBrowser);
            return this;
        }

        /// <summary>
        ///     Constructs and returns a new instance of the DropDownSelectAction class.
        /// </summary>
        /// <returns>A new instance of DropDownSelectAction.</returns>
        public override DropDownSelectAction Build()
        {
            return new DropDownSelectAction(this);
        }
    }
}
