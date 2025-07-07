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

using System.Collections.Concurrent;
using Agenix.Api.Log;
using Agenix.Configuration.Core.Models;
using Agenix.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agenix.Configuration.Core;

/// <summary>
/// Implementation of IConfigurationManager that provides environment-specific configuration loading
/// with caching support for both JSON and YAML formats.
/// </summary>
/// <typeparam name="T">The type of the configuration object</typeparam>
public class ConfigurationManager<T> : IConfigurationManager<T> where T : class, new()
{
    private readonly ConfigurationOptions _options;
    private readonly ConcurrentDictionary<string, T> _cache = new();

    /// <summary>
    ///     Logger.
    /// </summary>
    private readonly ILogger _logger = LogManager.GetLogger(typeof(ConfigurationManager<>).MakeGenericType(typeof(T)));


    /// <summary>
    /// Initializes a new instance of the ConfigurationManager class.
    /// </summary>
    /// <param name="options">Configuration options</param>
    public ConfigurationManager(ConfigurationOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        CurrentEnvironment = _options.DefaultEnvironment;
    }

    /// <inheritdoc />
    public bool IsConfigurationCached => _options.CachingEnabled;

    /// <inheritdoc />
    public string? CurrentEnvironment { get; private set; }

    /// <inheritdoc />
    public T GetConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                         ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                         ?? _options.DefaultEnvironment;

        CurrentEnvironment = environment;
        return GetConfiguration(environment);
    }

    /// <inheritdoc />
    public T GetConfiguration(string environment)
    {
        if (string.IsNullOrWhiteSpace(environment))
            throw new ArgumentException("Environment cannot be null or empty", nameof(environment));

        CurrentEnvironment = environment;

        if (_options.CachingEnabled && _cache.TryGetValue(environment, out var cachedConfig))
        {
            _logger.LogDebug("Retrieved cached configuration for environment: {environment}", environment);
            return cachedConfig;
        }

        var config = LoadConfiguration(environment);

        if (_options.CachingEnabled)
        {
            _cache.TryAdd(environment, config);
            _logger.LogDebug("Cached configuration for environment: {environment}", environment);
        }

        return config;
    }

    /// <inheritdoc />
    public async Task<T> GetConfigurationAsync()
    {
        return await Task.Run(GetConfiguration);
    }

    /// <inheritdoc />
    public async Task<T> GetConfigurationAsync(string environment)
    {
        return await Task.Run(() => GetConfiguration(environment));
    }

    /// <inheritdoc />
    public void ReloadConfiguration()
    {
        if (!string.IsNullOrEmpty(CurrentEnvironment))
        {
            ReloadConfiguration(CurrentEnvironment);
        }
    }

    /// <inheritdoc />
    public void ReloadConfiguration(string environment)
    {
        if (_options.CachingEnabled)
        {
            _cache.TryRemove(environment, out _);
            _logger.LogDebug("Cleared cache for environment: {environment}", environment);
        }
    }

    private T LoadConfiguration(string environment)
    {
        try
        {
            var configBuilder = new ConfigurationBuilder();

            // Load environment file if supported
            if (_options.EnvironmentFileSupport)
            {
                LoadEnvironmentFile();
            }

            // Load main configuration file
            var mainConfigPath = Path.Combine(_options.ConfigurationDirectory,
                $"{_options.ConfigurationName}{_options.FileExtension}");

            if (File.Exists(mainConfigPath))
            {
                AddConfigurationFile(configBuilder, mainConfigPath, false);
                _logger.LogDebug("Loaded main configuration from: {mainConfigPath} (Format: {_options.Format})",
                    mainConfigPath, _options.Format);
            }

            // Load environment-specific configuration
            var envConfigPath = Path.Combine(_options.ConfigurationDirectory,
                $"{_options.ConfigurationName}.{environment}{_options.FileExtension}");

            if (File.Exists(envConfigPath))
            {
                AddConfigurationFile(configBuilder, envConfigPath, true);
                _logger.LogDebug("Loaded environment configuration from: {envConfigPath} (Format: {_options.Format})",
                    envConfigPath, _options.Format);
            }

            // Add environment variables
            configBuilder.AddEnvironmentVariables();

            var configuration = configBuilder.Build();
            var configObject = new T();
            configuration.Bind(configObject);

            _logger.LogInformation("Successfully loaded configuration for environment: {environment} (Format: {_options.Format})",
                environment, _options.Format);
            return configObject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration for environment: {environment} (Format: {_options.Format})",
                environment, _options.Format);
            throw new InvalidOperationException($"Failed to load configuration for environment '{environment}' with format '{_options.Format}'", ex);
        }
    }

    private void AddConfigurationFile(IConfigurationBuilder builder, string filePath, bool optional)
    {
        switch (_options.Format)
        {
            case ConfigurationFormat.JSON:
                builder.AddJsonFile(filePath, optional: optional, reloadOnChange: false);
                break;
            case ConfigurationFormat.YAML:
                builder.AddYamlFile(filePath, optional: optional, reloadOnChange: false);
                break;
            default:
                throw new NotSupportedException($"Configuration format '{_options.Format}' is not supported.");
        }
    }

    private void LoadEnvironmentFile()
    {
        var envFilePath = Path.Combine(_options.EnvironmentFileDirectory, _options.EnvironmentFileName);
        if (!File.Exists(envFilePath)) return;

        try
        {
            var lines = File.ReadAllLines(envFilePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
            _logger.LogDebug("Loaded environment file: {Path}", envFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load environment file: {Path}", envFilePath);
        }
    }
}
