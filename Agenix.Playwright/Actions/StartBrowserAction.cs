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

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for starting a browser instance.
///     This action initializes the Playwright browser with the specified configuration
///     and makes it available for later actions in the test context.
/// </summary>
public class StartBrowserAction : AbstractPlaywrightAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(StartBrowserAction));

    private readonly bool _allowAlreadyStarted;

    /// <summary>
    ///     Represents an action for starting a Playwright browser instance.
    ///     This action initializes the browser with the specified configuration
    ///     and prepares it for interaction within the test execution context.
    /// </summary>
    public StartBrowserAction(Builder builder) : this("start-browser", builder) { }

    /// <summary>
    ///     Represents an action for starting a Playwright browser instance.
    ///     This action configures and initializes the browser using the specified settings,
    ///     making it available for interaction within the test context.
    /// </summary>
    public StartBrowserAction(string name, Builder builder) : base(name, builder)
    {
        _allowAlreadyStarted = builder.AllowAlreadyStarted;
    }

    /// <summary>
    ///     Executes the browser start action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance to start</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Opening browser of type {BrowserType}",
                browser.EndpointConfiguration.BrowserType);

            switch (_allowAlreadyStarted)
            {
                // Check if the browser is already started and handle accordingly
                case false when browser.IsStarted:
                    Logger.LogWarning("There are some open web browsers. They will be stopped.");
                    await browser.StopAsync();
                    break;
                case true when browser.IsStarted:
                    Logger.LogWarning("Browser is already started, skipping start action");
                    return;
            }

            // Start the browser
            await browser.StartAsync();

            Logger.LogInformation("Browser started successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start Playwright browser");
            throw new AgenixSystemException("Failed to start Playwright browser", ex);
        }
    }

    /// <summary>
    ///     Overrides the base DoExecute method to handle browser startup without context resolution.
    ///     StartBrowserAction creates contexts, so it shouldn't try to resolve them beforehand.
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="cancellationToken"></param>
    public override async Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        // Use the Browser property from the base class instead of ResolveBrowser
        var browserToUse = Browser;

        // Skip context resolution for start action - we're creating them
        // Just execute the browser startup directly
        await Execute(browserToUse, context);
    }


    /// <summary>
    ///     Builder class for creating StartBrowserAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<StartBrowserAction, Builder>
    {
        internal bool AllowAlreadyStarted { get; private set; } = true;

        /// <summary>
        ///     Sets whether to allow starting when a browser is already started.
        ///     If false, existing browsers will be stopped before starting a new one.
        /// </summary>
        /// <param name="allow">True to allow already started browsers, false to stop them first</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithAllowAlreadyStarted(bool allow = true)
        {
            AllowAlreadyStarted = allow;
            return this;
        }

        /// <summary>
        ///     Builds the StartBrowserAction instance.
        /// </summary>
        /// <returns>A new StartBrowserAction instance</returns>
        public override StartBrowserAction Build()
        {
            return new StartBrowserAction(this);
        }
    }
}
