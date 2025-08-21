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
///     Playwright action for taking screenshots of web pages or specific elements.
///     Supports full page screenshots, element screenshots, and various image formats.
/// </summary>
public class MakeScreenshotAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of screenshot targets.
    /// </summary>
    public enum ScreenshotTarget
    {
        /// <summary>
        ///     Take a screenshot of the entire page.
        /// </summary>
        PAGE,

        /// <summary>
        ///     Take a screenshot of a specific element.
        /// </summary>
        ELEMENT
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(MakeScreenshotAction));
    private readonly string? _contextVariableName;
    private readonly LocatorScreenshotOptions? _elementOptions;
    private readonly string? _elementSelector;
    private readonly string? _fileName;
    private readonly string? _outputDirectory;

    private readonly string? _outputPath;
    private readonly PageScreenshotOptions? _pageOptions;
    private readonly bool _storeInContext;
    private readonly ScreenshotTarget _target;

    /// <summary>
    ///     Represents a Playwright action for capturing screenshots of web pages or specific elements.
    ///     Supports various screenshot configurations, including full page, element-specific, and different image formats.
    /// </summary>
    public MakeScreenshotAction(Builder builder) : this("screenshot", builder)
    {
    }

    /// <summary>
    ///     Playwright action for taking screenshots of web pages or specific elements.
    ///     Supports full page screenshots, element screenshots, and various image formats.
    /// </summary>
    public MakeScreenshotAction(string name, Builder builder) : base(name, builder)
    {
        _outputPath = builder.OutputPath;
        _outputDirectory = builder.OutputDirectory;
        _fileName = builder.FileName;
        _pageOptions = builder.PageOptions;
        _elementOptions = builder.ElementOptions;
        _target = builder.Target;
        _elementSelector = builder.ElementSelector;
        _storeInContext = builder.StoreInContext;
        _contextVariableName = builder.ContextVariableName;
    }

    /// <summary>
    ///     Executes the screenshot action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Taking screenshot with target: {Target}", _target);

            var page = browser.GetCurrentPage();
            if (page == null)
            {
                throw new InvalidOperationException("No active page available");
            }

            byte[] screenshotBytes;

            switch (_target)
            {
                case ScreenshotTarget.PAGE:
                    screenshotBytes = await TakePageScreenshot(page);
                    break;

                case ScreenshotTarget.ELEMENT:
                    screenshotBytes = await TakeElementScreenshot(page);
                    break;

                default:
                    throw new ArgumentException($"Unsupported screenshot target: {_target}");
            }

            // Save screenshot to file if a path is specified
            if (!string.IsNullOrEmpty(_outputPath) || !string.IsNullOrEmpty(_fileName))
            {
                var filePath = await SaveScreenshotToFile(screenshotBytes, context);
                context.SetVariable(PlaywrightHeaders.PlaywrightScreenshotPath, filePath);
                Logger.LogInformation("Screenshot saved to: {FilePath}", filePath);
            }

            // Store screenshot bytes in context if requested
            if (_storeInContext)
            {
                var variableName = _contextVariableName ?? PlaywrightHeaders.PlaywrightScreenshotBytes;
                context.SetVariable(variableName, screenshotBytes);
                Logger.LogDebug("Screenshot bytes stored in context variable: {VariableName}", variableName);
            }

            Logger.LogInformation("Screenshot taken successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to take screenshot");
            throw new AgenixSystemException("Failed to take screenshot", ex);
        }
    }

    /// <summary>
    ///     Takes a screenshot of the entire page.
    /// </summary>
    /// <param name="page">The page instance</param>
    /// <returns>Screenshot bytes</returns>
    private async Task<byte[]> TakePageScreenshot(IPage page)
    {
        Logger.LogDebug("Taking page screenshot");
        return await page.ScreenshotAsync(_pageOptions);
    }

    /// <summary>
    ///     Takes a screenshot of a specific element.
    /// </summary>
    /// <param name="page">The page instance</param>
    /// <returns>Screenshot bytes</returns>
    private async Task<byte[]> TakeElementScreenshot(IPage page)
    {
        if (string.IsNullOrEmpty(_elementSelector))
        {
            throw new InvalidOperationException("Element selector must be specified for element screenshots");
        }

        Logger.LogDebug("Taking element screenshot for selector: {Selector}", _elementSelector);

        var locator = page.Locator(_elementSelector);
        return await locator.ScreenshotAsync(_elementOptions);
    }

    /// <summary>
    ///     Saves the screenshot bytes to a file.
    /// </summary>
    /// <param name="screenshotBytes">The screenshot data</param>
    /// <param name="context">The test context</param>
    /// <returns>The full path where the screenshot was saved</returns>
    private async Task<string> SaveScreenshotToFile(byte[] screenshotBytes, TestContext context)
    {
        string? filePath = null;

        if (!string.IsNullOrEmpty(_outputPath))
        {
            // Use the explicitly provided path
            filePath = context.ReplaceDynamicContentInString(_outputPath);
        }
        else
        {
            // Build a path from directory and filename
            var directory = string.IsNullOrEmpty(_outputDirectory)
                ? Environment.CurrentDirectory
                : context.ReplaceDynamicContentInString(_outputDirectory);

            var fileName = string.IsNullOrEmpty(_fileName)
                ? $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                : context.ReplaceDynamicContentInString(_fileName);

            if (directory != null && fileName != null)
            {
                filePath = Path.Combine(directory, fileName);
            }
        }

        // Ensure directory exists
        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Logger.LogDebug("Created directory: {Directory}", directoryPath);
        }

        // Save the screenshot
        await File.WriteAllBytesAsync(filePath ?? throw new InvalidOperationException("The file path is null"),
            screenshotBytes);

        return Path.GetFullPath(filePath);
    }

    /// <summary>
    ///     Builder class for creating MakeScreenshotAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<MakeScreenshotAction, Builder>
    {
        internal string? OutputPath { get; private set; }
        internal string? OutputDirectory { get; private set; }
        internal string? FileName { get; private set; }
        internal PageScreenshotOptions? PageOptions { get; private set; }
        internal LocatorScreenshotOptions? ElementOptions { get; private set; }
        internal ScreenshotTarget Target { get; private set; } = ScreenshotTarget.PAGE;
        internal string? ElementSelector { get; private set; }
        internal bool StoreInContext { get; private set; }
        internal string? ContextVariableName { get; private set; }

        /// <summary>
        ///     Sets the complete output path for the screenshot.
        /// </summary>
        /// <param name="path">The full file path</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithOutputPath(string path)
        {
            OutputPath = path;
            return this;
        }

        /// <summary>
        ///     Sets the output directory for the screenshot.
        /// </summary>
        /// <param name="directory">The directory path</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithOutputDirectory(string directory)
        {
            OutputDirectory = directory;
            return this;
        }

        /// <summary>
        ///     Sets the filename for the screenshot.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFileName(string fileName)
        {
            FileName = fileName;
            return this;
        }

        /// <summary>
        ///     Configures the action to take a page screenshot.
        /// </summary>
        /// <param name="options">Page screenshot options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder TakePageScreenshot(PageScreenshotOptions? options = null)
        {
            Target = ScreenshotTarget.PAGE;
            PageOptions = options;
            return this;
        }

        /// <summary>
        ///     Configures the action to take a page screenshot with custom options.
        /// </summary>
        /// <param name="configureOptions">Action to configure page options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder TakePageScreenshot(Action<PageScreenshotOptionsBuilder> configureOptions)
        {
            var builder = new PageScreenshotOptionsBuilder();
            configureOptions(builder);
            return TakePageScreenshot(builder.Build());
        }

        /// <summary>
        ///     Configures the action to take an element screenshot.
        /// </summary>
        /// <param name="selector">The element selector</param>
        /// <param name="options">Element screenshot options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder TakeElementScreenshot(string selector, LocatorScreenshotOptions? options = null)
        {
            Target = ScreenshotTarget.ELEMENT;
            ElementSelector = selector;
            ElementOptions = options;
            return this;
        }

        /// <summary>
        ///     Configures the action to take an element screenshot with custom options.
        /// </summary>
        /// <param name="selector">The element selector</param>
        /// <param name="configureOptions">Action to configure element options</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder TakeElementScreenshot(string selector, Action<ElementScreenshotOptionsBuilder> configureOptions)
        {
            var builder = new ElementScreenshotOptionsBuilder();
            configureOptions(builder);
            return TakeElementScreenshot(selector, builder.Build());
        }

        /// <summary>
        ///     Stores the screenshot bytes in a context variable.
        /// </summary>
        /// <param name="variableName">The context variable name (optional)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithStoreInContext(string? variableName = null)
        {
            StoreInContext = true;
            ContextVariableName = variableName;
            return this;
        }

        /// <summary>
        ///     Sets the image format to PNG.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithPngFormat()
        {
            PageOptions ??= new PageScreenshotOptions();
            PageOptions.Type = ScreenshotType.Png;

            ElementOptions ??= new LocatorScreenshotOptions();
            ElementOptions.Type = ScreenshotType.Png;

            return this;
        }

        /// <summary>
        ///     Sets the image format to JPEG with optional quality.
        /// </summary>
        /// <param name="quality">JPEG quality (0-100)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithJpegFormat(int? quality = null)
        {
            PageOptions ??= new PageScreenshotOptions();
            PageOptions.Type = ScreenshotType.Jpeg;
            if (quality.HasValue)
            {
                PageOptions.Quality = quality.Value;
            }

            ElementOptions ??= new LocatorScreenshotOptions();
            ElementOptions.Type = ScreenshotType.Jpeg;
            if (quality.HasValue)
            {
                ElementOptions.Quality = quality.Value;
            }

            return this;
        }

        /// <summary>
        ///     Sets whether to take a full page screenshot.
        /// </summary>
        /// <param name="fullPage">Whether to capture the full scrollable page</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFullPage(bool fullPage = true)
        {
            PageOptions ??= new PageScreenshotOptions();
            PageOptions.FullPage = fullPage;
            return this;
        }

        /// <summary>
        ///     Builds the MakeScreenshotAction instance.
        /// </summary>
        /// <returns>A new MakeScreenshotAction instance</returns>
        public override MakeScreenshotAction Build()
        {
            return new MakeScreenshotAction(this);
        }
    }

    /// <summary>
    ///     Builder for configuring page screenshot options.
    /// </summary>
    public class PageScreenshotOptionsBuilder
    {
        private readonly PageScreenshotOptions _options = new();

        /// <summary>
        ///     Sets the screenshot format.
        /// </summary>
        /// <param name="type">The screenshot type</param>
        /// <returns>The builder instance for method chaining</returns>
        public PageScreenshotOptionsBuilder WithType(ScreenshotType type)
        {
            _options.Type = type;
            return this;
        }

        /// <summary>
        ///     Sets the JPEG quality.
        /// </summary>
        /// <param name="quality">Quality (0-100)</param>
        /// <returns>The builder instance for method chaining</returns>
        public PageScreenshotOptionsBuilder WithQuality(int quality)
        {
            _options.Quality = quality;
            return this;
        }

        /// <summary>
        ///     Sets whether to capture the full scrollable page.
        /// </summary>
        /// <param name="fullPage">Whether to capture full page</param>
        /// <returns>The builder instance for method chaining</returns>
        public PageScreenshotOptionsBuilder WithFullPage(bool fullPage = true)
        {
            _options.FullPage = fullPage;
            return this;
        }

        /// <summary>
        ///     Sets a specific clip area.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>The builder instance for method chaining</returns>
        public PageScreenshotOptionsBuilder WithClip(float x, float y, float width, float height)
        {
            _options.Clip = new Clip { X = x, Y = y, Width = width, Height = height };
            return this;
        }

        /// <summary>
        ///     Sets animations handling.
        /// </summary>
        /// <param name="animations">How to handle animations</param>
        /// <returns>The builder instance for method chaining</returns>
        public PageScreenshotOptionsBuilder WithAnimations(ScreenshotAnimations animations)
        {
            _options.Animations = animations;
            return this;
        }

        /// <summary>
        ///     Builds the page screenshot options.
        /// </summary>
        /// <returns>The configured options</returns>
        internal PageScreenshotOptions Build()
        {
            return _options;
        }
    }

    /// <summary>
    ///     Builder for configuring element screenshot options.
    /// </summary>
    public class ElementScreenshotOptionsBuilder
    {
        private readonly LocatorScreenshotOptions _options = new();

        /// <summary>
        ///     Sets the screenshot format.
        /// </summary>
        /// <param name="type">The screenshot type</param>
        /// <returns>The builder instance for method chaining</returns>
        public ElementScreenshotOptionsBuilder WithType(ScreenshotType type)
        {
            _options.Type = type;
            return this;
        }

        /// <summary>
        ///     Sets the JPEG quality.
        /// </summary>
        /// <param name="quality">Quality (0-100)</param>
        /// <returns>The builder instance for method chaining</returns>
        public ElementScreenshotOptionsBuilder WithQuality(int quality)
        {
            _options.Quality = quality;
            return this;
        }

        /// <summary>
        ///     Sets animations handling.
        /// </summary>
        /// <param name="animations">How to handle animations</param>
        /// <returns>The builder instance for method chaining</returns>
        public ElementScreenshotOptionsBuilder WithAnimations(ScreenshotAnimations animations)
        {
            _options.Animations = animations;
            return this;
        }

        /// <summary>
        ///     Sets whether to omit the background.
        /// </summary>
        /// <param name="omitBackground">Whether to omit background</param>
        /// <returns>The builder instance for method chaining</returns>
        public ElementScreenshotOptionsBuilder WithOmitBackground(bool omitBackground = true)
        {
            _options.OmitBackground = omitBackground;
            return this;
        }

        /// <summary>
        ///     Builds the element screenshot options.
        /// </summary>
        /// <returns>The configured options</returns>
        internal LocatorScreenshotOptions Build()
        {
            return _options;
        }
    }
}
