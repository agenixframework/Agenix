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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// Action to load application settings from a specified file path.
public class LoadAppSettingsAction(LoadAppSettingsAction.Builder builder) : AbstractTestActionAsync("load", builder)
{
    /// Logger for LoadAppSettingsAction.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(LoadAppSettingsAction));

    /// File resource path
    private readonly string _filePath = builder.ResourceName;

    /// <summary>
    ///     Executes the core action to load application settings by reading a configuration file,
    ///     resolving dynamic variables, and storing the settings in the test context.
    /// </summary>
    /// <param name="context">The context object that holds variables and manages dynamic content resolution.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous task representing the operation.</returns>
    public override async Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        var resource = await FileUtils.GetFileResourceAsync(_filePath, context);

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Reading config file =>{FileName}", FileUtils.GetFileName(resource.Description));
        }

        var settings = await FileUtils.LoadAsSettings(resource);

        var unresolved = new Dictionary<string, string>();

        foreach (var key in settings.AllKeys)
        {
            var value = settings[key];

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Loading setting: {Key}={Value} into variables", key, value);
            }

            if (Log.IsEnabled(LogLevel.Debug) && context.GetVariables().ContainsKey(key))
            {
                Log.LogDebug("Overwriting setting {Key} old value: {GetVariable} new value: {Value}", key,
                    context.GetVariable(key), value);
            }

            try
            {
                context.SetVariable(key, context.ReplaceDynamicContentInString(value));
            }
            catch (AgenixSystemException)
            {
                unresolved.Add(key, value);
            }
        }

        foreach (var entry in context.ResolveDynamicValuesInMap(unresolved))
        {
            context.SetVariable(entry.Key, entry.Value);
        }

        Log.LogDebug("Loaded config file => {GetFileName}", FileUtils.GetFileName(resource.Description));
    }

    /// Builder for creating and configuring LoadAppSettingsAction instances.
    /// /
    public sealed class Builder : AbstractAsyncTestActionBuilder<IAsyncTestAction, dynamic>
    {
        internal string ResourceName;

        /// Fluent API action building entry method used in C# DSL.
        /// @return
        /// /
        public static Builder Load()
        {
            return new Builder();
        }

        /// Fluent API action building entry method used in C# DSL.
        /// <return>The builder instance for configuring LoadAppSettingsAction.</return>
        public static Builder Load(string resourceName)
        {
            var builder = new Builder();
            builder.WithResourceName(resourceName);
            return builder;
        }

        /// Sets the file path for the LoadAppSettingsAction.
        /// <param name="resourceName">
        ///     The file path to be used for loading the application settings, typically in the form of a string indicating the
        ///     file location or resource path.
        /// </param>
        /// <return>
        ///     The builder instance with the specified file path set, enabling further configuration or building of the action.
        /// </return>
        public Builder WithResourceName(string resourceName)
        {
            ResourceName = resourceName;
            return this;
        }

        /// Builds and returns an instance of LoadAppSettingsAction.
        /// <returns>
        ///     A new instance of LoadAppSettingsAction constructed with the current state of the Builder.
        /// </returns>
        public override LoadAppSettingsAction Build()
        {
            return new LoadAppSettingsAction(this);
        }
    }
}
