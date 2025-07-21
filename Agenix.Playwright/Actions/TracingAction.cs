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
///     Playwright action for controlling tracing operations using context.Tracing API.
///     Supports starting and stopping traces, grouping operations, and capturing browser operations and network activity
///     for debugging.
/// </summary>
public class TracingAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of tracing operations.
    /// </summary>
    public enum TracingOperation
    {
        /// <summary>
        ///     Start tracing using Tracing.StartAsync().
        /// </summary>
        START,

        /// <summary>
        ///     Start a new tracing chunk using Tracing.StartChunkAsync().
        /// </summary>
        START_CHUNK,

        /// <summary>
        ///     Stop tracing using Tracing.StopAsync().
        /// </summary>
        STOP,

        /// <summary>
        ///     Stop the current tracing chunk using Tracing.StopChunkAsync().
        /// </summary>
        STOP_CHUNK,

        /// <summary>
        ///     Start a tracing group using Tracing.GroupAsync().
        /// </summary>
        GROUP,

        /// <summary>
        ///     End the current tracing group using Tracing.GroupEndAsync().
        /// </summary>
        GROUP_END
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(TracingAction));
    private readonly string? _name;

    private readonly TracingOperation _operation;
    private readonly string? _path;
    private readonly bool _screenshots;
    private readonly bool _snapshots;
    private readonly bool _sources;
    private readonly string? _title;

    /// <summary>
    ///     Represents a Playwright action for controlling tracing operations using context.Tracing API.
    /// </summary>
    public TracingAction(Builder builder) : this("tracing", builder)
    {
    }

    /// <summary>
    ///     Playwright action for controlling tracing operations using context.Tracing API.
    ///     Supports starting and stopping traces, grouping operations, and capturing browser operations and network activity
    ///     for debugging.
    /// </summary>
    public TracingAction(string name, Builder builder) : base(name, builder)
    {
        _operation = builder.Operation;
        _name = builder.TraceName;
        _title = builder.Title;
        _screenshots = builder.Screenshots;
        _snapshots = builder.Snapshots;
        _sources = builder.Sources;
        _path = builder.Path;
    }

    /// <summary>
    ///     Executes the tracing action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing tracing action: {Operation}", _operation);

            var browserContext = browser.GetCurrentContext();
            if (browserContext == null)
            {
                throw new InvalidOperationException("No active browser context available for tracing");
            }

            switch (_operation)
            {
                case TracingOperation.START:
                    await ExecuteStart(browserContext, context);
                    break;

                case TracingOperation.START_CHUNK:
                    await ExecuteStartChunk(browserContext, context);
                    break;

                case TracingOperation.STOP:
                    await ExecuteStop(browserContext, context);
                    break;

                case TracingOperation.STOP_CHUNK:
                    await ExecuteStopChunk(browserContext, context);
                    break;

                case TracingOperation.GROUP:
                    await ExecuteGroup(browserContext, context);
                    break;

                case TracingOperation.GROUP_END:
                    await ExecuteGroupEnd(browserContext);
                    break;

                default:
                    throw new ArgumentException($"Unsupported tracing operation: {_operation}");
            }

            Logger.LogInformation("Tracing action completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute tracing action");
            throw new AgenixSystemException("Failed to execute tracing action", ex);
        }
    }

    /// <summary>
    ///     Executes start tracing operation using context.Tracing.StartAsync().
    /// </summary>
    private async Task ExecuteStart(IBrowserContext browserContext, TestContext context)
    {
        var options = new TracingStartOptions
        {
            Screenshots = _screenshots,
            Snapshots = _snapshots,
            Sources = _sources
        };

        if (!string.IsNullOrEmpty(_name))
        {
            options.Name = context.ReplaceDynamicContentInString(_name);
        }

        if (!string.IsNullOrEmpty(_title))
        {
            options.Title = context.ReplaceDynamicContentInString(_title);
        }

        await browserContext.Tracing.StartAsync(options);
        Logger.LogDebug(
            "Started tracing with name: {Name}, title: {Title}, screenshots: {Screenshots}, snapshots: {Snapshots}, sources: {Sources}",
            options.Name, options.Title, _screenshots, _snapshots, _sources);
    }

    /// <summary>
    ///     Executes start chunk tracing operation using context.Tracing.StartChunkAsync().
    /// </summary>
    private async Task ExecuteStartChunk(IBrowserContext browserContext, TestContext context)
    {
        var options = new TracingStartChunkOptions();

        if (!string.IsNullOrEmpty(_name))
        {
            options.Name = context.ReplaceDynamicContentInString(_name);
        }

        if (!string.IsNullOrEmpty(_title))
        {
            options.Title = context.ReplaceDynamicContentInString(_title);
        }

        await browserContext.Tracing.StartChunkAsync(options);
        Logger.LogDebug("Started tracing chunk with name: {Name}, title: {Title}", options.Name, options.Title);
    }

    /// <summary>
    ///     Executes stop tracing operation using context.Tracing.StopAsync().
    /// </summary>
    private async Task ExecuteStop(IBrowserContext browserContext, TestContext context)
    {
        var options = new TracingStopOptions();

        if (!string.IsNullOrEmpty(_path))
        {
            options.Path = context.ReplaceDynamicContentInString(_path);
        }

        await browserContext.Tracing.StopAsync(options);
        Logger.LogDebug("Stopped tracing and saved to path: {Path}", options.Path);
    }

    /// <summary>
    ///     Executes stop chunk tracing operation using context.Tracing.StopChunkAsync().
    /// </summary>
    private async Task ExecuteStopChunk(IBrowserContext browserContext, TestContext context)
    {
        var options = new TracingStopChunkOptions();

        if (!string.IsNullOrEmpty(_path))
        {
            options.Path = context.ReplaceDynamicContentInString(_path);
        }

        await browserContext.Tracing.StopChunkAsync(options);
        Logger.LogDebug("Stopped tracing chunk and saved to path: {Path}", options.Path);
    }

    /// <summary>
    ///     Executes a group tracing operation using context.Tracing.GroupAsync().
    /// </summary>
    private async Task ExecuteGroup(IBrowserContext browserContext, TestContext context)
    {
        if (string.IsNullOrEmpty(_name))
        {
            throw new InvalidOperationException("Group name must be specified for group operation");
        }

        var groupName = context.ReplaceDynamicContentInString(_name);
        await browserContext.Tracing.GroupAsync(groupName);
        Logger.LogDebug("Started tracing group: {GroupName}", groupName);
    }

    /// <summary>
    ///     Executes group end tracing operation using context.Tracing.GroupEndAsync().
    /// </summary>
    private static async Task ExecuteGroupEnd(IBrowserContext browserContext)
    {
        await browserContext.Tracing.GroupEndAsync();
        Logger.LogDebug("Ended tracing group");
    }

    /// <summary>
    ///     Builder class for creating TracingAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<TracingAction, Builder>
    {
        internal TracingOperation Operation { get; private set; }
        internal string? TraceName { get; private set; }
        internal string? Title { get; private set; }
        internal bool Screenshots { get; private set; } = true;
        internal bool Snapshots { get; private set; } = true;
        internal bool Sources { get; private set; } = true;
        internal string? Path { get; private set; }

        /// <summary>
        ///     Configures the action to start tracing using Tracing.StartAsync().
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Start()
        {
            Operation = TracingOperation.START;
            return this;
        }

        /// <summary>
        ///     Configures the action to start a new tracing chunk using Tracing.StartChunkAsync().
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder StartChunk()
        {
            Operation = TracingOperation.START_CHUNK;
            return this;
        }

        /// <summary>
        ///     Configures the action to stop tracing using Tracing.StopAsync().
        /// </summary>
        /// <param name="path">Path where to save the trace file</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Stop(string? path = null)
        {
            Operation = TracingOperation.STOP;
            if (!string.IsNullOrEmpty(path))
            {
                Path = path;
            }

            return this;
        }

        /// <summary>
        ///     Configures the action to stop the curre nt tracing chunk using Tracing.StopChunkAsync().
        /// </summary>
        /// <param name="path">Path where to save the trace chunk file</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder StopChunk(string? path = null)
        {
            Operation = TracingOperation.STOP_CHUNK;
            if (!string.IsNullOrEmpty(path))
            {
                Path = path;
            }

            return this;
        }

        /// <summary>
        ///     Configures the action to start a tracing group using Tracing.GroupAsync().
        /// </summary>
        /// <param name="groupName">Name of the tracing group</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Group(string groupName)
        {
            Operation = TracingOperation.GROUP;
            TraceName = groupName;
            return this;
        }

        /// <summary>
        ///     Configures the action to end current tracing group using Tracing.GroupEndAsync().
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GroupEnd()
        {
            Operation = TracingOperation.GROUP_END;
            return this;
        }

        /// <summary>
        ///     Sets the name for the trace.
        /// </summary>
        /// <param name="name">The trace name</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithName(string name)
        {
            TraceName = name;
            return this;
        }

        /// <summary>
        ///     Sets the title for the trace.
        /// </summary>
        /// <param name="title">The trace title</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        ///     Sets whether to capture screenshots during tracing.
        /// </summary>
        /// <param name="capture">True to capture screenshots, false otherwise</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithScreenshots(bool capture = true)
        {
            Screenshots = capture;
            return this;
        }

        /// <summary>
        ///     Sets whether to capture DOM snapshots during tracing.
        /// </summary>
        /// <param name="capture">True to capture snapshots, false otherwise</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSnapshots(bool capture = true)
        {
            Snapshots = capture;
            return this;
        }

        /// <summary>
        ///     Sets whether to include source code in tracing.
        /// </summary>
        /// <param name="include">True to include sources, false otherwise</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithSources(bool include = true)
        {
            Sources = include;
            return this;
        }

        /// <summary>
        ///     Sets the output path for the trace file.
        /// </summary>
        /// <param name="filePath">Path where to save the trace file</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithPath(string filePath)
        {
            Path = filePath;
            return this;
        }

        /// <summary>
        ///     Convenience method to disable all capture options for minimal tracing.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder MinimalTracing()
        {
            Screenshots = false;
            Snapshots = false;
            Sources = false;
            return this;
        }

        /// <summary>
        ///     Convenience method to enable all capture options for full tracing.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder FullTracing()
        {
            Screenshots = true;
            Snapshots = true;
            Sources = true;
            return this;
        }

        /// <summary>
        ///     Builds the TracingAction instance.
        /// </summary>
        /// <returns>A new TracingAction instance</returns>
        public override TracingAction Build()
        {
            return new TracingAction(this);
        }
    }
}
