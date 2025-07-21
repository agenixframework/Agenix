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
///     Helper class for setting up Playwright mocks consistently across tests.
/// </summary>
public class PlaywrightTestMockSetup
{
    public const string DefaultContextId = "default-context";
    public const string DefaultPageId = "default-page";
    private readonly Mock<IBrowserContext> _browserContext;
    private readonly Mock<ILocator> _locator;
    private readonly Mock<IPage> _page;
    private readonly Mock<PlaywrightBrowser> _playwrightBrowser;

    public PlaywrightTestMockSetup(
        Mock<PlaywrightBrowser> playwrightBrowser,
        Mock<IPage> page,
        Mock<ILocator> locator,
        Mock<IBrowserContext> browserContext)
    {
        _playwrightBrowser = playwrightBrowser;
        _page = page;
        _locator = locator;
        _browserContext = browserContext;
    }

    /// <summary>
    ///     Applies the default mock setup that works for most Playwright action tests.
    /// </summary>
    public void ApplyDefaultSetup()
    {
        ResetMocks();
        SetupBrowserContexts();
        SetupBrowserPages();
        SetupBrowserNavigation();
        SetupPageLocator();
        SetupBrowserState();
    }

    /// <summary>
    ///     Resets all mocks to their default state.
    /// </summary>
    public void ResetMocks()
    {
        _page.Reset();
        _locator.Reset();
        _browserContext.Reset();
        _playwrightBrowser.Reset();
    }

    /// <summary>
    ///     Sets up browser context-related mocks.
    /// </summary>
    public void SetupBrowserContexts()
    {
        var contextIds = new List<string> { DefaultContextId }.AsReadOnly();
        _playwrightBrowser.Setup(x => x.ContextIds).Returns(contextIds);
        _playwrightBrowser.Setup(x => x.CurrentContextId).Returns(DefaultContextId);
        _playwrightBrowser.Setup(x => x.GetContext(DefaultContextId)).Returns(_browserContext.Object);
        _playwrightBrowser.Setup(x => x.SwitchToContext(It.IsAny<string>()));

        // Add this line to fix the issue
        _playwrightBrowser.Setup(x => x.GetCurrentContext()).Returns(_browserContext.Object);
    }

    /// <summary>
    ///     Sets up browser page-related mocks.
    /// </summary>
    public void SetupBrowserPages()
    {
        var pageIds = new List<string> { DefaultPageId }.AsReadOnly();
        _playwrightBrowser.Setup(x => x.PageIds).Returns(pageIds);
        _playwrightBrowser.Setup(x => x.CurrentPageId).Returns(DefaultPageId);
        _playwrightBrowser.Setup(x => x.GetCurrentPage()).Returns(_page.Object);
        _playwrightBrowser.Setup(x => x.GetPage(DefaultPageId)).Returns(_page.Object);
        _playwrightBrowser.Setup(x => x.SwitchToPage(It.IsAny<string>()));

        // Setup GetPagesInContext to return a non-null dictionary
        var pagesInContext = new Dictionary<string, IPage> { { DefaultPageId, _page.Object } };
        _playwrightBrowser.Setup(x => x.GetPagesInContext(DefaultContextId)).Returns(pagesInContext);
        _playwrightBrowser.Setup(x => x.GetPagesInContext(It.IsAny<string>())).Returns(pagesInContext);
    }

    /// <summary>
    ///     Sets up browser navigation-related mocks.
    /// </summary>
    public void SetupBrowserNavigation()
    {
        _playwrightBrowser.Setup(x => x.CreatePageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(DefaultPageId);
    }

    /// <summary>
    ///     Sets up page locator mocks.
    /// </summary>
    public void SetupPageLocator()
    {
        _page.Setup(x => x.Locator(It.IsAny<string>(), It.IsAny<PageLocatorOptions>()))
            .Returns(_locator.Object);
    }

    /// <summary>
    ///     Sets up browser state mocks.
    /// </summary>
    public void SetupBrowserState()
    {
        _playwrightBrowser.Setup(x => x.IsStarted).Returns(true);
    }

    /// <summary>
    ///     Sets up locator for basic element interactions (click, type, etc.).
    /// </summary>
    public void SetupLocatorForBasicInteraction(bool isVisible = true, bool isEnabled = true)
    {
        _locator.Setup(x => x.IsVisibleAsync(It.IsAny<LocatorIsVisibleOptions>())).ReturnsAsync(isVisible);
        _locator.Setup(x => x.IsEnabledAsync(It.IsAny<LocatorIsEnabledOptions>())).ReturnsAsync(isEnabled);

        // Setup basic actions
        _locator.Setup(x => x.ClickAsync(It.IsAny<LocatorClickOptions>())).Returns(Task.CompletedTask);
        _locator.Setup(x => x.FillAsync(It.IsAny<string>(), It.IsAny<LocatorFillOptions>()))
            .Returns(Task.CompletedTask);
        _locator.Setup(x => x.ClearAsync(It.IsAny<LocatorClearOptions>())).Returns(Task.CompletedTask);
        _locator.Setup(x => x.TypeAsync(It.IsAny<string>(), It.IsAny<LocatorTypeOptions>()))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    ///     Sets up locator for checkbox/radio button interactions.
    /// </summary>
    public void SetupLocatorForCheckboxInteraction(bool isChecked = false, bool isVisible = true, bool isEnabled = true)
    {
        SetupLocatorForBasicInteraction(isVisible, isEnabled);

        _locator.Setup(x => x.IsCheckedAsync(It.IsAny<LocatorIsCheckedOptions>())).ReturnsAsync(isChecked);
        _locator.Setup(x => x.CheckAsync(It.IsAny<LocatorCheckOptions>())).Returns(Task.CompletedTask);
        _locator.Setup(x => x.UncheckAsync(It.IsAny<LocatorUncheckOptions>())).Returns(Task.CompletedTask);
    }

    /// <summary>
    ///     Sets up locator for select/dropdown interactions.
    /// </summary>
    public void SetupLocatorForSelectInteraction(string[] selectedOptions = null, bool isVisible = true,
        bool isEnabled = true)
    {
        SetupLocatorForBasicInteraction(isVisible, isEnabled);

        selectedOptions ??= [];
        IReadOnlyList<string> readOnlySelectedOptions = selectedOptions.AsReadOnly();

        _locator.Setup(x => x.SelectOptionAsync(It.IsAny<string>(), It.IsAny<LocatorSelectOptionOptions>()))
            .Returns(Task.FromResult(readOnlySelectedOptions));
        _locator
            .Setup(x => x.SelectOptionAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<LocatorSelectOptionOptions>()))
            .Returns(Task.FromResult(readOnlySelectedOptions));
    }

    /// <summary>
    ///     Sets up locator to return specific text content.
    /// </summary>
    public void SetupLocatorTextContent(string textContent)
    {
        _locator.Setup(x => x.TextContentAsync(It.IsAny<LocatorTextContentOptions>())).ReturnsAsync(textContent);
        _locator.Setup(x => x.InnerTextAsync(It.IsAny<LocatorInnerTextOptions>())).ReturnsAsync(textContent);
    }

    /// <summary>
    ///     Sets up locator to return specific attribute value.
    /// </summary>
    public void SetupLocatorAttribute(string attributeName, string attributeValue)
    {
        _locator.Setup(x => x.GetAttributeAsync(attributeName, It.IsAny<LocatorGetAttributeOptions>()))
            .ReturnsAsync(attributeValue);
    }

    /// <summary>
    ///     Sets up locator to simulate element not found.
    /// </summary>
    public void SetupLocatorNotFound()
    {
        _locator.Setup(x => x.IsVisibleAsync(It.IsAny<LocatorIsVisibleOptions>())).ReturnsAsync(false);
        _locator.Setup(x => x.IsEnabledAsync(It.IsAny<LocatorIsEnabledOptions>())).ReturnsAsync(false);
    }

    /// <summary>
    ///     Sets up browser with custom context and page IDs.
    /// </summary>
    /// <summary>
    ///     Sets up browser with custom context and page IDs.
    /// </summary>
    public void SetupCustomContextAndPage(string contextId, string pageId)
    {
        var contextIds = new List<string> { contextId }.AsReadOnly();
        var pageIds = new List<string> { pageId }.AsReadOnly();

        _playwrightBrowser.Setup(x => x.ContextIds).Returns(contextIds);
        _playwrightBrowser.Setup(x => x.PageIds).Returns(pageIds);
        _playwrightBrowser.Setup(x => x.CurrentContextId).Returns(contextId);
        _playwrightBrowser.Setup(x => x.CurrentPageId).Returns(pageId);
        _playwrightBrowser.Setup(x => x.GetContext(contextId)).Returns(_browserContext.Object);
        _playwrightBrowser.Setup(x => x.GetPage(pageId)).Returns(_page.Object);
        _playwrightBrowser.Setup(x => x.GetCurrentPage()).Returns(_page.Object);

        // Add the missing setups based on the invocation log
        _playwrightBrowser.Setup(x => x.SwitchToContext(contextId));
        _playwrightBrowser.Setup(x => x.SwitchToPage(pageId));

        // Setup GetPagesInContext for the custom context
        var pagesInContext = new Dictionary<string, IPage> { { pageId, _page.Object } };
        _playwrightBrowser.Setup(x => x.GetPagesInContext(contextId)).Returns(pagesInContext);

        // Setup page locator
        _page.Setup(x => x.Locator(It.IsAny<string>(), It.IsAny<PageLocatorOptions>()))
            .Returns(_locator.Object);

        // Setup browser state
        _playwrightBrowser.Setup(x => x.IsStarted).Returns(true);
    }
}
