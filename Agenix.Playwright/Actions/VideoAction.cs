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
///     Playwright action for working with video recordings using page.Video() API.
///     Supports saving videos, getting video paths, and deleting video files.
/// </summary>
public class VideoAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of video operations.
    /// </summary>
    public enum VideoOperation
    {
        /// <summary>
        ///     Save video to a specified path using Video.SaveAsAsync().
        /// </summary>
        SAVE_AS,

        /// <summary>
        ///     Get the current video file path using Video.PathAsync().
        /// </summary>
        GET_PATH,

        /// <summary>
        ///     Delete a video file using Video.DeleteAsync().
        /// </summary>
        DELETE
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(VideoAction));

    private readonly VideoOperation _operation;
    private readonly string? _path;

    /// <summary>
    ///     Represents a Playwright action for working with video recordings using page.Video() API.
    /// </summary>
    public VideoAction(Builder builder) : this("video", builder)
    {
    }

    /// <summary>
    ///     Playwright action for working with video recordings using page.Video() API.
    ///     Supports saving videos, getting video paths, and deleting video files.
    /// </summary>
    public VideoAction(string name, Builder builder) : base(name, builder)
    {
        _operation = builder.Operation;
        _path = builder.Path;
    }

    /// <summary>
    ///     Executes the video action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing video action: {Operation}", _operation);

            var page = browser.GetCurrentPage();
            if (page == null)
            {
                throw new InvalidOperationException("No active page available");
            }

            var video = page.Video;
            if (video == null)
            {
                throw new InvalidOperationException(
                    "No video recording available. Make sure video recording is enabled in browser context options.");
            }

            switch (_operation)
            {
                case VideoOperation.SAVE_AS:
                    await ExecuteSaveAs(video, context);
                    break;

                case VideoOperation.GET_PATH:
                    await ExecuteGetPath(video, context);
                    break;

                case VideoOperation.DELETE:
                    await ExecuteDelete(video);
                    break;

                default:
                    throw new ArgumentException($"Unsupported video operation: {_operation}");
            }

            Logger.LogInformation("Video action completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute video action");
            throw new AgenixSystemException("Failed to execute video action", ex);
        }
    }

    /// <summary>
    ///     Executes save video operation using video.SaveAsAsync().
    /// </summary>
    private async Task ExecuteSaveAs(IVideo video, TestContext context)
    {
        if (string.IsNullOrEmpty(_path))
        {
            throw new InvalidOperationException("Path must be specified for SaveAs operation");
        }

        var savePath = context.ReplaceDynamicContentInString(_path);
        await video.SaveAsAsync(savePath ?? throw new InvalidOperationException("The save path is null or empty"));

        // Store the path in context for potential use by other actions
        context.SetVariable(PlaywrightHeaders.PlaywrightVideoSavedPath, savePath);

        Logger.LogDebug("Saved video to path: {Path}", savePath);
    }

    /// <summary>
    ///     Executes get video path operation using video.PathAsync().
    /// </summary>
    private static async Task ExecuteGetPath(IVideo video, TestContext context)
    {
        var videoPath = await video.PathAsync();

        // Store the path in context for potential use by other actions
        context.SetVariable(PlaywrightHeaders.PlaywrightVideoCurrentPath, videoPath);

        Logger.LogDebug("Retrieved video path: {Path}", videoPath);
    }

    /// <summary>
    ///     Executes delete video operation using video.DeleteAsync().
    /// </summary>
    private static async Task ExecuteDelete(IVideo video)
    {
        await video.DeleteAsync();

        Logger.LogDebug("Deleted video recording");
    }

    /// <summary>
    ///     Builder class for creating VideoAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<VideoAction, Builder>
    {
        internal VideoOperation Operation { get; private set; }
        internal string? Path { get; private set; }

        /// <summary>
        ///     Configures the action to save video using Video.SaveAsAsync().
        /// </summary>
        /// <param name="filePath">Path where to save the video file</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SaveAs(string filePath)
        {
            Operation = VideoOperation.SAVE_AS;
            Path = filePath;
            return this;
        }

        /// <summary>
        ///     Configures the action to get a video path using Video.PathAsync().
        ///     The path will be stored in the context variable 'video.currentPath'.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GetPath()
        {
            Operation = VideoOperation.GET_PATH;
            return this;
        }

        /// <summary>
        ///     Configures the action to delete video using Video.DeleteAsync().
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Delete()
        {
            Operation = VideoOperation.DELETE;
            return this;
        }

        /// <summary>
        ///     Sets the file path for save operations.
        /// </summary>
        /// <param name="filePath">Path where to save the video file</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithPath(string filePath)
        {
            Path = filePath;
            return this;
        }

        /// <summary>
        ///     Builds the VideoAction instance.
        /// </summary>
        /// <returns>A new VideoAction instance</returns>
        public override VideoAction Build()
        {
            return new VideoAction(this);
        }
    }
}
