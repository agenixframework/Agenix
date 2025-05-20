using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Core.Util;
using log4net;

namespace Agenix.Core.Actions;

/// Action to load application settings from a specified file path.
public class LoadAppSettingsAction(LoadAppSettingsAction.Builder builder) : AbstractTestAction("load", builder)
{
    /// Logger for LoadAppSettingsAction.
    /// /
    private static readonly ILog Log = LogManager.GetLogger(typeof(LoadAppSettingsAction));

    /// File resource path
    private readonly string _filePath = builder._resourceName;

    /// Executes the action defined by the LoadAppSettingsAction.
    /// <param name="context">
    ///     The context in which this action is executed, containing runtime state and variables to be modified.
    /// </param>
    public override void DoExecute(TestContext context)
    {
        var resource = FileUtils.GetFileResource(_filePath, context);

        if (Log.IsDebugEnabled) Log.Debug("Reading config file => " + FileUtils.GetFileName(resource.Description));

        var settings = FileUtils.LoadAsSettings(resource);

        var unresolved = new Dictionary<string, string>();

        foreach (var key in settings.AllKeys)
        {
            var value = settings[key];

            if (Log.IsDebugEnabled) Log.Debug($"Loading setting: {key}={value} into variables");

            if (Log.IsDebugEnabled && context.GetVariables().ContainsKey(key))
                Log.Debug($"Overwriting setting {key} old value: {context.GetVariable(key)} new value: {value}");

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
            context.SetVariable(entry.Key, entry.Value);

        Log.Info($"Loaded config file => {FileUtils.GetFileName(resource.Description)}");
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
        /// The file path to be used for loading the application settings, typically in the form of a string indicating the file location or resource path.
        /// </param>
        /// <return>
        /// The builder instance with the specified file path set, enabling further configuration or building of the action.
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