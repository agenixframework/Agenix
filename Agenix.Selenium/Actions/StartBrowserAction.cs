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

using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Selenium.Endpoint;
using Microsoft.Extensions.Logging;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Selenium action for starting a browser instance.
/// </summary>
public class StartBrowserAction : AbstractSeleniumAction, ITestAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(StartBrowserAction));

    /// <summary>
    ///     Allow already started browser.
    /// </summary>
    private readonly bool _allowAlreadyStarted;

    /// <summary>
    ///     Default constructor.
    /// </summary>
    /// <param name="builder">The action builder</param>
    public StartBrowserAction(Builder builder) : base("start", builder)
    {
        _allowAlreadyStarted = builder._allowAlreadyStarted;
    }

    /// <summary>
    ///     Gets the allow already started flag.
    /// </summary>
    /// <returns>True if already started browsers are allowed</returns>
    public bool IsAllowAlreadyStarted => _allowAlreadyStarted;

    /// <summary>
    ///     Executes the start browser action.
    /// </summary>
    /// <param name="browser">The Selenium browser instance</param>
    /// <param name="context">The test context</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        if (!_allowAlreadyStarted && browser.IsStarted)
        {
            Logger.LogWarning("There are some open web browsers. They will be stopped.");
            browser.Stop();
        }
        else if (browser.IsStarted)
        {
            Logger.LogInformation("Browser already started - skip start action");
            context.SetVariable(SeleniumHeaders.SeleniumBrowser, browser.Name);
            return;
        }

        Logger.LogInformation("Opening browser of type {BrowserType}",
            browser.EndpointConfiguration.BrowserType);
        browser.Start();

        // Navigate to the start page if configured
        if (!string.IsNullOrWhiteSpace(Browser.EndpointConfiguration.StartPageUrl))
        {
            var openStartPage = new NavigateAction.Builder()
                .WithBrowser(browser)
                .SetPage(Browser.EndpointConfiguration.StartPageUrl)
                .Build();
            openStartPage.Execute(context);
        }

        context.SetVariable(SeleniumHeaders.SeleniumBrowser, browser.Name);
    }

    /// <summary>
    ///     Builder for creating StartBrowserAction instances.
    /// </summary>
    public class Builder : Builder<StartBrowserAction, Builder>
    {
        /// <summary>
        ///     Allow already started browser flag.
        /// </summary>
        internal bool _allowAlreadyStarted { get; private set; } = true;

        /// <summary>
        ///     Sets whether to allow already started browsers.
        /// </summary>
        /// <param name="permission">True to allow already started browsers</param>
        /// <returns>This builder instance</returns>
        public Builder AllowAlreadyStarted(bool permission)
        {
            _allowAlreadyStarted = permission;
            return this;
        }

        /// <summary>
        ///     Builds the StartBrowserAction instance.
        /// </summary>
        /// <returns>A configured StartBrowserAction</returns>
        public override StartBrowserAction Build()
        {
            return new StartBrowserAction(this);
        }
    }
}
