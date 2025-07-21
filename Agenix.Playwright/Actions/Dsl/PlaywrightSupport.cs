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

using System.ComponentModel;
using Agenix.Playwright.Endpoint;

namespace Agenix.Playwright.Actions.Dsl;

/// <summary>
///     Provides static support methods for initializing and working with Playwright-based test actions.
/// </summary>
public static class PlaywrightSupport
{
    private static PlaywrightActionBuilder? _playwrightActionBuilder;
    private static PlaywrightBrowserBuilder? _playwrightBrowserBuilder;

    /// <summary>
    ///     Provides a fluent entry point to create and configure Playwright-based test actions.
    /// </summary>
    /// <returns>
    ///     A singleton instance of <see cref="PlaywrightActionBuilder" /> for building Playwright test actions.
    /// </returns>
    public static PlaywrightActionBuilder Playwright()
    {
        if (_playwrightActionBuilder != null)
        {
            return _playwrightActionBuilder;
        }

        var newInstance = PlaywrightActionBuilder.Playwright();
        Interlocked.CompareExchange(ref _playwrightActionBuilder, newInstance, null);
        return _playwrightActionBuilder;
    }

    /// <summary>
    ///     Creates a new Playwright browser configuration builder.
    /// </summary>
    /// <returns>A singleton instance of PlaywrightBrowserBuilder for configuring browsers.</returns>
    public static PlaywrightBrowserBuilder Browser()
    {
        if (_playwrightBrowserBuilder != null)
        {
            return _playwrightBrowserBuilder;
        }

        var newInstance = new PlaywrightBrowserBuilder();
        Interlocked.CompareExchange(ref _playwrightBrowserBuilder, newInstance, null);
        return _playwrightBrowserBuilder;
    }

    /// <summary>
    ///     Creates a builder for starting a test session with isolation.
    /// </summary>
    /// <returns>A StartTestSessionAction builder</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Description("EXPERIMENTAL: Test isolation features - subject to breaking changes")]
    public static StartTestSessionAction.Builder StartTestSession()
    {
        return Playwright().StartTestSession();
    }

    /// <summary>
    ///     Creates a builder for starting a test session with a specific browser.
    /// </summary>
    /// <param name="playwrightBrowser">The Playwright browser</param>
    /// <returns>A StartTestSessionAction builder</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Description("EXPERIMENTAL: Test isolation features - subject to breaking changes")]
    public static StartTestSessionAction.Builder StartTestSession(PlaywrightBrowser playwrightBrowser)
    {
        return Playwright().StartTestSession(playwrightBrowser);
    }

    /// <summary>
    ///     Creates a builder for ending a test session.
    /// </summary>
    /// <returns>An EndTestSessionAction builder</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Description("EXPERIMENTAL: Test isolation features - subject to breaking changes")]
    public static EndTestSessionAction.Builder EndTestSession()
    {
        return Playwright().EndTestSession();
    }

    /// <summary>
    ///     Creates a builder for ending a test session with a specific browser.
    /// </summary>
    /// <param name="playwrightBrowser">The Playwright browser</param>
    /// <returns>An EndTestSessionAction builder</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Description("EXPERIMENTAL: Test isolation features - subject to breaking changes")]
    public static EndTestSessionAction.Builder EndTestSession(PlaywrightBrowser playwrightBrowser)
    {
        return Playwright().EndTestSession(playwrightBrowser);
    }

    /// <summary>
    ///     Resets the singleton instances, forcing them to be recreated on next access.
    ///     This is useful for testing scenarios or when you need fresh instances.
    /// </summary>
    public static void Reset()
    {
        Interlocked.Exchange(ref _playwrightActionBuilder, null);
        Interlocked.Exchange(ref _playwrightBrowserBuilder, null);
    }
}
