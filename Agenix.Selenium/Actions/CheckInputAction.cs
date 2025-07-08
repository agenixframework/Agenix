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

namespace Agenix.Selenium.Actions;

/// <summary>
///     Sets value on checkbox form input element.
/// </summary>
public class CheckInputAction : FindElementAction
{
    /// <summary>
    ///     Checkbox checked state
    /// </summary>
    private readonly bool _checked;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public CheckInputAction(Builder builder) : base("check-input", builder)
    {
        _checked = builder.Checked;
    }

    /// <summary>
    ///     Gets the checked state
    /// </summary>
    public bool Checked => _checked;

    /// <summary>
    ///     Executes the main logic of the CheckInputAction by setting the desired state of a checkbox element.
    /// </summary>
    /// <param name="element">The web element representing the checkbox input to interact with.</param>
    /// <param name="browser">The SeleniumBrowser instance managing the Selenium WebDriver session.</param>
    /// <param name="context">The TestContext providing the execution context for the action.</param>
    protected override void Execute(IWebElement element, SeleniumBrowser browser, TestContext context)
    {
        base.Execute(element, browser, context);

        if ((element.Selected && !_checked) || (_checked && !element.Selected))
        {
            element.Click();
        }
    }

    /// <summary>
    ///     Action builder for CheckInputAction
    /// </summary>
    public new class Builder : FindElementAction.Builder
    {
        /// <summary>
        ///     Represents the checked state of a checkbox form input element.
        /// </summary>
        public bool Checked { get; private set; }

        /// <summary>
        ///     Set the checked state for the checkbox
        /// </summary>
        public Builder SetChecked(bool isChecked)
        {
            Checked = isChecked;
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
        ///     Creates and returns a new instance of the CheckInputAction class.
        /// </summary>
        /// <returns>
        ///     An instance of CheckInputAction, constructed using the provided configuration.
        /// </returns>
        public override CheckInputAction Build()
        {
            return new CheckInputAction(this);
        }
    }
}
