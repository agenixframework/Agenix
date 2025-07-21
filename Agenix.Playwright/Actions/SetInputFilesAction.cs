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
///     Playwright action for setting files in file input elements.
///     Supports single and multiple file uploads using Playwright's built-in file handling methods.
///     Provides validation for file existence and proper file input element detection.
/// </summary>
public class SetInputFilesAction : LocatingElementAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(SetInputFilesAction));

    private readonly string[] _filePaths;
    private readonly int? _timeout;
    private readonly bool _validateTheFiles;

    /// <summary>
    ///     Represents an action that sets files in file input elements using Playwright's file handling methods.
    ///     Supports both single and multiple file uploads while providing validation for file existence
    ///     and accurate detection of target file input elements.
    /// </summary>
    public SetInputFilesAction(Builder builder) : this("set-input-files", builder) { }

    /// <summary>
    ///     Represents an action that sets files in file input elements using Playwright's file handling methods.
    /// </summary>
    public SetInputFilesAction(string name, Builder builder) : base(name, builder)
    {
        const string errorMsg = "At least one file path must be provided";
        _filePaths = builder.FilePaths?.ToArray() ??
                     throw new ArgumentException(errorMsg);
        _validateTheFiles = builder.ValidateFiles;
        _timeout = builder.Timeout;

        if (_filePaths.Length == 0)
        {
            throw new ArgumentException(errorMsg, nameof(builder));
        }
    }

    /// <summary>
    ///     Executes the set input files action after locating the element.
    /// </summary>
    /// <param name="locator">The located file input element</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing set input files action with {FileCount} files", _filePaths.Length);

            // Process file paths with dynamic content replacement
            var processedPaths = _filePaths
                .Select(path => context.ReplaceDynamicContentInString(path))
                .ToArray();

            // Validate files if requested
            if (_validateTheFiles)
            {
                ValidateTheFiles(processedPaths);
            }

            // Validate that the element is a file input
            await ValidateFileInput(locator);

            // Set the files
            await SetFiles(locator, processedPaths);

            Logger.LogInformation("Set input files action completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute set input files action");
            throw new AgenixSystemException("Failed to execute set input files action", ex);
        }
    }

    /// <summary>
    ///     Validates that all specified files exist on the file system.
    /// </summary>
    /// <param name="filePaths">The file paths to validate</param>
    private static void ValidateTheFiles(string[] filePaths)
    {
        var missingFiles = filePaths.Where(filePath => !File.Exists(filePath)).ToList();

        if (missingFiles.Count > 0)
        {
            var missingFilesList = string.Join(", ", missingFiles);
            throw new FileNotFoundException($"The following files do not exist: {missingFilesList}");
        }

        Logger.LogDebug("All {FileCount} files validated successfully", filePaths.Length);
    }

    /// <summary>
    ///     Validates that the target element is a file input.
    /// </summary>
    /// <param name="locator">The element locator</param>
    private static async Task ValidateFileInput(ILocator locator)
    {
        try
        {
            var tagName = await locator.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
            var inputType = await locator.GetAttributeAsync("type");

            if (tagName != "input" || inputType?.ToLower() != "file")
            {
                throw new InvalidOperationException(
                    $"Element is not a file input. Found tag: {tagName}, type: {inputType}");
            }

            Logger.LogDebug("File input element validated successfully");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Failed to validate file input element", ex);
        }
    }

    /// <summary>
    ///     Sets the files in the file input element.
    /// </summary>
    /// <param name="locator">The file input element locator</param>
    /// <param name="filePaths">The file paths to set</param>
    private async Task SetFiles(ILocator locator, string[] filePaths)
    {
        var options = new LocatorSetInputFilesOptions { Timeout = _timeout };

        Logger.LogDebug("Setting {FileCount} files: {Files}",
            filePaths.Length,
            string.Join(", ", filePaths.Select(Path.GetFileName)));

        await locator.SetInputFilesAsync(filePaths, options);

        Logger.LogDebug("Files set successfully");
    }

    /// <summary>
    ///     Builder class for creating SetInputFilesAction instances with fluent API.
    /// </summary>
    public new class Builder : LocatingElementAction.Builder
    {
        internal List<string>? FilePaths { get; private set; }
        internal bool ValidateFiles { get; private set; } = true;
        internal int? Timeout { get; private set; }

        /// <summary>
        ///     Sets a single file path.
        /// </summary>
        /// <param name="filePath">The file path to upload</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFile(string filePath)
        {
            FilePaths = [filePath ?? throw new ArgumentNullException(nameof(filePath))];
            return this;
        }

        /// <summary>
        ///     Sets multiple file paths.
        /// </summary>
        /// <param name="filePaths">The file paths to upload</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFiles(params string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
            {
                throw new ArgumentException("At least one file path must be provided", nameof(filePaths));
            }

            FilePaths = new List<string>(filePaths);
            return this;
        }

        /// <summary>
        ///     Sets multiple file paths from a collection.
        /// </summary>
        /// <param name="filePaths">The file paths to upload</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFiles(IEnumerable<string> filePaths)
        {
            ArgumentNullException.ThrowIfNull(filePaths);

            var pathList = filePaths.ToList();

            if (pathList.Count == 0)
            {
                throw new ArgumentException("At least one file path must be provided", nameof(filePaths));
            }

            FilePaths = pathList;
            return this;
        }

        /// <summary>
        ///     Adds a file path to the existing list.
        /// </summary>
        /// <param name="filePath">The file path to add</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder AddFile(string filePath)
        {
            FilePaths ??= [];
            FilePaths.Add(filePath ?? throw new ArgumentNullException(nameof(filePath)));
            return this;
        }

        /// <summary>
        ///     Clears all files (useful for clearing file input).
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder ClearFiles()
        {
            FilePaths = [];
            return this;
        }

        /// <summary>
        ///     Sets whether to validate that files exist before uploading.
        /// </summary>
        /// <param name="validate">True to validate file existence, false to skip validation</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithFileValidation(bool validate = true)
        {
            ValidateFiles = validate;
            return this;
        }

        /// <summary>
        ///     Skips file existence validation.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SkipFileValidation()
        {
            ValidateFiles = false;
            return this;
        }

        /// <summary>
        ///     Sets the timeout for the file upload operation.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(int timeoutMs)
        {
            Timeout = timeoutMs;
            return this;
        }


        /// <summary>
        ///     Builds the SetInputFilesAction instance.
        /// </summary>
        /// <returns>A new SetInputFilesAction instance</returns>
        public override SetInputFilesAction Build()
        {
            if (FilePaths == null || FilePaths.Count == 0)
            {
                throw new InvalidOperationException("At least one file path must be specified for SetInputFilesAction");
            }

            return new SetInputFilesAction(this);
        }
    }
}
