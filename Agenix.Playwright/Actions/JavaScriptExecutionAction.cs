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
using Microsoft.Playwright;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for executing JavaScript code in the browser context.
///     Supports script execution with parameters, return value handling, and error detection.
/// </summary>
public class JavaScriptAction : AbstractPlaywrightAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(JavaScriptAction));
    private readonly object[]? _arguments;
    private readonly bool _checkForErrors;
    private readonly string? _resultVariableName;

    private string _script;
    private readonly bool _waitForFunction;
    private readonly PageWaitForFunctionOptions? _waitOptions;

    /// <summary>
    ///     Represents a Playwright action for executing JavaScript code within the browser context.
    ///     Provides capabilities for script execution with parameters, return value handling, and error detection.
    /// </summary>
    public JavaScriptAction(Builder builder) : this("javascript", builder)
    {
    }

    /// <summary>
    ///     Playwright action for executing JavaScript code in the browser context.
    ///     Supports script execution with parameters, return value handling, and error detection.
    /// </summary>
    public JavaScriptAction(string name, Builder builder) : base(name, builder)
    {
        _script = builder.Script ?? throw new ArgumentException("Script cannot be null or empty");
        _arguments = builder.Arguments;
        _resultVariableName = builder.ResultVariableName;
        _checkForErrors = builder.CheckForErrors;
        _waitForFunction = builder.WaitForFunction;
        _waitOptions = builder.WaitOptions;
    }

    /// <summary>
    ///     Executes the JavaScript action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            _script = context.ReplaceDynamicContentInString(_script);
            Logger.LogInformation("Executing JavaScript: {Script}",
                _script.Length > 100 ? _script[..100] + "..." : _script);

            var page = browser.GetCurrentPage();
            if (page == null)
            {
                throw new InvalidOperationException("No active page available");
            }

            object? result;

            if (_waitForFunction)
            {
                Logger.LogDebug("Executing JavaScript as wait for function");
                var handle = await page.WaitForFunctionAsync(_script, _arguments, _waitOptions);
                result = await handle.JsonValueAsync<string>();
            }
            else
            {
                Logger.LogDebug("Executing JavaScript as evaluate");
                result = await page.EvaluateAsync(_script, _arguments);
            }

            // Store result in context variable if specified
            if (!string.IsNullOrEmpty(_resultVariableName) && result != null)
            {
                context.SetVariable(_resultVariableName, result);
                Logger.LogDebug("JavaScript result stored in variable '{VariableName}': {Result}",
                    _resultVariableName, result);
            }

            // Check for JavaScript errors if requested
            if (_checkForErrors)
            {
                await CheckForJavaScriptErrors(page, context);
            }

            Logger.LogInformation("JavaScript execution completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute JavaScript");
            throw new AgenixSystemException("Failed to execute JavaScript", ex);
        }
    }

    /// <summary>
    ///     Checks for JavaScript errors in the console and stores them in the context.
    /// </summary>
    /// <param name="page">The page instance</param>
    /// <param name="context">The test context</param>
    private static async Task CheckForJavaScriptErrors(IPage page, TestContext context)
    {
        try
        {
            Logger.LogDebug("Checking for JavaScript errors");

            // Get console messages - this would need to be collected via console event handlers
            // For demonstration, we'll check for common error patterns
            const string errorCheckScript = """

                                                            (() => {
                                                                const errors = [];

                                                                // Check for global error handlers
                                                                if (window.jsErrors && Array.isArray(window.jsErrors)) {
                                                                    errors.push(...window.jsErrors);
                                                                }

                                                                // Check for unhandled promise rejections
                                                                if (window.unhandledRejections && Array.isArray(window.unhandledRejections)) {
                                                                    errors.push(...window.unhandledRejections);
                                                                }

                                                                return errors;
                                                            })()

                                            """;

            var errors = await page.EvaluateAsync<string[]>(errorCheckScript);

            if (errors.Length > 0)
            {
                context.SetVariable("JAVASCRIPT_ERRORS", errors);
                Logger.LogWarning("JavaScript errors detected: {ErrorCount}", errors.Length);

                foreach (var error in errors)
                {
                    Logger.LogWarning("JavaScript error: {Error}", error);
                }
            }
            else
            {
                Logger.LogDebug("No JavaScript errors detected");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to check for JavaScript errors");
        }
    }

    /// <summary>
    ///     Builder class for creating JavaScriptAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<JavaScriptAction, Builder>
    {
        internal string? Script { get; private set; }
        internal object[]? Arguments { get; private set; }
        internal string? ResultVariableName { get; private set; }
        internal bool CheckForErrors { get; private set; }
        internal bool WaitForFunction { get; private set; }
        internal PageWaitForFunctionOptions? WaitOptions { get; private set; }

        /// <summary>
        ///     Sets the JavaScript code to execute.
        /// </summary>
        /// <param name="script">The JavaScript code</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithScript(string script)
        {
            Script = script ?? throw new ArgumentNullException(nameof(script));
            return this;
        }

        /// <summary>
        ///     Sets arguments to pass to the JavaScript function.
        /// </summary>
        /// <param name="arguments">The arguments to pass</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithArguments(params object[] arguments)
        {
            Arguments = arguments;
            return this;
        }

        /// <summary>
        ///     Sets the variable name to store the script result.
        /// </summary>
        /// <param name="variableName">The context variable name</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithResultVariable(string variableName)
        {
            ResultVariableName = variableName;
            return this;
        }

        /// <summary>
        ///     Enables checking for JavaScript errors after script execution.
        /// </summary>
        /// <param name="check">Whether to check for errors</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithErrorChecking(bool check = true)
        {
            CheckForErrors = check;
            return this;
        }

        /// <summary>
        ///     Configures the action to wait for the function to return a truthy value.
        /// </summary>
        /// <param name="wait">Whether to wait for function</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithWaitForFunction(bool wait = true)
        {
            WaitForFunction = wait;
            return this;
        }

        /// <summary>
        ///     Sets wait options for wait for function mode.
        /// </summary>
        /// <param name="options">The wait options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithWaitOptions(PageWaitForFunctionOptions options)
        {
            WaitOptions = options;
            return this;
        }

        /// <summary>
        ///     Sets wait options using a configuration action.
        /// </summary>
        /// <param name="configureOptions">Action to configure wait options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithWaitOptions(Action<WaitOptionsBuilder> configureOptions)
        {
            var builder = new WaitOptionsBuilder();
            configureOptions(builder);
            WaitOptions = builder.Build();
            return this;
        }

        /// <summary>
        ///     Sets a timeout for the JavaScript execution.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(int timeoutMs)
        {
            WaitOptions ??= new PageWaitForFunctionOptions();
            WaitOptions.Timeout = timeoutMs;
            return this;
        }

        /// <summary>
        ///     Sets the polling interval for wait for function mode.
        /// </summary>
        /// <param name="intervalMs">Polling interval in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithPollingInterval(int intervalMs)
        {
            WaitOptions ??= new PageWaitForFunctionOptions();
            WaitOptions.PollingInterval = intervalMs;
            return this;
        }

        /// <summary>
        ///     Convenience method to execute a simple JavaScript statement.
        /// </summary>
        /// <param name="statement">The JavaScript statement</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ExecuteStatement(string statement)
        {
            return WithScript($"(() => {{ {statement} }})()");
        }

        /// <summary>
        ///     Convenience method to execute JavaScript and return a value.
        /// </summary>
        /// <param name="expression">The JavaScript expression</param>
        /// <param name="resultVariable">Variable name to store the result</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder EvaluateExpression(string expression, string? resultVariable = null)
        {
            WithScript($"(() => {{ return {expression}; }})()");
            if (!string.IsNullOrEmpty(resultVariable))
            {
                WithResultVariable(resultVariable);
            }

            return this;
        }

        /// <summary>
        ///     Convenience method to wait for an element to be visible.
        /// </summary>
        /// <param name="selector">The CSS selector</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WaitForElementVisible(string selector, int timeoutMs = 5000)
        {
            var script = $"document.querySelector('{selector}') && " +
                         $"getComputedStyle(document.querySelector('{selector}')).display !== 'none'";

            return WithScript(script)
                .WithWaitForFunction()
                .WithTimeout(timeoutMs);
        }

        /// <summary>
        ///     Convenience method to wait for a condition to be true.
        /// </summary>
        /// <param name="condition">The JavaScript condition</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WaitForCondition(string condition, int timeoutMs = 5000)
        {
            return WithScript(condition)
                .WithWaitForFunction()
                .WithTimeout(timeoutMs);
        }

        /// <summary>
        ///     Builds the JavaScriptAction instance.
        /// </summary>
        /// <returns>A new JavaScriptAction instance</returns>
        public override JavaScriptAction Build()
        {
            if (string.IsNullOrEmpty(Script))
            {
                throw new InvalidOperationException("Script must be specified");
            }

            return new JavaScriptAction(this);
        }
    }

    /// <summary>
    ///     Builder for configuring wait options.
    /// </summary>
    public class WaitOptionsBuilder
    {
        private readonly PageWaitForFunctionOptions _options = new();

        /// <summary>
        ///     Sets the timeout for waiting.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public WaitOptionsBuilder WithTimeout(int timeoutMs)
        {
            _options.Timeout = timeoutMs;
            return this;
        }

        /// <summary>
        ///     Sets the polling interval.
        /// </summary>
        /// <param name="intervalMs">Polling interval in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public WaitOptionsBuilder WithPollingInterval(int intervalMs)
        {
            _options.PollingInterval = intervalMs;
            return this;
        }

        /// <summary>
        ///     Builds the wait options.
        /// </summary>
        /// <returns>The configured wait options</returns>
        internal PageWaitForFunctionOptions Build()
        {
            return _options;
        }
    }
}
