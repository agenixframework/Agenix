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
using Agenix.Api.Validation.Matcher;
using Agenix.Selenium.Endpoint;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Access current alert dialog. In case no alert is opened action fails.
///     Action supports optional alert text validation.
/// </summary>
public class AlertAction : AbstractSeleniumAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(AlertAction));

    /// <summary>
    ///     Accept or dismiss dialog
    /// </summary>
    private readonly bool _accept;

    /// <summary>
    ///     Optional dialog text validation
    /// </summary>
    private readonly string _text;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public AlertAction(Builder builder) : base("alert", builder)
    {
        _accept = builder.Accept;
        _text = builder.Text;
    }

    /// <summary>
    ///     Gets whether to accept the alert
    /// </summary>
    public bool Accept => _accept;

    /// <summary>
    ///     Gets the expected text
    /// </summary>
    public string Text => _text;

    /// <summary>
    ///     Executes the defined action on an alert dialog in a Selenium browser context.
    ///     Validates the text of the alert, sets a variable in the test context, and
    ///     either accepts or dismisses the alert based on the specified configuration.
    /// </summary>
    /// <param name="browser">The Selenium browser instance used to interact with the browser during execution.</param>
    /// <param name="context">The test context containing execution state and variables.</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        IAlert alert;
        try
        {
            alert = browser.WebDriver.SwitchTo().Alert();
        }
        catch (NoAlertPresentException)
        {
            throw new AgenixSystemException("Failed to access alert dialog - not found");
        }

        // Handle null alert (additional safety check)
        if (alert == null)
        {
            throw new AgenixSystemException("Failed to access alert dialog - not found");
        }

        if (!string.IsNullOrWhiteSpace(_text))
        {
            Logger.LogInformation("Validating alert text");

            var alertText = context.ReplaceDynamicContentInString(_text);

            if (ValidationMatcherUtils.IsValidationMatcherExpression(alertText))
            {
                ValidationMatcherUtils.ResolveValidationMatcher("alertText", alert.Text, alertText, context);
            }
            else
            {
                if (!alertText.Equals(alert.Text))
                {
                    throw new ValidationException($"Failed to validate alert dialog text, " +
                                                  $"expected '{alertText}', but was '{alert.Text}'");
                }
            }

            Logger.LogDebug("Alert text validation successful - All values Ok");
        }

        context.SetVariable(SeleniumHeaders.SeleniumAlertText, alert.Text);

        if (_accept)
        {
            alert.Accept();
        }
        else
        {
            alert.Dismiss();
        }
    }

    /// <summary>
    ///     Action builder for AlertAction
    /// </summary>
    public class Builder : Builder<AlertAction, Builder>
    {
        public bool Accept { get; private set; } = true;
        public string Text { get; private set; }

        /// <summary>
        ///     Add alert text validation
        /// </summary>
        public Builder SetText(string text)
        {
            Text = text;
            return self;
        }

        /// <summary>
        ///     Accept alert dialog
        /// </summary>
        public Builder AcceptAlert()
        {
            Accept = true;
            return self;
        }

        /// <summary>
        ///     Dismiss alert dialog
        /// </summary>
        public Builder DismissAlert()
        {
            Accept = false;
            return self;
        }

        public override AlertAction Build()
        {
            return new AlertAction(this);
        }
    }
}
