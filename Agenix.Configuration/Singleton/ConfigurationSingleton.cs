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

    // Use a concurrent dictionary to store configurations per type
    private static readonly ConcurrentDictionary<Type, ConfigurationSettings> TypeConfigurations =
        new();

    private static ConfigurationSettings GetOrCreateSettings()
    {
        return TypeConfigurations.GetOrAdd(typeof(T), _ => new ConfigurationSettings
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

    private record ConfigurationSettings
    {
        public string ConfigurationName { get; set; } = "appsettings";
        public string ConfigurationDirectory { get; set; } = "./Config";
        public ConfigurationFormat Format { get; set; } = ConfigurationFormat.JSON;
        public bool EnvironmentFileSupport { get; set; } = true;
        public string EnvironmentFileDirectory { get; set; } = "./";
        public string EnvironmentFileName { get; set; } = ".env";
        public bool CachingEnabled { get; set; } = true;
        public string DefaultEnvironment { get; set; } = "dev";
    }

    // Update Configure method to use the settings
    /// <summary>
    /// Configures the settings for the configuration manager before it is initialized.
    /// Once the configuration manager has been created, this method cannot be called again.
    /// </summary>
    /// <param name="configurationName">
    /// The name of the configuration file. Default is "appsettings".
    /// </param>
    /// <param name="configurationDirectory">
    /// The directory where the configuration file is located. Default is "./Config".
    /// </param>
    /// <param name="format">
    /// The format of the configuration file. Supported formats are JSON and YAML. Default is JSON.
    /// </param>
    /// <param name="environmentFileSupport">
    /// A flag indicating whether environment file support is enabled. Default is true.
    /// </param>
    /// <param name="environmentFileDirectory">
    /// The directory where the environment file is located. Default is "./".
    /// </param>
    /// <param name="environmentFileName">
    /// The name of the environment file. Default is ".env".
    /// </param>
    /// <param name="cachingEnabled">
    /// A flag indicating whether caching is enabled for the configuration manager. Default is true.
    /// </param>
    /// <param name="defaultEnvironment">
    /// The default environment for the configuration. Default is "dev".
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this method is called after the configuration manager has been initialized.
    /// </exception>
    public static void Configure(
        string? configurationName = null,
        string? configurationDirectory = null,
        ConfigurationFormat? format = null,
        bool? environmentFileSupport = null,
        string? environmentFileDirectory = null,
        string? environmentFileName = null,
        bool? cachingEnabled = null,
        string? defaultEnvironment = null)
    {
        if (LazyInstance.IsValueCreated)
        {
            throw new InvalidOperationException("Cannot configure after configuration manager is created.");
        }

        var settings = GetOrCreateSettings();

        if (configurationName != null) settings.ConfigurationName = configurationName;
        if (configurationDirectory != null) settings.ConfigurationDirectory = configurationDirectory;
        if (format.HasValue) settings.Format = format.Value;
        if (environmentFileSupport.HasValue) settings.EnvironmentFileSupport = environmentFileSupport.Value;
        if (environmentFileDirectory != null) settings.EnvironmentFileDirectory = environmentFileDirectory;
        if (environmentFileName != null) settings.EnvironmentFileName = environmentFileName;
        if (cachingEnabled.HasValue) settings.CachingEnabled = cachingEnabled.Value;
        if (defaultEnvironment != null) settings.DefaultEnvironment = defaultEnvironment;
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
