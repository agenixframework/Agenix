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
using Agenix.Core.Util;
using Agenix.Playwright.Endpoint;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action builder provides a fluent API for constructing Playwright actions.
///     This builder follows the same pattern as SeleniumActionBuilder, providing convenient
///     methods for creating various Playwright automation actions with method chaining.
/// </summary>
public class
    PlaywrightActionBuilder : IAsyncTestActionBuilder<IPlaywrightAction>.IDelegatingTestActionBuilder<IPlaywrightAction>
{
    /// <summary>
    ///     Gets or sets the delegate test action builder.
    /// </summary>
    private AbstractPlaywrightAction.IPlaywrightActionBuilder<IPlaywrightAction>? _delegateBuilder;

    private PlaywrightBrowser? _playwrightBrowser;

    /// <summary>
    ///     Builds the final Playwright action.
    /// </summary>
    /// <returns>The built Playwright action</returns>
    public IPlaywrightAction Build()
    {
        ObjectHelper.AssertNotNull(_delegateBuilder, "Missing delegate action to build");
        if (_playwrightBrowser != null)
        {
            _delegateBuilder.WithBrowser(_playwrightBrowser);
        }

        return _delegateBuilder.Build();
    }


    /// <summary>
    ///     Gets the delegating test action builder representing the delegate functionality for creating or composing
    ///     playwright actions.
    /// </summary>
    public IAsyncTestActionBuilder<IPlaywrightAction> Delegate { get; }

    /// <summary>
    ///     Creates a new instance of PlaywrightActionBuilder.
    /// </summary>
    /// <returns>A new PlaywrightActionBuilder instance</returns>
    public static PlaywrightActionBuilder Playwright()
    {
        return new PlaywrightActionBuilder();
    }

    /// <summary>
    ///     Sets the Playwright browser instance to use for actions.
    /// </summary>
    /// <param name="newPlaywrightBrowser">The Playwright browser instance</param>
    /// <returns>The builder instance for method chaining</returns>
    public PlaywrightActionBuilder Browser(PlaywrightBrowser newPlaywrightBrowser)
    {
        _playwrightBrowser = newPlaywrightBrowser;
        return this;
    }

    /// <summary>
    ///     Creates a builder for starting a Playwright browser.
    /// </summary>
    /// <returns>A StartBrowserAction builder</returns>
    public StartBrowserAction.Builder Start()
    {
        var builder = new StartBrowserAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for starting a specific Playwright browser.
    /// </summary>
    /// <param name="playwrightBrowser">The Playwright browser to start</param>
    /// <returns>A StartBrowserAction builder</returns>
    public StartBrowserAction.Builder Start(PlaywrightBrowser playwrightBrowser)
    {
        Browser(playwrightBrowser); // (re)set the browser (if any)
        var builder = new StartBrowserAction.Builder().WithBrowser(playwrightBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for stopping a Playwright browser.
    /// </summary>
    /// <returns>A StopBrowserAction builder</returns>
    public StopBrowserAction.Builder Stop()
    {
        var builder = new StopBrowserAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for stopping a specific Playwright browser.
    /// </summary>
    /// <param name="playwrightBrowser">The Playwright browser to stop</param>
    /// <returns>A StopBrowserAction builder</returns>
    public StopBrowserAction.Builder Stop(PlaywrightBrowser playwrightBrowser)
    {
        var builder = new StopBrowserAction.Builder().WithBrowser(playwrightBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for navigating to a URL.
    /// </summary>
    /// <returns>A NavigateAction builder</returns>
    public NavigateAction.Builder Navigate()
    {
        var builder = new NavigateAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for navigating to a specific URL.
    /// </summary>
    /// <param name="url">The URL to navigate to</param>
    /// <returns>A NavigateAction builder</returns>
    public NavigateAction.Builder Navigate(string url)
    {
        var builder = Navigate();
        builder.WithUrl(url);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder by clicking elements.
    /// </summary>
    /// <returns>A ClickAction builder</returns>
    public ClickAction.Builder Click()
    {
        var builder = new ClickAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for double-clicking elements.
    /// </summary>
    /// <returns>A DoubleClickAction builder</returns>
    public DoubleClickAction.Builder DoubleClick()
    {
        var builder = new DoubleClickAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for hovering over elements.
    /// </summary>
    /// <returns>A HoverAction builder</returns>
    public HoverAction.Builder Hover()
    {
        var builder = new HoverAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Initializes a builder for an action expecting multiple locators.
    /// </summary>
    /// <returns>An instance of ExpectMultipleLocatorsAction.Builder.</returns>
    public ExpectMultipleLocatorsAction.Builder ExpectMultipleLocators()
    {
        var builder = new ExpectMultipleLocatorsAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a new instance of ExpectLocatorAction.Builder.
    /// </summary>
    /// <returns>An instance of ExpectLocatorAction.Builder configured with the current browser context.</returns>
    public ExpectLocatorAction.Builder ExpectLocator()
    {
        var builder = new ExpectLocatorAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a new instance of the ExpectPageAction builder.
    /// </summary>
    /// <returns>An ExpectPageAction.Builder instance to configure an ExpectPageAction.</returns>
    public ExpectPageAction.Builder ExpectPage()
    {
        var builder = new ExpectPageAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for configuring and validating page state expectations.
    /// </summary>
    /// <returns>An instance of ExpectPageStateAction.Builder</returns>
    public ExpectPageStateAction.Builder ExpectPageState()
    {
        var builder = new ExpectPageStateAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for focusing on elements.
    /// </summary>
    /// <returns>A FocusAction builder</returns>
    public FocusAction.Builder Focus()
    {
        var builder = new FocusAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for setting input values.
    /// </summary>
    /// <returns>A SetInputAction builder</returns>
    public SetInputAction.Builder SetInput()
    {
        var builder = new SetInputAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for setting input values with specific value.
    /// </summary>
    /// <param name="value">The value to set</param>
    /// <returns>A SetInputAction builder</returns>
    public SetInputAction.Builder SetInput(string value)
    {
        var builder = SetInput();
        builder.Fill(value);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for checking/unchecking inputs.
    /// </summary>
    /// <returns>A CheckInputAction builder</returns>
    public CheckInputAction.Builder CheckInput()
    {
        var builder = new CheckInputAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for checking/unchecking inputs with a specific state.
    /// </summary>
    /// <param name="isChecked">Whether to check or uncheck</param>
    /// <returns>A CheckInputAction builder</returns>
    public CheckInputAction.Builder CheckInput(bool isChecked)
    {
        var builder = CheckInput();
        builder.SetChecked(isChecked);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for selecting dropdown options.
    /// </summary>
    /// <returns>A DropdownSelectAction builder</returns>
    public DropdownSelectAction.Builder Select()
    {
        var builder = new DropdownSelectAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for selecting a specific option from dropdown.
    /// </summary>
    /// <param name="option">The option to select</param>
    /// <returns>A DropdownSelectAction builder</returns>
    public DropdownSelectAction.Builder Select(string option)
    {
        var builder = Select();
        builder.SelectByValue(option);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for selecting multiple options from dropdown.
    /// </summary>
    /// <param name="options">The options to select</param>
    /// <returns>A DropdownSelectAction builder</returns>
    public DropdownSelectAction.Builder Select(params string[] options)
    {
        var builder = Select();
        builder.SelectByValue(options);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for setting input files.
    /// </summary>
    /// <returns>A SetInputFilesAction builder</returns>
    public SetInputFilesAction.Builder SetFiles()
    {
        var builder = new SetInputFilesAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for filling forms.
    /// </summary>
    /// <returns>A FillFormAction builder</returns>
    public FillFormAction.Builder FillForm()
    {
        var builder = new FillFormAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for taking screenshots.
    /// </summary>
    /// <returns>A MakeScreenshotAction builder</returns>
    public MakeScreenshotAction.Builder Screenshot()
    {
        var builder = new MakeScreenshotAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for taking screenshots with specific output directory.
    /// </summary>
    /// <param name="outputDir">The output directory for screenshots</param>
    /// <returns>A MakeScreenshotAction builder</returns>
    public MakeScreenshotAction.Builder Screenshot(string outputDir)
    {
        var builder = Screenshot();
        builder.WithOutputDirectory(outputDir);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for executing JavaScript.
    /// </summary>
    /// <returns>A JavaScriptAction builder</returns>
    public JavaScriptAction.Builder JavaScript()
    {
        var builder = new JavaScriptAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for executing specific JavaScript code.
    /// </summary>
    /// <param name="script">The JavaScript code to execute</param>
    /// <returns>A JavaScriptAction builder</returns>
    public JavaScriptAction.Builder JavaScript(string script)
    {
        var builder = JavaScript();
        builder.WithScript(script);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for keyboard actions.
    /// </summary>
    /// <returns>A KeyboardAction builder</returns>
    public KeyboardAction.Builder Keyboard()
    {
        var builder = new KeyboardAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for mouse actions.
    /// </summary>
    /// <returns>A MouseAction builder</returns>
    public MouseAction.Builder Mouse()
    {
        var builder = new MouseAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for dialog actions.
    /// </summary>
    /// <returns>A DialogAction builder</returns>
    public DialogAction.Builder Dialog()
    {
        var builder = new DialogAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for opening windows.
    /// </summary>
    /// <returns>An OpenWindowAction builder</returns>
    public OpenWindowAction.Builder OpenWindow()
    {
        var builder = new OpenWindowAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for closing windows.
    /// </summary>
    /// <returns>A CloseWindowAction builder</returns>
    public CloseWindowAction.Builder CloseWindow()
    {
        var builder = new CloseWindowAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for frame locator actions.
    /// </summary>
    /// <returns>A FrameLocatorAction builder</returns>
    public FrameLocatorAction.Builder Frame()
    {
        var builder = new FrameLocatorAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for frame locator actions with a specific frame selector.
    /// </summary>
    /// <param name="frameSelector">The frame selector</param>
    /// <returns>A FrameLocatorAction builder</returns>
    public FrameLocatorAction.Builder Frame(string frameSelector)
    {
        var builder = Frame();
        builder.WithFrame(frameSelector);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for clearing browser cache.
    /// </summary>
    /// <returns>A ClearBrowserCacheAction builder</returns>
    public ClearBrowserCacheAction.Builder ClearCache()
    {
        var builder = new ClearBrowserCacheAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for video actions.
    /// </summary>
    /// <returns>A VideoAction builder</returns>
    public VideoAction.Builder Video()
    {
        var builder = new VideoAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for configuring a tracing action in Playwright.
    /// </summary>
    /// <returns>A new instance of TracingAction.Builder that allows configuration of tracing behavior.</returns>
    public TracingAction.Builder Tracing()
    {
        var builder = new TracingAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for starting a test session with isolation.
    /// </summary>
    /// <returns>A StartTestSessionAction builder</returns>
    public StartTestSessionAction.Builder StartTestSession()
    {
        var builder = new StartTestSessionAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for starting a test session with a specific browser.
    /// </summary>
    /// <param name="playwrightBrowser">The Playwright browser</param>
    /// <returns>A StartTestSessionAction builder</returns>
    public StartTestSessionAction.Builder StartTestSession(PlaywrightBrowser playwrightBrowser)
    {
        var builder = new StartTestSessionAction.Builder().WithBrowser(playwrightBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for ending a test session.
    /// </summary>
    /// <returns>An EndTestSessionAction builder</returns>
    public EndTestSessionAction.Builder EndTestSession()
    {
        var builder = new EndTestSessionAction.Builder();
        if (_playwrightBrowser != null)
        {
            builder.WithBrowser(_playwrightBrowser);
        }

        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    ///     Creates a builder for ending a test session with a specific browser.
    /// </summary>
    /// <param name="playwrightBrowser">The Playwright browser</param>
    /// <returns>An EndTestSessionAction builder</returns>
    public EndTestSessionAction.Builder EndTestSession(PlaywrightBrowser playwrightBrowser)
    {
        var builder = new EndTestSessionAction.Builder().WithBrowser(playwrightBrowser);
        _delegateBuilder = builder;
        return builder;
    }
}
