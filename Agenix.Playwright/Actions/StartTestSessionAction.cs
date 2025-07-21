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

using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for starting a test session with isolation.
///     This action creates isolated contexts or pages based on the test isolation configuration.
/// </summary>
public class StartTestSessionAction : AbstractPlaywrightAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(StartTestSessionAction));
    private static long _testIdCounter;
    private readonly BrowserNewContextOptions? _contextOptions;

    private readonly string? _customTestId;

    /// <summary>
    ///     Initializes a new instance of the StartTestSessionAction.
    /// </summary>
    /// <param name="builder">The builder instance containing configuration</param>
    public StartTestSessionAction(Builder builder) : base("startTestSession", builder)
    {
        _customTestId = builder.CustomTestId;
        _contextOptions = builder.ContextOptions;
    }

    /// <summary>
    ///     Initializes a new instance of the StartTestSessionAction with a custom name.
    /// </summary>
    /// <param name="name">The action name</param>
    /// <param name="builder">The builder instance containing configuration</param>
    public StartTestSessionAction(string name, Builder builder) : base(name, builder)
    {
        _customTestId = builder.CustomTestId;
        _contextOptions = builder.ContextOptions;
    }

    /// <summary>
    ///     Executes the test session start action.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Starting test session with isolation mode {Mode}",
                browser.EndpointConfiguration.TestIsolationMode);

            if (!browser.IsStarted)
            {
                throw new InvalidOperationException("Browser is not started. Call StartBrowserAction first.");
            }

            if (!browser.EndpointConfiguration.TestIsolation)
            {
                Logger.LogDebug("Test isolation is disabled, no test session created");
                return;
            }

            var testId = _customTestId ?? GenerateTestId(context);

            await browser.StartTestSessionAsync(testId, _contextOptions);

            // Store the test ID in context for cleanup
            context.SetVariable(PlaywrightHeaders.PlaywrightTestSessionId, testId);

            Logger.LogInformation("Test session {TestId} started successfully", testId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start test session");
            throw new AgenixSystemException("Failed to start test session", ex);
        }
    }

    /// <summary>
    ///     Generates a unique test ID for the current test context.
    /// </summary>
    /// <param name="context">The test context</param>
    /// <returns>A unique test identifier</returns>
    private static string GenerateTestId(TestContext context)
    {
        const string unknown = "unknown";
        var nameSpace = context.GetVariables().TryGetValue(AgenixSettings.TestNameSpaceVariable(), out var cls)
            ? cls.ToString()?.Split('.').LastOrDefault() ?? unknown
            : unknown;

        var uniqueId = Interlocked.Increment(ref _testIdCounter);

        return $"{nameSpace}_{uniqueId}";
    }

    /// <summary>
    ///     Builder for creating StartTestSessionAction instances.
    /// </summary>
    public class Builder : Builder<StartTestSessionAction, Builder>
    {
        /// <summary>
        ///     Gets or sets the custom test ID to use.
        /// </summary>
        public string? CustomTestId { get; set; }

        /// <summary>
        ///     Gets or sets the custom context options to use.
        /// </summary>
        public BrowserNewContextOptions? ContextOptions { get; set; }

        /// <summary>
        ///     Sets a custom test ID for the session.
        /// </summary>
        /// <param name="testId">The custom test ID</param>
        /// <returns>This builder instance for method chaining</returns>
        public Builder WithTestId(string testId)
        {
            CustomTestId = testId;
            return this;
        }

        /// <summary>
        ///     Sets custom context options for the session.
        /// </summary>
        /// <param name="options">The context options</param>
        /// <returns>This builder instance for method chaining</returns>
        public Builder WithContextOptions(BrowserNewContextOptions options)
        {
            ContextOptions = options;
            return this;
        }

        /// <summary>
        ///     Builds the StartTestSessionAction instance.
        /// </summary>
        /// <returns>A new StartTestSessionAction instance</returns>
        public override StartTestSessionAction Build()
        {
            return new StartTestSessionAction(this);
        }
    }
}
