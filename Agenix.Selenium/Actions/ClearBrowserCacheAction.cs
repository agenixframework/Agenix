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
using Agenix.Selenium.Endpoint;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Clear browser cache action that deletes all cookies from the browser.
/// </summary>
public class ClearBrowserCacheAction : AbstractSeleniumAction
{
    /// <summary>
    ///     Default constructor
    /// </summary>
    public ClearBrowserCacheAction(Builder builder) : base("clear-cache", builder)
    {
    }

    /// <summary>
    ///     Executes the action to clear all cookies using the Selenium browser instance.
    /// </summary>
    /// <param name="browser">The Selenium browser instance used for executing the action.</param>
    /// <param name="context">The testing context associated with the Selenium action.</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        browser.WebDriver.Manage().Cookies.DeleteAllCookies();
    }

    /// <summary>
    ///     Action builder for ClearBrowserCacheAction
    /// </summary>
    public class Builder : Builder<ClearBrowserCacheAction, Builder>
    {
        /// <summary>
        ///     Constructs and returns an instance of the ClearBrowserCacheAction class using the specified builder.
        /// </summary>
        /// <returns>A newly created instance of ClearBrowserCacheAction.</returns>
        public override ClearBrowserCacheAction Build()
        {
            return new ClearBrowserCacheAction(this);
        }
    }
}
