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

using Agenix.Playwright.Endpoint;
using Microsoft.Playwright;
using Moq;

namespace Agenix.Playwright.Tests.Actions;

/// <summary>
///     Base test class for Playwright action tests. Provides common mock setup and utilities.
/// </summary>
public abstract class AbstractPlaywrightActionTestBase : AbstractNUnitSetUp
{
    protected Mock<PlaywrightBrowser> PlaywrightBrowser { get; private set; }
    protected Mock<IPage> Page { get; private set; }
    protected Mock<ILocator> Locator { get; private set; }
    protected Mock<IBrowserContext> BrowserContext { get; private set; }

    protected PlaywrightTestMockSetup MockSetup { get; private set; }

    [SetUp]
    public virtual void SetupPlaywrightMocks()
    {
        // Initialize mocks
        PlaywrightBrowser = new Mock<PlaywrightBrowser>();
        Page = new Mock<IPage>();
        Locator = new Mock<ILocator>();
        BrowserContext = new Mock<IBrowserContext>();

        // Create and apply mock setup
        MockSetup = new PlaywrightTestMockSetup(PlaywrightBrowser, Page, Locator, BrowserContext);
        MockSetup.ApplyDefaultSetup();

        // Allow subclasses to customize the setup
        CustomizeSetup();
    }

    /// <summary>
    ///     Override this method to customize the mock setup for specific test classes.
    /// </summary>
    protected virtual void CustomizeSetup()
    {
        // Default implementation - can be overridden by subclasses
    }

    /// <summary>
    ///     Helper method to create a basic locator setup for most common scenarios.
    /// </summary>
    protected void SetupLocatorForBasicInteraction(bool isVisible = true, bool isEnabled = true)
    {
        MockSetup.SetupLocatorForBasicInteraction(isVisible, isEnabled);
    }

    /// <summary>
    ///     Helper method to setup locator for checkbox/radio button interactions.
    /// </summary>
    protected void SetupLocatorForCheckboxInteraction(bool isChecked = false, bool isVisible = true,
        bool isEnabled = true)
    {
        MockSetup.SetupLocatorForCheckboxInteraction(isChecked, isVisible, isEnabled);
    }
}
