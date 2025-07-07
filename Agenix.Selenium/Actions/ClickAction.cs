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
///     Clicks on a web element after finding and validating it.
/// </summary>
public class ClickAction : FindElementAction
{
    /// <summary>
    ///     Default constructor
    /// </summary>
    public ClickAction(Builder builder) : base("click", builder)
    {
    }

    /// <summary>
    ///     Executes the click action on the found web element
    /// </summary>
    protected override void Execute(IWebElement webElement, SeleniumBrowser browser, TestContext context)
    {
        // Call base implementation to set the element in context
        base.Execute(webElement, browser, context);

        // Perform the click action
        webElement.Click();
    }

    /// <summary>
    ///     Action builder for ClickAction
    /// </summary>
    public new class Builder : FindElementAction.Builder
    {
        /// <summary>
        ///     Builds and returns a new instance of the ClickAction class.
        /// </summary>
        /// <returns>A new instance of the ClickAction class.</returns>
        public override ClickAction Build()
        {
            return new ClickAction(this);
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
    }
}
