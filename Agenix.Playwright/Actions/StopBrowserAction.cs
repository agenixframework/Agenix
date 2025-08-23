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
///     Playwright action for stopping a browser instance.
///     This action stops the Playwright browser and releases all associated resources
///     including contexts, pages, and the browser instance itself.
/// </summary>
public class StopBrowserAction : AbstractPlaywrightAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(StopBrowserAction));

    /// <summary>
    ///     Represents an action for stopping a Playwright browser instance.
    /// </summary>
    public StopBrowserAction(Builder builder) : this("stop-browser", builder) { }

    /// <summary>
    ///     Represents an action responsible for stopping a Playwright browser instance.
    /// </summary>
    public StopBrowserAction(string name, Builder builder) : base(name, builder)
    {
    }

    /// <summary>
    ///     Executes the browser stop action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance to stop</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Stopping browser of type {BrowserType}",
                browser.EndpointConfiguration.BrowserType);

            // Check if the browser is already stopped
            if (!browser.IsStarted)
            {
                Logger.LogWarning("Browser is not started, skipping stop action");
                return;
            }

            // Stop the browser
            await browser.StopAsync();

            Logger.LogInformation("Browser stopped successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to stop Playwright browser");
            throw new AgenixSystemException("Failed to stop Playwright browser", ex);
        }
    }

    /// <summary>
    ///     Builder class for creating StopBrowserAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<StopBrowserAction, Builder>
    {
        /// <summary>
        ///     Builds the StopBrowserAction instance.
        /// </summary>
        /// <returns>A new StopBrowserAction instance</returns>
        public override StopBrowserAction Build()
        {
            return new StopBrowserAction(this);
        }
    }
}
