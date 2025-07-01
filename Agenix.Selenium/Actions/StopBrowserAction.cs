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
using Agenix.Api.Log;
using Agenix.Selenium.Endpoint;
using Microsoft.Extensions.Logging;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Selenium action for stopping a browser instance.
/// </summary>
public class StopBrowserAction : AbstractSeleniumAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(StopBrowserAction));

    /// <summary>
    ///     Default constructor.
    /// </summary>
    /// <param name="builder">The action builder</param>
    public StopBrowserAction(Builder builder) : base("stop", builder)
    {
    }

    /// <summary>
    ///     Executes the stop browser action.
    /// </summary>
    /// <param name="browser">The Selenium browser instance</param>
    /// <param name="context">The test context</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        Logger.LogInformation("Stopping browser of type {BrowserType}",
            browser.EndpointConfiguration.BrowserType);

        browser.Stop();

        context.GetVariables().Remove(SeleniumHeaders.SeleniumBrowser);
    }

    /// <summary>
    ///     Builder for creating StopBrowserAction instances.
    /// </summary>
    public class Builder : Builder<StopBrowserAction, Builder>
    {
        /// <summary>
        ///     Builds the StopBrowserAction instance.
        /// </summary>
        /// <returns>A configured StopBrowserAction</returns>
        public override StopBrowserAction Build()
        {
            return new StopBrowserAction(this);
        }
    }
}
