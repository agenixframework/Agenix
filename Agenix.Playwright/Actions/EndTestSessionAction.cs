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

#endregion

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for ending a test session and cleaning up isolated resources
///     without stopping the browser. This allows the browser to be reused for subsequent tests.
/// </summary>
public class EndTestSessionAction : AbstractPlaywrightAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(EndTestSessionAction));

    /// <summary>
    ///     Initializes a new instance of the EndTestSessionAction.
    /// </summary>
    /// <param name="builder">The builder instance containing configuration</param>
    public EndTestSessionAction(Builder builder) : base("endTestSession", builder)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the EndTestSessionAction with a custom name.
    /// </summary>
    /// <param name="name">The action name</param>
    /// <param name="builder">The builder instance containing configuration</param>
    public EndTestSessionAction(string name, Builder builder) : base(name, builder)
    {
    }

    /// <summary>
    ///     Executes the test session cleanup without stopping the browser.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Ending test session and cleaning up isolated resources");

            if (!browser.EndpointConfiguration.TestIsolation)
            {
                Logger.LogDebug("Test isolation is not enabled, no cleanup needed");
                return;
            }

            await HandleTestIsolationCleanup(browser, context);

            Logger.LogInformation("Test session ended successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to end test session");
            throw new AgenixSystemException("Failed to end test session", ex);
        }
    }

    /// <summary>
    ///     Handles test isolation cleanup for the current test session.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    private static async Task HandleTestIsolationCleanup(PlaywrightBrowser browser, TestContext context)
    {
        if (context.GetVariables().TryGetValue(PlaywrightHeaders.PlaywrightTestSessionId, out var testIdObj) &&
            testIdObj is string testId)
        {
            try
            {
                await browser.EndTestSessionAsync(testId);
                Logger.LogDebug("Successfully ended test session {TestId}", testId);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to cleanup test session {TestId}", testId);
            }
            finally
            {
                context.GetVariables().Remove(PlaywrightHeaders.PlaywrightTestSessionId);
            }
        }
        else
        {
            Logger.LogWarning("Test isolation is enabled but no test session ID found in context");
        }
    }

    /// <summary>
    ///     Builder for creating EndTestSessionAction instances.
    /// </summary>
    public class Builder : Builder<EndTestSessionAction, Builder>
    {
        /// <summary>
        ///     Builds the EndTestSessionAction instance.
        /// </summary>
        /// <returns>A new EndTestSessionAction instance</returns>
        public override EndTestSessionAction Build()
        {
            return new EndTestSessionAction(this);
        }
    }
}
