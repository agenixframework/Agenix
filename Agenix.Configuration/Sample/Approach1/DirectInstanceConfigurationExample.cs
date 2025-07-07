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

#endregion

using Agenix.Configuration.Core;
using Agenix.Configuration.Core.Models;
using Agenix.Configuration.Sample.Models;

namespace Agenix.Configuration.Sample.Approach1;

/// <summary>
/// Demonstrates Approach 1: Direct Instance Usage
/// - Create ConfigurationOptions directly
/// - Instantiate ConfigurationManager with options
/// - Use the manager instance throughout the application
/// </summary>
public class DirectInstanceExample
{
    private readonly IConfigurationManager<ApplicationSettings> _configManager;

    /// <summary>
    /// Provides an example of direct instance usage for configuration management.
    /// - Demonstrates creating a configuration options object manually
    /// - Showcases instantiation and use of a configuration manager with the defined options
    /// - Includes various implementation scenarios like environment-specific configurations, caching, and async operations
    /// </summary>
    public DirectInstanceExample()
    {

        // Create configuration options
        var options = new ConfigurationOptions
        {
            ConfigurationName = "appsettings",
            ConfigurationDirectory = "./Sample/Config",
            Format = ConfigurationFormat.JSON,
            EnvironmentFileSupport = true,
            CachingEnabled = true,
            DefaultEnvironment = "dev"
        };

        // Create configuration manager instance
        _configManager = new ConfigurationManager<ApplicationSettings>(options);
    }

    /// <summary>
    /// Demonstrates basic configuration usage
    /// </summary>
    public void DemonstrateBasicUsage()
    {
        Console.WriteLine("=== Approach 1: Direct Instance - Basic Usage ===");

        // Get configuration for current environment
        var config = _configManager.GetConfiguration();

        Console.WriteLine("Current Environment: {0}", _configManager.CurrentEnvironment);
        Console.WriteLine("Database Connection: {0}", config.DatabaseConnectionString);
        Console.WriteLine("API Base URL: {0}", config.ApiBaseUrl);
        Console.WriteLine("Cache Provider: {0}", config.Cache.Provider);
        Console.WriteLine("Log Level: {0}", config.Logging.Level);
    }

    /// <summary>
    /// Demonstrates environment-specific configuration
    /// </summary>
    public void DemonstrateEnvironmentSpecificUsage()
    {
        Console.WriteLine("=== Approach 1: Direct Instance - Environment Specific ===");

        // Get configuration for different environments
        var devConfig = _configManager.GetConfiguration("dev");
        var prodConfig = _configManager.GetConfiguration("prod");

        Console.WriteLine("Dev Environment:");
        Console.WriteLine("  Database: {0}", devConfig.DatabaseConnectionString);
        Console.WriteLine("  API URL: {0}", devConfig.ApiBaseUrl);
        Console.WriteLine("  Beta Features: {0}", devConfig.Features.EnableBetaFeatures);

        Console.WriteLine("Prod Environment:");
        Console.WriteLine("  Database: {0}", prodConfig.DatabaseConnectionString);
        Console.WriteLine("  API URL: {0}", prodConfig.ApiBaseUrl);
        Console.WriteLine("  Beta Features: {0}", prodConfig.Features.EnableBetaFeatures);
    }

    /// <summary>
    /// Demonstrates async configuration loading
    /// </summary>
    public async Task DemonstrateAsyncUsage()
    {
        Console.WriteLine("=== Approach 1: Direct Instance - Async Usage ===");

        // Async configuration loading
        var config = await _configManager.GetConfigurationAsync();
        var devConfig = await _configManager.GetConfigurationAsync("dev");

        Console.WriteLine("Async loaded config for current environment:");
        Console.WriteLine("  Cache expiration: {0} minutes", config.Cache.ExpirationMinutes);

        Console.WriteLine("Async loaded config for dev environment:");
        Console.WriteLine("  Cache expiration: {0} minutes", devConfig.Cache.ExpirationMinutes);
    }

    /// <summary>
    /// Demonstrates configuration caching and reloading
    /// </summary>
    public void DemonstrateCachingAndReloading()
    {
        Console.WriteLine("=== Approach 1: Direct Instance - Caching & Reloading ===");

        Console.WriteLine("Caching enabled: {0}", _configManager.IsConfigurationCached);

        // Load configuration (will be cached)
        var config1 = _configManager.GetConfiguration("dev");
        Console.WriteLine("First load - Cache provider: {0}", config1.Cache.Provider);

        // Load again (should come from cache)
        var config2 = _configManager.GetConfiguration("dev");
        Console.WriteLine("Second load - Cache provider: {0}", config2.Cache.Provider);

        // Reload configuration (clears cache)
        _configManager.ReloadConfiguration("dev");
        Console.WriteLine("Configuration reloaded for dev environment");

        // Load after reload
        var config3 = _configManager.GetConfiguration("dev");
        Console.WriteLine("After reload - Cache provider: {0}", config3.Cache.Provider);
    }

    /// <summary>
    /// Demonstrates passing configuration manager to services
    /// </summary>
    public void DemonstrateServiceUsage()
    {
        Console.WriteLine("=== Approach 1: Direct Instance - Service Usage ===");

        // Pass configuration manager to services
        var databaseService = new DatabaseService(_configManager);
        var apiService = new ApiService(_configManager);

        // Use services
        databaseService.Connect();
        apiService.GetData();
    }
}

/// <summary>
/// Example service that uses configuration manager
/// </summary>
public class DatabaseService
{
    private readonly IConfigurationManager<ApplicationSettings> _configManager;

    /// <summary>
    /// Represents a service for managing database connections and operations.
    /// Utilizes the configuration manager to retrieve connection settings.
    /// - Integrates with the application's configuration management to adapt to environment-specific settings.
    /// - Supports connection initialization based on retrieved configuration data.
    /// </summary>
    public DatabaseService(IConfigurationManager<ApplicationSettings> configManager)
    {
        _configManager = configManager;
    }

    /// <summary>
    /// Establishes a connection to the database using configuration settings retrieved from
    /// the configuration manager.
    /// - Retrieves the database connection string from the configuration object
    /// - Outputs a truncated version of the connection string for demonstration purposes
    /// </summary>
    public void Connect()
    {
        var config = _configManager.GetConfiguration();
        Console.WriteLine("Connecting to database: {0}",
            config.DatabaseConnectionString[..Math.Min(50, config.DatabaseConnectionString.Length)] + "...");
    }
}

/// <summary>
/// Example service that uses configuration manager
/// </summary>
public class ApiService
{
    private readonly IConfigurationManager<ApplicationSettings> _configManager;

    /// <summary>
    /// Represents an example API service that retrieves configuration data and demonstrates its usage.
    /// - Designed to consume configuration settings through the provided configuration manager
    /// - Retrieves necessary configuration like the API base URL for making service calls
    /// - Showcases dependency injection for passing the configuration manager to services
    /// </summary>
    public ApiService(IConfigurationManager<ApplicationSettings> configManager)
    {
        _configManager = configManager;
    }

    /// <summary>
    /// Retrieves data by using the configuration settings provided through the injected configuration manager.
    /// - Accesses the current API base URL from the configuration
    /// - Outputs the API call base URL to the console
    /// </summary>
    public void GetData()
    {
        var config = _configManager.GetConfiguration();
        Console.WriteLine(@"Making API call to: {0}", config.ApiBaseUrl);
    }
}
