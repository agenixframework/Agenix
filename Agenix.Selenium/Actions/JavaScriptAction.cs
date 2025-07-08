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

namespace Agenix.Selenium.Actions;

/// <summary>
///     Executes JavaScript code on current page and validates errors.
/// </summary>
public class JavaScriptAction : AbstractSeleniumAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(JavaScriptAction));

    /// <summary>
    ///     Optional arguments for the script
    /// </summary>
    private readonly List<object> _arguments;

    /// <summary>
    ///     JavaScript errors to validate
    /// </summary>
    private readonly List<string> _expectedErrors;

    /// <summary>
    ///     JavaScript code to execute
    /// </summary>
    private readonly string _script;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public JavaScriptAction(Builder builder) : base("javascript", builder)
    {
        _script = builder.Script;
        _arguments = builder.Arguments.ToList();
        _expectedErrors = builder.ExpectedErrors.ToList();
    }

    /// <summary>
    ///     Gets the JavaScript script
    /// </summary>
    public string Script => _script;

    /// <summary>
    ///     Gets the script arguments
    /// </summary>
    public IReadOnlyList<object> Arguments => _arguments.AsReadOnly();

    /// <summary>
    ///     Gets the expected JavaScript errors
    /// </summary>
    public IReadOnlyList<string> ExpectedErrors => _expectedErrors.AsReadOnly();

    /// <summary>
    ///     Executes a JavaScript code snippet on the provided Selenium browser instance within the given test context.
    /// </summary>
    /// <param name="browser">The Selenium browser object containing the WebDriver instance.</param>
    /// <param name="context">The test context used to resolve dynamic values for script execution.</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        try
        {
            if (browser.WebDriver is IJavaScriptExecutor jsExecutor)
            {
                var resolvedScript = context.ReplaceDynamicContentInString(_script);
                var resolvedArguments = context.ResolveDynamicValuesInArray(_arguments.ToArray());

                jsExecutor.ExecuteScript(resolvedScript, resolvedArguments);

                // Check for JavaScript errors
                var errors = GetJavaScriptErrors(jsExecutor);
                context.SetVariable(SeleniumHeaders.SeleniumJsErrors, errors);

                // Validate expected errors
                ValidateExpectedErrors(errors);
            }
            else
            {
                Logger.LogWarning("Skip JavaScript action because web driver is missing JavaScript features");
            }
        }
        catch (WebDriverException ex)
        {
            throw new AgenixSystemException("Failed to execute JavaScript code", ex);
        }
    }

    /// <summary>
    ///     Retrieves JavaScript errors from the browser's current execution context.
    /// </summary>
    /// <param name="jsExecutor">An instance of <see cref="IJavaScriptExecutor" /> used to execute JavaScript.</param>
    /// <returns>A list of JavaScript error messages collected from the browser.</returns>
    private static List<string> GetJavaScriptErrors(IJavaScriptExecutor jsExecutor)
    {
        var errors = new List<string>();

        try
        {
            if (jsExecutor.ExecuteScript("return window._selenide_jsErrors") is IList<string> errorObjects)
            {
                errors.AddRange(errorObjects.Select(error => error?.ToString() ?? string.Empty));
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Could not retrieve JavaScript errors");
        }

        return errors;
    }

    /// <summary>
    ///     Validates that the actual JavaScript errors match the expected errors.
    /// </summary>
    /// <param name="actualErrors">A list of JavaScript errors encountered during script execution.</param>
    private void ValidateExpectedErrors(List<string> actualErrors)
    {
        foreach (var expectedError in _expectedErrors.Where(expectedError => !actualErrors.Contains(expectedError)))
        {
            throw new ValidationException($"Missing JavaScript error: {expectedError}");
        }
    }

    /// <summary>
    ///     Action builder for JavaScriptAction
    /// </summary>
    public class Builder : Builder<JavaScriptAction, Builder>
    {
        public string Script { get; private set; }
        public List<object> Arguments { get; } = [];
        public List<string> ExpectedErrors { get; } = [];

        /// <summary>
        ///     Set the JavaScript script to execute
        /// </summary>
        public Builder SetScript(string script)
        {
            Script = script;
            return self;
        }

        /// <summary>
        ///     Add script arguments
        /// </summary>
        public Builder SetArguments(params object[] args)
        {
            return SetArguments(args.ToList());
        }

        /// <summary>
        ///     Add script arguments
        /// </summary>
        public Builder SetArguments(IEnumerable<object> args)
        {
            Arguments.AddRange(args);
            return self;
        }

        /// <summary>
        ///     Add single script argument
        /// </summary>
        public Builder AddArgument(object arg)
        {
            Arguments.Add(arg);
            return self;
        }

        /// <summary>
        ///     Add expected JavaScript errors
        /// </summary>
        public Builder SetExpectedErrors(params string[] errors)
        {
            return SetExpectedErrors(errors.ToList());
        }

        /// <summary>
        ///     Add expected JavaScript errors
        /// </summary>
        public Builder SetExpectedErrors(IEnumerable<string> errors)
        {
            ExpectedErrors.AddRange(errors);
            return self;
        }

        /// <summary>
        ///     Add a single expected error
        /// </summary>
        public Builder AddExpectedError(string error)
        {
            ExpectedErrors.Add(error);
            return self;
        }

        /// <summary>
        ///     Constructs and returns an instance of the JavaScriptAction class.
        /// </summary>
        /// <returns>A new instance of JavaScriptAction.</returns>
        public override JavaScriptAction Build()
        {
            return new JavaScriptAction(this);
        }
    }
}
