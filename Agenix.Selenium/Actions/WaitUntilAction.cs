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
using Agenix.Selenium.Endpoint;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Waits until element is visible or hidden.
/// </summary>
public class WaitUntilAction : FindElementAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(WaitUntilAction));

    /// <summary>
    ///     Wait condition on element.
    /// </summary>
    private readonly string _condition;

    /// <summary>
    ///     Wait timeout in milliseconds.
    /// </summary>
    private readonly long _timeout;

    /// <summary>
    ///     Default constructor.
    /// </summary>
    /// <param name="builder">The action builder</param>
    public WaitUntilAction(Builder builder) : base("wait", builder)
    {
        _timeout = builder._timeout;
        _condition = builder._condition;
    }

    /// <summary>
    ///     Gets the timeout.
    /// </summary>
    public long Timeout => _timeout;

    /// <summary>
    ///     Gets the condition.
    /// </summary>
    public string Condition => _condition;

    /// <summary>
    ///     Executes the wait until action on the web element.
    /// </summary>
    /// <param name="webElement">The web element to wait for</param>
    /// <param name="browser">The Selenium browser instance</param>
    /// <param name="context">The test context</param>
    protected override void Execute(IWebElement webElement, SeleniumBrowser browser, TestContext context)
    {
        var wait = new WebDriverWait(browser.WebDriver, TimeSpan.FromMilliseconds(_timeout));

        Logger.LogDebug("Waiting for element condition '{Condition}' with timeout {Timeout}ms", _condition, _timeout);

        try
        {
            switch (_condition?.ToLowerInvariant())
            {
                case "hidden":
                    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath("*")));
                    break;
                case "visible":
                    wait.Until(ExpectedConditions.ElementToBeClickable(webElement));
                    break;
                default:
                    throw new AgenixSystemException($"Unknown wait condition: {_condition}");
            }

            Logger.LogInformation("Element condition '{Condition}' satisfied", _condition);
        }
        catch (WebDriverTimeoutException ex)
        {
            throw new AgenixSystemException(
                $"Timeout waiting for element condition '{_condition}' after {_timeout}ms", ex);
        }
    }

    /// <summary>
    ///     Validates the element - no validation needed for wait action.
    /// </summary>
    /// <param name="element">The web element</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    protected override void Validate(IWebElement element, SeleniumBrowser browser, TestContext context)
    {
        // No validation needed for wait action
    }

    /// <summary>
    ///     Builder for creating WaitUntilAction instances.
    /// </summary>
    public class Builder : FindElementAction.Builder
    {
        internal long _timeout { get; private set; } = 5000L;
        internal string _condition { get; private set; }

        /// <summary>
        ///     Sets the condition to wait for element visibility.
        /// </summary>
        /// <returns>This builder instance</returns>
        public Builder Visible()
        {
            return Condition("visible");
        }

        /// <summary>
        ///     Sets the condition to wait for element to be hidden.
        /// </summary>
        /// <returns>This builder instance</returns>
        public Builder Hidden()
        {
            return Condition("hidden");
        }

        /// <summary>
        ///     Sets the wait condition.
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <returns>This builder instance</returns>
        public Builder Condition(string condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the wait operation.
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>This builder instance</returns>
        public Builder Timeout(long timeout)
        {
            if (timeout <= 0)
            {
                throw new ArgumentException("Timeout must be positive", nameof(timeout));
            }

            _timeout = timeout;
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
        ///     Builds the WaitUntilAction instance.
        /// </summary>
        /// <returns>A configured WaitUntilAction</returns>
        public override WaitUntilAction Build()
        {
            if (string.IsNullOrWhiteSpace(_condition))
            {
                throw new InvalidOperationException("Wait condition must be specified");
            }

            return new WaitUntilAction(this);
        }
    }
}
