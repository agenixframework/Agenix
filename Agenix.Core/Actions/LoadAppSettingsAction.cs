#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// Action to load application settings from a specified file path.
public class LoadAppSettingsAction(LoadAppSettingsAction.Builder builder) : AbstractTestAction("load", builder)
{
    /// Logger for LoadAppSettingsAction.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(LoadAppSettingsAction));

    /// File resource path
    private readonly string _filePath = builder._resourceName;

    /// Executes the action defined by the LoadAppSettingsAction.
    /// <param name="context">
    ///     The context in which this action is executed, containing runtime state and variables to be modified.
    /// </param>
    public override void DoExecute(TestContext context)
    {
        var resource = FileUtils.GetFileResource(_filePath, context);

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Reading config file => " + FileUtils.GetFileName(resource.Description));
        }

        var settings = FileUtils.LoadAsSettings(resource);

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
    public sealed class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        internal string _resourceName;

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
            builder.ResourceName(resourceName);
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
        public Builder ResourceName(string resourceName)
        {
            _resourceName = resourceName;
            return this;
        }

        public override LoadAppSettingsAction Build()
        {
            return new LoadAppSettingsAction(this);
        }
    }
}
