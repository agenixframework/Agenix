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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Selenium.Endpoint;
using Microsoft.Extensions.Logging;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Abstract base class for all Selenium actions.
///     Provides common functionality for executing Selenium browser commands and managing browser instances.
/// </summary>
public abstract class AbstractSeleniumAction : AbstractTestAction, ISeleniumAction
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(AbstractSeleniumAction));

    /// <summary>
    ///     Selenium browser instance
    /// </summary>
    private readonly SeleniumBrowser? _browser;

    protected AbstractSeleniumAction(ISeleniumActionBuilder<ISeleniumAction> builder) : this(builder.GetName(), builder)
    {
    }


    /// <summary>
    ///     Initializes a new instance of the AbstractSeleniumAction class
    /// </summary>
    /// <param name="name">The action name</param>
    /// <param name="builder">The builder instance containing configuration</param>
    protected AbstractSeleniumAction(string name, ISeleniumActionBuilder<ISeleniumAction> builder)
        : base("selenium:" + name, builder.GetDescription())
    {
        _browser = builder.Browser;
    }

    /// <summary>
    ///     Gets the Selenium browser instance
    /// </summary>
    /// <returns>The SeleniumBrowser instance or null if not set</returns>
    public SeleniumBrowser Browser => _browser;

    /// <summary>
    ///     Executes the Selenium action within the provided test context
    /// </summary>
    /// <param name="context">The test context</param>
    public override void DoExecute(TestContext context)
    {
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebug("Executing Selenium browser command '{ActionName}'", Name);
        }

        var browserToUse = _browser;
        if (browserToUse == null)
        {
            if (context.GetVariables().ContainsKey(SeleniumHeaders.SeleniumBrowser))
            {
                var browserReference = context.GetVariable(SeleniumHeaders.SeleniumBrowser);
                browserToUse = context.ReferenceResolver.Resolve<SeleniumBrowser>(browserReference);
            }
            else
            {
                throw new AgenixSystemException("Failed to get active browser instance, " +
                                                "either set explicit browser for action or start browser instance");
            }
        }

        Execute(browserToUse, context);

        Logger.LogInformation("Selenium browser command execution successful: '{ActionName}'", Name);
    }

    /// <summary>
    ///     Abstract method to be implemented by concrete Selenium actions
    /// </summary>
    /// <param name="browser">The Selenium browser instance to use</param>
    /// <param name="context">The test context</param>
    protected abstract void Execute(SeleniumBrowser browser, TestContext context);

    /// <summary>
    /// Interface for building Selenium-based test actions.
    /// Provides methods for configuring and describing Selenium actions,
    /// including setting custom browsers and retrieving metadata such as names and descriptions.
    /// </summary>
    public interface ISeleniumActionBuilder<out T> : ITestActionBuilder<T> where T : ISeleniumAction
    {
        /// <summary>
        ///     Gets the browser instance to use for the action
        /// </summary>
        SeleniumBrowser? Browser { get; }

        /// <summary>
        ///     Sets a custom Selenium browser for the action
        /// </summary>
        /// <param name="seleniumBrowser">The Selenium browser instance</param>
        /// <returns>This builder instance for method chaining</returns>
        ISeleniumActionBuilder<T> WithBrowser(SeleniumBrowser seleniumBrowser);

        /// <summary>
        ///     Gets the name of the test action
        /// </summary>
        /// <returns>The name of the test action</returns>
        string GetName();

        /// <summary>
        ///     Gets the description of the test action
        /// </summary>
        /// <returns>The description of the test action</returns>
        string GetDescription();
    }


    /// <summary>
    ///     Abstract builder class for Selenium actions
    /// </summary>
    /// <typeparam name="T">The type of Selenium action being built</typeparam>
    /// <typeparam name="TB">The type of builder (for fluent interface)</typeparam>
    public abstract class Builder<T, TB> : AbstractTestActionBuilder<T, TB>, ISeleniumActionBuilder<T> where T : ISeleniumAction
        where TB : Builder<T, TB>
    {
        /// <summary>
        ///     The browser instance to use for the action
        /// </summary>
        public SeleniumBrowser? Browser { get; private set; }

        /// <summary>
        ///     Sets a custom Selenium browser for the action
        /// </summary>
        /// <param name="seleniumBrowser">The Selenium browser instance</param>
        /// <returns>This builder instance for method chaining</returns>
        public virtual TB WithBrowser(SeleniumBrowser seleniumBrowser)
        {
            Browser = seleniumBrowser;
            return self;
        }

        // Explicit interface implementation to return the interface type
        ISeleniumActionBuilder<T> ISeleniumActionBuilder<T>.WithBrowser(SeleniumBrowser seleniumBrowser)
        {
            return WithBrowser(seleniumBrowser);
        }

        public abstract override T Build();

    }
}
