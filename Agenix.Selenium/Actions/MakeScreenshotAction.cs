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
using Agenix.Selenium.Endpoint;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Takes a screenshot of the current browser window and saves it to storage.
/// </summary>
public class MakeScreenshotAction : AbstractSeleniumAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(MakeScreenshotAction));

    /// <summary>
    ///     Storage directory to save screenshot to
    /// </summary>
    private readonly string _outputDir;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public MakeScreenshotAction(Builder builder) : base("screenshot", builder)
    {
        _outputDir = builder.OutputDir;
    }

    /// <summary>
    ///     Gets the output directory
    /// </summary>
    public string OutputDir => _outputDir;

    /// <summary>
    /// The file path where the screenshot taken by the browser is stored.
    /// </summary>
    public string ScreenshotPath { get; private set; }

    /// <summary>
    ///     Executes the screenshot action using the provided browser and test context.
    /// </summary>
    /// <param name="browser">The SeleniumBrowser instance used to perform the action.</param>
    /// <param name="context">The TestContext instance containing contextual data for the test.</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        ScreenshotPath = string.Empty;

        if (browser.WebDriver is ITakesScreenshot screenshotDriver)
        {
            try
            {
                var screenshot = screenshotDriver.GetScreenshot();
                ScreenshotPath = SaveScreenshot(screenshot, context);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to take screenshot");
                throw new AgenixSystemException("Failed to take screenshot", ex);
            }
        }
        else
        {
            Logger.LogWarning("Skip screenshot action because web driver is missing screenshot features");
            return;
        }

        if (!string.IsNullOrEmpty(ScreenshotPath))
        {
            var testName = GetTestName(context);
            var fileName = Path.GetFileName(ScreenshotPath);
            var screenshotName = $"{testName}_{fileName}";

            context.SetVariable(SeleniumHeaders.SeleniumScreenshot, screenshotName);

            if (!string.IsNullOrWhiteSpace(_outputDir))
            {
                CopyToOutputDirectory(ScreenshotPath, screenshotName, context);
            }
            else
            {
                // Store in browser's temporary storage
                browser.StoreFile(ScreenshotPath);
            }
        }
    }

    /// <summary>
    ///     Retrieves the test name from the specified test context. If the test name is not set in the context variables, a
    ///     default value of "Test" is returned.
    /// </summary>
    /// <param name="context">The test context containing variables and other execution metadata.</param>
    /// <returns>The name of the test as a string.</returns>
    private static string GetTestName(TestContext context)
    {
        return context.GetVariables().ContainsKey(AgenixSettings.TestNameVariable())
            ? context.GetVariable(AgenixSettings.TestNameVariable())
            : "Test";
    }

    /// <summary>
    ///     Saves a screenshot to a temporary file path with a unique timestamped name.
    /// </summary>
    /// <param name="screenshot">The screenshot to be saved.</param>
    /// <param name="context">The test context that may contain relevant information for saving the screenshot.</param>
    /// <returns>The file path where the screenshot was saved.</returns>
    private static string SaveScreenshot(Screenshot screenshot, TestContext context)
    {
        var tempFileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
        var tempPath = Path.Combine(Path.GetTempPath(), tempFileName);

        screenshot.SaveAsFile(tempPath);
        return tempPath;
    }

    /// <summary>
    ///     Copies a file from the specified source path to a target directory in the output directory.
    /// </summary>
    /// <param name="sourcePath">The full path of the file to be copied.</param>
    /// <param name="fileName">The name of the file to be created in the target directory.</param>
    /// <param name="context">The test execution context, used to resolve the output directory.</param>
    private void CopyToOutputDirectory(string sourcePath, string fileName, TestContext context)
    {
        try
        {
            var resolvedOutputDir = context.ReplaceDynamicContentInString(_outputDir);
            var targetDir = new DirectoryInfo(resolvedOutputDir);

            if (!targetDir.Exists)
            {
                targetDir.Create();
            }

            var targetPath = Path.Combine(targetDir.FullName, fileName);
            File.Copy(sourcePath, targetPath, true);

            Logger.LogDebug("Screenshot saved to: {TargetPath}", targetPath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save screenshot to target storage");
            throw new AgenixSystemException($"Failed to save screenshot to output directory: {_outputDir}", ex);
        }
    }

    /// <summary>
    ///     Action builder for MakeScreenshotAction
    /// </summary>
    public class Builder : Builder<MakeScreenshotAction, Builder>
    {
        public string OutputDir { get; private set; }

        /// <summary>
        ///     Set the output directory for screenshots
        /// </summary>
        public Builder SetOutputDir(string outputDir)
        {
            OutputDir = outputDir;
            return self;
        }

        /// <summary>
        ///     Builds and returns an instance of MakeScreenshotAction.
        /// </summary>
        /// <returns>A new instance of MakeScreenshotAction.</returns>
        public override MakeScreenshotAction Build()
        {
            return new MakeScreenshotAction(this);
        }
    }
}
