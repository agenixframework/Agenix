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

using System.Configuration;
using Agenix.Configuration.Core;
using Agenix.Configuration.Core.Models;
using ConfigurationExtensions = Agenix.Configuration.Extensions.ConfigurationExtensions;

namespace Agenix.Configuration.Singleton;

using System.Collections.Concurrent;

/// <summary>
/// High-performance singleton configuration manager for Agenix ATF.
/// Provides lazy initialization with thread-safe access to configuration objects.
/// Supports both JSON and YAML configuration formats.
/// </summary>
/// <typeparam name="T">The type of the configuration object</typeparam>
public sealed record ConfigurationSingleton<T> where T : class, new()
{
    private static readonly Lazy<IConfigurationManager<T>> LazyInstance =
        new(CreateConfigurationManager, LazyThreadSafetyMode.ExecutionAndPublication);

    private static ConfigurationSettings GetOrCreateSettings()
    {
        return ConfigurationCache.TypeConfigurations.GetOrAdd(typeof(T), _ => new ConfigurationSettings
        {
            ConfigurationName = "appsettings",
            ConfigurationDirectory = "./Config",
            Format = ConfigurationFormat.JSON,
            EnvironmentFileSupport = true,
            EnvironmentFileDirectory = "./",
            EnvironmentFileName = ".env",
            CachingEnabled = true,
            DefaultEnvironment = "dev"
        });
    }

    // Update Configure method to use the settings
    /// <summary>
    /// Represents configuration settings for the <see cref="ConfigurationSingleton{T}"/>.
    /// Provides configurable properties for defining the configuration
    /// details such as file format, caching, and environment-specific options.
    /// </summary>
    public class ConfSettings
    {
        /// <summary>
        /// Gets or sets the name of the configuration file.
        /// This property specifies the name of the primary configuration
        /// file used by the application, such as "appsettings".
        /// </summary>
        public string? ConfigurationName { get; set; }

        /// <summary>
        /// Gets or sets the directory path where the configuration files are located.
        /// This property defines the location used to store or access configuration
        /// files required by the application, with a default value of "./Config".
        /// </summary>
        public string? ConfigurationDirectory { get; set; }

        /// <summary>
        /// Gets or sets the format of the configuration file.
        /// The format determines the structure and syntax of the configuration file,
        /// such as JSON or YAML, and must align with the options supported by the system.
        /// </summary>
        public ConfigurationFormat? Format { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether support for environment-specific
        /// configuration files is enabled.
        /// When enabled, the application will attempt to load environment-specific
        /// settings from an additional file, typically identified by environment
        /// variables or a predefined naming convention.
        /// </summary>
        public bool? EnvironmentFileSupport { get; set; }

        /// <summary>
        /// Gets or sets the directory path where environment-specific
        /// configuration files are located.
        /// This property determines the directory to be used for loading
        /// environment variable files, such as `.env`.
        /// </summary>
        public string? EnvironmentFileDirectory { get; set; }

        /// <summary>
        /// Gets or sets the name of the environment file.
        /// This property specifies the file used to provide environment-specific
        /// configuration values, such as ".env".
        /// </summary>
        public string? EnvironmentFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether caching is enabled.
        /// This property determines if the configuration system should
        /// utilize caching mechanisms to improve performance by reducing
        /// the need for repeated access to configuration sources.
        /// </summary>
        public bool? CachingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the default environment for the configuration.
        /// This property determines the environment (e.g., "dev", "test", "prod")
        /// that is used when no specific environment is explicitly configured.
        /// </summary>
        public string? DefaultEnvironment { get; set; }
    }

    /// <summary>
    /// Configures the settings for the configuration manager.
    /// This method must be called before the configuration manager
    /// is accessed; otherwise, an exception will be thrown.
    /// </summary>
    /// <param name="settings">
    /// An instance of <see cref="ConfSettings"/> containing the configuration parameters.
    /// Settings include configuration name, directory, format, environment file support,
    /// environment file directory, as well as other settings for customization.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the method is called after the configuration manager is created.
    /// </exception>
    public static void Configure(ConfSettings settings)
    {
        if (LazyInstance.IsValueCreated)
        {
            throw new InvalidOperationException("Cannot configure after configuration manager is created.");
        }

        var currentSettings = GetOrCreateSettings();

        if (settings.ConfigurationName != null) currentSettings.ConfigurationName = settings.ConfigurationName;
        if (settings.ConfigurationDirectory != null) currentSettings.ConfigurationDirectory = settings.ConfigurationDirectory;
        if (settings.Format.HasValue) currentSettings.Format = settings.Format.Value;
        if (settings.EnvironmentFileSupport.HasValue) currentSettings.EnvironmentFileSupport = settings.EnvironmentFileSupport.Value;
        if (settings.EnvironmentFileDirectory != null) currentSettings.EnvironmentFileDirectory = settings.EnvironmentFileDirectory;
        if (settings.EnvironmentFileName != null) currentSettings.EnvironmentFileName = settings.EnvironmentFileName;
        if (settings.CachingEnabled.HasValue) currentSettings.CachingEnabled = settings.CachingEnabled.Value;
        if (settings.DefaultEnvironment != null) currentSettings.DefaultEnvironment = settings.DefaultEnvironment;
    }

    private static IConfigurationManager<T> CreateConfigurationManager()
    {
        var settings = GetOrCreateSettings();
        return ConfigurationExtensions
            .CreateConfigurationBuilder<T>()
            .WithConfigurationName(settings.ConfigurationName)
            .WithConfigurationDirectory(settings.ConfigurationDirectory)
            .WithFormat(settings.Format)
            .WithEnvironmentFileSupport(settings.EnvironmentFileSupport)
            .WithEnvironmentFileDirectory(settings.EnvironmentFileDirectory)
            .WithEnvironmentFileName(settings.EnvironmentFileName)
            .WithCaching(settings.CachingEnabled)
            .WithDefaultEnvironment(settings.DefaultEnvironment)
            .Build();
    }
}

// Use a concurrent dictionary to store configurations per type
/// <summary>
/// Provides a thread-safe caching mechanism for storing and retrieving configuration settings per type.
/// Uses a concurrent dictionary to manage configurations for high performance in multi-threaded environments.
/// This class is internal to the configuration management system and supports the underlying infrastructure.
/// </summary>
public static class ConfigurationCache
{
    internal static readonly ConcurrentDictionary<Type, ConfigurationSettings> TypeConfigurations = new();
}

/// <summary>
/// Represents the comprehensive settings for managing application configurations.
/// Provides options for specifying configuration file names, directories, formats, and environment support.
/// Supports toggling caching and setting default environments.
/// </summary>
public record ConfigurationSettings
{
    /// <summary>
    /// Gets or sets the name of the configuration file.
    /// This property defines the identifier for the configuration
    /// file used by the application to load settings.
    /// </summary>
    public string ConfigurationName { get; set; } = "appsettings";

    /// <summary>
    /// Gets or sets the directory path where the configuration files are located.
    /// This property determines the folder path containing the primary
    /// configuration files used by the application.
    /// </summary>
    public string ConfigurationDirectory { get; set; } = "./Config";

    /// <summary>
    /// Gets or sets the format of the configuration file.
    /// This property determines whether the configuration is in JSON or YAML format.
    /// </summary>
    public ConfigurationFormat Format { get; set; } = ConfigurationFormat.JSON;

    /// <summary>
    /// Gets or sets a value indicating whether support for environment files is enabled.
    /// When enabled, the application will load configuration values from a specified environment file,
    /// providing an additional layer of configuration customization.
    /// </summary>
    public bool EnvironmentFileSupport { get; set; } = true;

    /// <summary>
    /// Gets or sets the directory path where the environment file is located.
    /// This property defines the directory containing the environment-specific
    /// configuration file, typically used for environment variable overrides.
    /// </summary>
    public string EnvironmentFileDirectory { get; set; } = "./";

    /// <summary>
    /// Gets or sets the name of the environment file.
    /// This property specifies the file name for environment-specific settings,
    /// typically used to override or complement the primary configuration.
    /// </summary>
    public string EnvironmentFileName { get; set; } = ".env";

    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled for the configuration.
    /// When set to true, configuration values are cached to improve performance
    /// by reducing repetitive access to the configuration source.
    /// </summary>
    public bool CachingEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default environment name used by the application.
    /// This property specifies the current environment configuration, such as "dev", "staging", or "production".
    /// It helps in determining environment-specific overrides or settings if applicable.
    /// </summary>
    public string DefaultEnvironment { get; set; } = "dev";
}
