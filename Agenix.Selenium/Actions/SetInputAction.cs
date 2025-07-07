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
///     Sets new text value for form input element.
/// </summary>
public class SetInputAction : FindElementAction
{
    /// <summary>
    ///     Value to set on input
    /// </summary>
    private readonly string _value;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public SetInputAction(Builder builder) : base("set-input", builder)
    {
        _value = builder.Value;
    }

    /// <summary>
    ///     Gets the value to be set
    /// </summary>
    public string Value => _value;

    /// <summary>
    ///     Executes the set input action on the found web element
    /// </summary>
    protected override void Execute(IWebElement webElement, SeleniumBrowser browser, TestContext context)
    {
        // Call base implementation to set the element in context
        base.Execute(webElement, browser, context);

        var tagName = webElement.TagName;
        if (!"select".Equals(tagName, StringComparison.OrdinalIgnoreCase))
        {
            // For regular input elements
            webElement.Clear();
            webElement.SendKeys(context.ReplaceDynamicContentInString(_value));
        }
        else
        {
            // For select elements
            var select = new SelectElement(webElement);
            select.SelectByValue(context.ReplaceDynamicContentInString(_value));
        }
    }

    /// <summary>
    ///     Action builder for SetInputAction
    /// </summary>
    public new class Builder : FindElementAction.Builder
    {
        public string Value { get; private set; }

        /// <summary>
        ///     Set the value to input
        /// </summary>
        public Builder SetValue(string value)
        {
            Value = value;
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

        public override SetInputAction Build()
        {
            return new SetInputAction(this);
        }
    }
}
