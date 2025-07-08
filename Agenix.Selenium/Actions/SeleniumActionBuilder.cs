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
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Core.Util;
using Agenix.Selenium.Endpoint;

namespace Agenix.Selenium.Actions;

using System.IO;
using System.Text;

/// <summary>
/// Provides a fluent API for building Selenium-based test actions.
/// </summary>
public class SeleniumActionBuilder : ITestActionBuilder<ISeleniumAction>.IDelegatingTestActionBuilder<ISeleniumAction>
{
    /// <summary>Selenium browser</summary>
    private SeleniumBrowser? _seleniumBrowser;

    private AbstractSeleniumAction.ISeleniumActionBuilder<ISeleniumAction>? _delegateBuilder;

    /// <summary>
    /// Fluent API action building entry method used in C# DSL.
    /// </summary>
    /// <returns></returns>
    public static SeleniumActionBuilder Selenium()
    {
        return new SeleniumActionBuilder();
    }

    /// <summary>
    /// Use a custom selenium browser.
    /// </summary>
    public SeleniumActionBuilder Browser(SeleniumBrowser newSeleniumBrowser)
    {
        _seleniumBrowser = newSeleniumBrowser;
        return this;
    }

    /// <summary>
    /// Start a browser instance.
    /// </summary>
    public StartBrowserAction.Builder Start()
    {
        var builder = new StartBrowserAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Start explicit browser instance.
    /// </summary>
    public StartBrowserAction.Builder Start(SeleniumBrowser seleniumBrowser)
    {
        Browser(seleniumBrowser);
        var builder = new StartBrowserAction.Builder()
            .WithBrowser(seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Stop browser instance.
    /// </summary>
    public StopBrowserAction.Builder Stop()
    {
        var builder = new StopBrowserAction.Builder().WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Stop explicit browser instance.
    /// </summary>
    public StopBrowserAction.Builder Stop(SeleniumBrowser seleniumBrowser)
    {
        Browser(seleniumBrowser);
        var builder = new StopBrowserAction.Builder()
            .WithBrowser(seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Alert element.
    /// </summary>
    public AlertAction.Builder Alert()
    {
        var builder = new AlertAction.Builder().WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Navigate action.
    /// </summary>
    public NavigateAction.Builder Navigate()
    {
        var builder = new NavigateAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Navigate action.
    /// </summary>
    public NavigateAction.Builder Navigate(string page)
    {
        var builder = new NavigateAction.Builder()
            .SetPage(page)
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Finds element.
    /// </summary>
    public FindElementAction.Builder Find()
    {
        var builder = new FindElementAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Dropdown select single option action.
    /// </summary>
    public DropDownSelectAction.Builder Select(string option)
    {
        var builder = new DropDownSelectAction.Builder()
            .SetOption(option)
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Dropdown select multiple options action.
    /// </summary>
    public DropDownSelectAction.Builder Select(params string[] options)
    {
        var builder = new DropDownSelectAction.Builder()
            .SetOptions(options)
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Set input action.
    /// </summary>
    public SetInputAction.Builder SetInput(string value)
    {
        var builder = new SetInputAction.Builder()
            .SetValue(value)
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Set input action.
    /// </summary>
    public SetInputAction.Builder SetInput()
    {
        var builder = new SetInputAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Fill form action.
    /// </summary>
    public FillFormAction.Builder FillForm()
    {
        var builder = new FillFormAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Check input action.
    /// </summary>
    public CheckInputAction.Builder CheckInput()
    {
        var builder = new CheckInputAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Check input action.
    /// </summary>
    public CheckInputAction.Builder CheckInput(bool isChecked)
    {
        var builder = new CheckInputAction.Builder()
            .SetChecked(isChecked)
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Clicks element.
    /// </summary>
    public ClickAction.Builder Click()
    {
        var builder = new ClickAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Hover element.
    /// </summary>
    public HoverAction.Builder Hover()
    {
        var builder = new HoverAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Clear browser cache.
    /// </summary>
    public ClearBrowserCacheAction.Builder ClearCache()
    {
        var builder = new ClearBrowserCacheAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Make screenshot.
    /// </summary>
    public MakeScreenshotAction.Builder Screenshot()
    {
        var builder = new MakeScreenshotAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Make screenshot with custom output directory.
    /// </summary>
    public MakeScreenshotAction.Builder Screenshot(string outputDir)
    {
        var builder = new MakeScreenshotAction.Builder()
            .SetOutputDir(outputDir)
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Store file.
    /// </summary>
    public StoreFileAction.Builder Store()
    {
        var builder = new StoreFileAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Store file.
    /// </summary>
    /// <param name="filePath"></param>
    public StoreFileAction.Builder Store(string filePath)
    {
        var builder = new StoreFileAction.Builder()
            .FilePath(filePath)
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Get stored file.
    /// </summary>
    public GetStoredFileAction.Builder GetStored()
    {
        var builder = new GetStoredFileAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Get stored file.
    /// </summary>
    /// <param name="fileName"></param>
    public GetStoredFileAction.Builder GetStored(string fileName)
    {
        var builder = new GetStoredFileAction.Builder()
            .SetFileName(fileName)
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Wait until element meets condition.
    /// </summary>
    public WaitUntilAction.Builder WaitUntil()
    {
        var builder = new WaitUntilAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Execute JavaScript.
    /// </summary>
    public JavaScriptAction.Builder JavaScript()
    {
        var builder = new JavaScriptAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Execute JavaScript.
    /// </summary>
    public JavaScriptAction.Builder JavaScript(string script)
    {
        var builder = new JavaScriptAction.Builder()
            .SetScript(script)
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Execute JavaScript.
    /// </summary>
    public JavaScriptAction.Builder JavaScript(IResource script)
    {
        return JavaScript(script, Encoding.UTF8);
    }

    /// <summary>
    /// Execute JavaScript.
    /// </summary>
    public JavaScriptAction.Builder JavaScript(IResource scriptResource, Encoding encoding)
    {
        try
        {
            var builder = new JavaScriptAction.Builder()
                .SetScript(FileUtils.ReadToString(scriptResource, encoding))
                .WithBrowser(_seleniumBrowser);
            _delegateBuilder = builder;
            return builder;
        }
        catch (IOException e)
        {
            throw new AgenixSystemException("Failed to read script resource", e);
        }
    }

    /// <summary>
    /// Open window.
    /// </summary>
    public OpenWindowAction.Builder Open()
    {
        var builder = new OpenWindowAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Close window.
    /// </summary>
    public CloseWindowAction.Builder Close()
    {
        var builder = new CloseWindowAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Switch window.
    /// </summary>
    public SwitchWindowAction.Builder Focus()
    {
        var builder = new SwitchWindowAction.Builder()
            .WithBrowser(_seleniumBrowser);
        _delegateBuilder = builder;
        return builder;
    }

    /// <summary>
    /// Switch window.
    /// </summary>
    public SwitchWindowAction.Builder SwitchWindow()
    {
        return Focus();
    }

    /// <summary>
    /// Builds the ISeleniumAction instance configured by the current SeleniumActionBuilder instance.
    /// </summary>
    /// <returns>An instance of ISeleniumAction representing the configured Selenium action.</returns>
    public ISeleniumAction Build()
    {
        ObjectHelper.AssertNotNull(_delegateBuilder, "Missing delegate action to build");
        if (_seleniumBrowser != null)
        {
            _delegateBuilder.WithBrowser(_seleniumBrowser);
        }

        return _delegateBuilder.Build();
    }


    /// <summary>
    /// Gets the delegating test action builder representing the delegate functionality for creating or composing Selenium actions.
    /// </summary>
    public ITestActionBuilder<ISeleniumAction> Delegate { get; }
}
