#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion


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
///     Hovers over a web element to trigger hover effects or reveal hidden content.
/// </summary>
public class HoverAction : FindElementAction
{
    /// <summary>
    ///     Default constructor
    /// </summary>
    public HoverAction(Builder builder) : base("hover", builder)
    {
    }

    /// <summary>
    ///     Performs the hover action on a specified web element, allowing hover effects such as tooltips or content display to
    ///     be triggered.
    /// </summary>
    /// <param name="webElement">The web element on which the hover action is to be performed.</param>
    /// <param name="browser">The Selenium browser instance controlling the WebDriver for interaction.</param>
    /// <param name="context">The test context providing additional data about the test environment and execution.</param>
    protected override void Execute(IWebElement element, SeleniumBrowser browser, TestContext context)
    {
        base.Execute(element, browser, context);

        var actions = new OpenQA.Selenium.Interactions.Actions(browser.WebDriver);
        actions.MoveToElement(element).Perform();
    }

    /// <summary>
    ///     Action builder for HoverAction
    /// </summary>
    public new class Builder : FindElementAction.Builder
    {
        /// <summary>
        ///     Builds and returns an instance of the HoverAction class.
        /// </summary>
        /// <returns>A new instance of the HoverAction class.</returns>
        public override HoverAction Build()
        {
            return new HoverAction(this);
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
