
## 4. Approach 2: Dependency Injection

<llm-snippet-file>Agenix.Configuration/Sample/Approach2/DependencyInjectionExample.cs</llm-snippet-file>
```csharp
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
using Agenix.Configuration.Extensions;
using Agenix.Configuration.Sample.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agenix.Configuration.Sample.Approach2;

/// <summary>
/// Demonstrates Approach 2: Dependency Injection Usage
/// - Register configuration manager in DI container
/// - Use builder pattern for configuration
/// - Inject configuration manager into services
/// </summary>
public class DependencyInjectionExample
{
    /// <summary>
    /// Demonstrates how to configure services with DI
    /// </summary>
    public static void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Method 1: Register with builder pattern
        services.AddSingleton<IConfigurationManager<ApplicationSettings>>(provider =>
        {
            var logger = provider.GetService<ILogger<ConfigurationManager<ApplicationSettings>>>();
            
            return ConfigurationExtensions
                .CreateConfigurationBuilder<ApplicationSettings>()
                .WithConfigurationName("appsettings")
                .WithConfigurationDirectory("./Sample/Config")
                .WithFormat(ConfigurationFormat.JSON)
                .WithEnvironmentFileSupport(true)
                .WithCaching(true)
                .WithDefaultEnvironment("dev")
                .WithLogger(logger)
                .Build();
        });

        // Method 2: Register with options (alternative)
        // services.AddSingleton<IConfigurationManager<ApplicationSettings>>(provider =>
        // {
        //     var logger = provider.GetService<ILogger<ConfigurationManager<ApplicationSettings>>>();
        //     var options = new ConfigurationOptions
        //     {
        //         ConfigurationName = "appsettings",
        //         ConfigurationDirectory = "./Sample/Config",
        //         Format = ConfigurationFormat.JSON,
        //         CachingEnabled = true,
        //         DefaultEnvironment = "dev"
        //     };
        //     return new ConfigurationManager<ApplicationSettings>(options, logger);
        // });

        // Register your services
        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddScoped<IApiService, ApiService>();
        services.AddScoped<IBusinessService, BusinessService>();
    }

    /// <summary>
    /// Demonstrates running the application with DI
    /// </summary>
    public static async Task RunExampleAsync()
    {
        // Create host builder
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
                services.AddHostedService<DemoHostedService>();
            })
            .Build();

        // Run the application
        await host.RunAsync();
    }
}

/// <summary>
/// Hosted service that demonstrates DI usage
/// </summary>
public class DemoHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DemoHostedService> _logger;

    public DemoHostedService(IServiceProvider serviceProvider, ILogger<DemoHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("=== Approach 2: Dependency Injection Example ===");

        using var scope = _serviceProvider.CreateScope();
        
        // Get services from DI container
        var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
        var apiService = scope.ServiceProvider.GetRequiredService<IApiService>();
        var businessService = scope.ServiceProvider.GetRequiredService<IBusinessService>();

        // Use services
        await databaseService.ConnectAsync();
        await apiService.GetDataAsync();
        await businessService.ProcessDataAsync();

        // Demonstrate direct configuration manager usage
        var configManager = scope.ServiceProvider.GetRequiredService<IConfigurationManager<ApplicationSettings>>();
        DemonstrateDirectConfigurationUsage(configManager);

        // Stop the host
        await Task.Delay(1000, stoppingToken);
        Environment.Exit(0);
    }

    private void DemonstrateDirectConfigurationUsage(IConfigurationManager<ApplicationSettings> configManager)
    {
        _logger.LogInformation("=== Direct Configuration Manager Usage ===");

        // Get configuration for different environments
        var devConfig = configManager.GetConfiguration("dev");
        var prodConfig = configManager.GetConfiguration("prod");

        _logger.LogInformation("Dev Config - New Dashboard: {Enabled}", devConfig.Features.EnableNewDashboard);
        _logger.LogInformation("Prod Config - New Dashboard: {Enabled}", prodConfig.Features.EnableNewDashboard);

        // Demonstrate caching
        _logger.LogInformation("Caching enabled: {Enabled}", configManager.IsConfigurationCached);
        _logger.LogInformation("Current environment: {Environment}", configManager.CurrentEnvironment);
    }
}

#region Service Interfaces

public interface IDatabaseService
{
    Task ConnectAsync();
}

public interface IApiService
{
    Task<string> GetDataAsync();
}

public interface IBusinessService
{
    Task ProcessDataAsync();
}

#endregion

#region Service Implementations

public class DatabaseService : IDatabaseService
{
    private readonly IConfigurationManager<ApplicationSettings> _configManager;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IConfigurationManager<ApplicationSettings> configManager, ILogger<DatabaseService> logger)
    {
        _configManager = configManager;
        _logger = logger;
    }

    public async Task ConnectAsync()
    {
        var config = await _configManager.GetConfigurationAsync();
        
        _logger.LogInformation("DatabaseService: Connecting to database...");
        _logger.LogInformation("Connection string: {ConnectionString}", 
            config.DatabaseConnectionString.Substring(0, Math.Min(50, config.DatabaseConnectionString.Length)) + "...");
        
        // Simulate database connection
        await Task.Delay(100);
        _logger.LogInformation("DatabaseService: Connected successfully!");
    }
}

public class ApiService : IApiService
{
    private readonly IConfigurationManager<ApplicationSettings> _configManager;
    private readonly ILogger<ApiService> _logger;

    public ApiService(IConfigurationManager<ApplicationSettings> configManager, ILogger<ApiService> logger)
    {
        _configManager = configManager;
        _logger = logger;
    }

    public async Task<string> GetDataAsync()
    {
        var config = await _configManager.GetConfigurationAsync();
        
        _logger.LogInformation("ApiService: Making API call...");
        _logger.LogInformation("API Base URL: {BaseUrl}", config.ApiBaseUrl);
        
        // Simulate API call
        await Task.Delay(200);
        var data = $"Data from {config.ApiBaseUrl}";
        
        _logger.LogInformation("ApiService: Received data: {Data}", data);
        return data;
    }
}

public class BusinessService : IBusinessService
{
    private readonly IDatabaseService _databaseService;
    private readonly IApiService _apiService;
    private readonly IConfigurationManager<ApplicationSettings> _configManager;
    private readonly ILogger<BusinessService> _logger;

    public BusinessService(
        IDatabaseService databaseService,
        IApiService apiService,
        IConfigurationManager<ApplicationSettings> configManager,
        ILogger<BusinessService> logger)
    {
        _databaseService = databaseService;
        _apiService = apiService;
        _configManager = configManager;
        _logger = logger;
    }

    public async Task ProcessDataAsync()
    {
        var config = await _configManager.GetConfigurationAsync();
        
        _logger.LogInformation("BusinessService: Processing data...");
        _logger.LogInformation("Advanced Analytics enabled: {Enabled}", config.Features.EnableAdvancedAnalytics);

        // Use other services
        await _databaseService.ConnectAsync();
        var data = await _apiService.GetDataAsync();

        if (config.Features.EnableAdvancedAnalytics)
        {
            _logger.LogInformation("BusinessService: Running advanced analytics on: {Data}", data);
        }
        else
        {
            _logger.LogInformation("BusinessService: Running basic processing on: {Data}", data);
        }

        _logger.LogInformation("BusinessService: Processing completed!");
    }
}

#endregion
```

<llm-snippet-file>Agenix.Configuration/Sample/Approach2/README.md</llm-snippet-file>
```markdown
# Approach 2: Dependency Injection

This approach demonstrates how to integrate the Agenix Configuration Manager with .NET's built-in dependency injection container.

## Key Characteristics

- **DI Container integration**: Register configuration manager in the service container
- **Builder pattern**: Use fluent builder API for configuration
- **Automatic injection**: Services receive configuration manager through constructor injection
- **Lifecycle management**: DI container manages the configuration manager lifecycle

## When to Use

- **ASP.NET Core applications**: Perfect for web applications and APIs
- **Modern .NET applications**: When using .NET Generic Host
- **Service-oriented architecture**: When you have multiple services that need configuration
- **Testable applications**: When you want to easily mock configuration for testing

## Pros

- ✅ Excellent testability (easy to mock)
- ✅ Clean separation of concerns
- ✅ Automatic lifecycle management
- ✅ Follows .NET conventions
- ✅ Easy to unit test
- ✅ Supports multiple configuration types

## Cons

- ❌ Requires DI container setup
- ❌ Slightly more complex initial setup
- ❌ DI container dependency

## Usage Example

```csharp
// Configure services
services.AddSingleton<IConfigurationManager<ApplicationSettings>>(provider =>
{
    var logger = provider.GetService<ILogger<ConfigurationManager<ApplicationSettings>>>();
    
    return ConfigurationExtensions
        .CreateConfigurationBuilder<ApplicationSettings>()
        .WithConfigurationName("appsettings")
        .WithConfigurationDirectory("./Config")
        .WithFormat(ConfigurationFormat.JSON)
        .WithCaching(true)
        .WithLogger(logger)
        .Build();
});

// Use in service
public class MyService
{
    private readonly IConfigurationManager<ApplicationSettings> _configManager;
    
    public MyService(IConfigurationManager<ApplicationSettings> configManager)
    {
        _configManager = configManager;
    }
}
```

## Registration Options

### Option 1: Builder Pattern (Recommended)
```csharp
services.AddSingleton<IConfigurationManager<ApplicationSettings>>(provider =>
    ConfigurationExtensions
        .CreateConfigurationBuilder<ApplicationSettings>()
        .WithConfigurationName("appsettings")
        .WithConfigurationDirectory("./Config")
        .WithFormat(ConfigurationFormat.JSON)
        .WithCaching(true)
        .Build());
```

### Option 2: Direct Options
```csharp
services.AddSingleton<IConfigurationManager<ApplicationSettings>>(provider =>
{
    var options = new ConfigurationOptions
    {
        ConfigurationName = "appsettings",
        ConfigurationDirectory = "./Config",
        Format = ConfigurationFormat.JSON,
        CachingEnabled = true
    };
    return new ConfigurationManager<ApplicationSettings>(options);
});
```

## Best Practices

1. **Register as Singleton**: Configuration manager should be registered as singleton for performance
2. **Use builder pattern**: More readable and maintainable than direct options
3. **Include logging**: Always register with logging support
4. **Enable caching**: Use caching for better performance
5. **Interface segregation**: Inject only what services need

## Testing

```csharp
// Easy to mock in unit tests
var mockConfigManager = new Mock<IConfigurationManager<ApplicationSettings>>();
mockConfigManager.Setup(x => x.GetConfiguration()).Returns(testConfig);

var service = new MyService(mockConfigManager.Object);
```

## Example Files

- `DependencyInjectionExample.cs` - Complete example with service registration and usage
- `README.md` - This documentation
```

## 5. Approach 3: Singleton

<llm-snippet-file>Agenix.Configuration/Sample/Approach3/SingletonExample.cs</llm-snippet-file>
```csharp
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

using Agenix.Configuration.Core.Models;
using Agenix.Configuration.Sample.Models;
using Agenix.Configuration.Singleton;
using Microsoft.Extensions.Logging;

namespace Agenix.Configuration.Sample.Approach3;

/// <summary>
/// Demonstrates Approach 3: Singleton Usage
/// - Use ConfigurationSingleton for global access
/// - Configure once at application startup
/// - Access configuration from anywhere without injection
/// </summary>
public class SingletonExample
{
    private readonly ILogger<SingletonExample> _logger;

    public SingletonExample(ILogger<SingletonExample> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Demonstrates how to configure the singleton at application startup
    /// </summary>
    public static void ConfigureAtStartup(ILogger? logger = null)
    {
        // Configure the singleton once at application startup
        // This must be done before any usage of the configuration
        ConfigurationSingleton<ApplicationSettings>.Configure(
            logger: logger,
            configurationName: "appsettings",
            configurationDirectory: "./Sample/Config",
            format: ConfigurationFormat.JSON,
            environmentFileSupport: true,
            cachingEnabled: true,
            defaultEnvironment: "dev"
        );
    }

    /// <summary>
    /// Demonstrates basic singleton usage
    /// </summary>
    public void DemonstrateBasicUsage()
    {
        _logger.LogInformation("=== Approach 3: Singleton - Basic Usage ===");

        // Access configuration from anywhere - no injection needed!
        var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();
        
        _logger.LogInformation("Current Environment: {Environment}", 
            ConfigurationSingleton<ApplicationSettings>.CurrentEnvironment);
        _logger.LogInformation("Database Connection: {Connection}", config.DatabaseConnectionString);
        _logger.LogInformation("API Base URL: {Url}", config.ApiBaseUrl);
        _logger.LogInformation("Cache Provider: {Provider}", config.Cache.Provider);
        _logger.LogInformation("Log Level: {Level}", config.Logging.Level);
    }

    /// <summary>
    /// Demonstrates environment-specific configuration access
    /// </summary>
    public void DemonstrateEnvironmentSpecificUsage()
    {
        _logger.LogInformation("=== Approach 3: Singleton - Environment Specific ===");

        // Get configuration for different environments
        var devConfig = ConfigurationSingleton<ApplicationSettings>.GetConfiguration("dev");
        var prodConfig = ConfigurationSingleton<ApplicationSettings>.GetConfiguration("prod");

        _logger.LogInformation("Dev Environment:");
        _logger.LogInformation("  New Dashboard: {Enabled}", devConfig.Features.EnableNewDashboard);
        _logger.LogInformation("  Beta Features: {Enabled}", devConfig.Features.EnableBetaFeatures);
        _logger.LogInformation("  Cache Expiration: {Minutes} minutes", devConfig.Cache.ExpirationMinutes);

        _logger.LogInformation("Prod Environment:");
        _logger.LogInformation("  New Dashboard: {Enabled}", prodConfig.Features.EnableNewDashboard);
        _logger.LogInformation("  Beta Features: {Enabled}", prodConfig.Features.EnableBetaFeatures);
        _logger.LogInformation("  Cache Expiration: {Minutes} minutes", prodConfig.Cache.ExpirationMinutes);
    }

    /// <summary>
    /// Demonstrates async configuration access
    /// </summary>
    public async Task DemonstrateAsyncUsage()
    {
        _logger.LogInformation("=== Approach 3: Singleton - Async Usage ===");

        // Async configuration access
        var config = await ConfigurationSingleton<ApplicationSettings>.GetConfigurationAsync();
        var devConfig = await ConfigurationSingleton<ApplicationSettings>.GetConfigurationAsync("dev");

        _logger.LogInformation("Async loaded config for current environment:");
        _logger.LogInformation("  Advanced Analytics: {Enabled}", config.Features.EnableAdvancedAnalytics);
        
        _logger.LogInformation("Async loaded config for dev environment:");
        _logger.LogInformation("  Advanced Analytics: {Enabled}", devConfig.Features.EnableAdvancedAnalytics);
    }

    /// <summary>
    /// Demonstrates configuration management features
    /// </summary>
    public void DemonstrateConfigurationManagement()
    {
        _logger.LogInformation("=== Approach 3: Singleton - Configuration Management ===");

        // Check configuration properties
        _logger.LogInformation("Caching enabled: {Enabled}", 
            ConfigurationSingleton<ApplicationSettings>.IsConfigurationCached);
        _logger.LogInformation("Current environment: {Environment}", 
            ConfigurationSingleton<ApplicationSettings>.CurrentEnvironment);

        // Load configuration (will be cached if caching is enabled)
        var config1 = ConfigurationSingleton<ApplicationSettings>.GetConfiguration("dev");
        _logger.LogInformation("First load - Log level: {Level}", config1.Logging.Level);

        // Load again (should come from cache if caching is enabled)
        var config2 = ConfigurationSingleton<ApplicationSettings>.GetConfiguration("dev");
        _logger.LogInformation("Second load - Log level: {Level}", config2.Logging.Level);

        // Reload configuration (clears cache)
        ConfigurationSingleton<ApplicationSettings>.ReloadConfiguration("dev");
        _logger.LogInformation("Configuration reloaded for dev environment");

        // Load after reload
        var config3 = ConfigurationSingleton<ApplicationSettings>.GetConfiguration("dev");
        _logger.LogInformation("After reload - Log level: {Level}", config3.Logging.Level);
    }

    /// <summary>
    /// Demonstrates using singleton with services
    /// </summary>
    public void DemonstrateServiceUsage()
    {
        _logger.LogInformation("=== Approach 3: Singleton - Service Usage ===");

        // Create services - no need to pass configuration!
        var databaseService = new DatabaseService(_logger);
        var apiService = new ApiService(_logger);
        var businessService = new BusinessService(_logger);

        // Use services
        databaseService.Connect();
        apiService.GetData();
        businessService.ProcessData();
    }

    /// <summary>
    /// Demonstrates static usage from anywhere in the application
    /// </summary>
    public static void DemonstrateStaticUsage()
    {
        // This can be called from anywhere in your application
        // No need for dependency injection or passing instances around
        
        var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();
        
        Console.WriteLine($"Static access - API URL: {config.ApiBaseUrl}");
        Console.WriteLine($"Static access - Current Environment: {ConfigurationSingleton<ApplicationSettings>.CurrentEnvironment}");
    }
}

#region Service Examples - No Configuration Injection Needed!

/// <summary>
/// Database service that uses singleton configuration
/// </summary>
public class DatabaseService
{
    private readonly ILogger _logger;

    public DatabaseService(ILogger logger)
    {
        _logger = logger;
        // No configuration injection needed!
    }

    public void Connect()
    {
        // Access configuration directly from singleton
        var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();
        
        _logger.LogInformation("DatabaseService: Connecting to database...");
        _logger.LogInformation("Connection string: {ConnectionString}", 
            config.DatabaseConnectionString.Substring(0, Math.Min(50, config.DatabaseConnectionString.Length)) + "...");
        
        _logger.LogInformation("DatabaseService: Connected successfully!");
    }

    public async Task ConnectAsync()
    {
        // Async access also available
        var config = await ConfigurationSingleton<ApplicationSettings>.GetConfigurationAsync();
        
        _logger.LogInformation("DatabaseService: Async connecting to database...");
        _logger.LogInformation("Connection string: {ConnectionString}", 
            config.DatabaseConnectionString.Substring(0, Math.Min(50, config.DatabaseConnectionString.Length)) + "...");
        
        await Task.Delay(100); // Simulate async operation
        _logger.LogInformation("DatabaseService: Async connected successfully!");
    }
}

/// <summary>
/// API service that uses singleton configuration
/// </summary>
public class ApiService
{
    private readonly ILogger _logger;

    public ApiService(ILogger logger)
    {
        _logger = logger;
        // No configuration injection needed!
    }

    public string GetData()
    {
        // Access configuration directly from singleton
        var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();
        
        _logger.LogInformation("ApiService: Making API call...");
        _logger.LogInformation("API Base URL: {BaseUrl}", config.ApiBaseUrl);
        
        var data = $"Data from {config.ApiBaseUrl}";
        _logger.LogInformation("ApiService: Received data: {Data}", data);
        
        return data;
    }

    public async Task<string> GetDataAsync()
    {
        // Async access
        var config = await ConfigurationSingleton<ApplicationSettings>.GetConfigurationAsync();
        
        _logger.LogInformation("ApiService: Making async API call...");
        _logger.LogInformation("API Base URL: {BaseUrl}", config.ApiBaseUrl);
        
        await Task.Delay(200); // Simulate async operation
        var data = $"Async data from {config.ApiBaseUrl}";
        _logger.LogInformation("ApiService: Received async data: {Data}", data);
        
        return data;
    }
}

/// <summary>
/// Business service that uses singleton configuration and other services
/// </summary>
public class BusinessService
{
    private readonly ILogger _logger;
    private readonly DatabaseService _databaseService;
    private readonly ApiService _apiService;

    public BusinessService(ILogger logger)
    {
        _logger = logger;
        // Create other services - no configuration needed to pass around!
        _databaseService = new DatabaseService(logger);
        _apiService = new ApiService(logger);
    }

    public void ProcessData()
    {
        // Access configuration directly
        var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();
        
        _logger.LogInformation("BusinessService: Processing data...");
        _logger.LogInformation("Advanced Analytics enabled: {Enabled}", config.Features.EnableAdvancedAnalytics);

        // Use other services
        _databaseService.Connect();
        var data = _apiService.GetData();

        if (config.Features.EnableAdvancedAnalytics)
        {
            _logger.LogInformation("BusinessService: Running advanced analytics on: {Data}", data);
        }
        else
        {
            _logger.LogInformation("BusinessService: Running basic processing on: {Data}", data);
        }

        _logger.LogInformation("BusinessService: Processing completed!");
    }
}

#endregion

/// <summary>
/// Utility class demonstrating configuration access from static contexts
/// </summary>
public static class ConfigurationUtilities
{
    /// <summary>
    /// Example of accessing configuration from a static method
    /// </summary>
    public static string GetDatabaseConnectionString()
    {
        var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();
        return config.DatabaseConnectionString;
    }

    /// <summary>
    /// Example of environment-specific configuration access
    /// </summary>
    public static bool IsFeatureEnabled(string feature, string environment)
    {
        var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration(environment);
        
        return feature.ToLower() switch
        {
            "newdashboard" => config.Features.EnableNewDashboard,
            "advancedanalytics" => config.Features.EnableAdvancedAnalytics,
            "betafeatures" => config.Features.EnableBetaFeatures,
            _ => false
        };
    }

    /// <summary>
    /// Example of async configuration access from static context
    /// </summary>
    public static async Task<string> GetApiBaseUrlAsync(string environment = "")
    {
        var config = string.IsNullOrEmpty(environment) 
            ? await ConfigurationSingleton<ApplicationSettings>.GetConfigurationAsync()
            : await ConfigurationSingleton<ApplicationSettings>.GetConfigurationAsync(environment);
            
        return config.ApiBaseUrl;
    }
}
```

<llm-snippet-file>Agenix.Configuration/Sample/Approach3/README.md</llm-snippet-file>
```markdown
# Approach 3: Singleton

This approach demonstrates the singleton pattern for global configuration access using the `ConfigurationSingleton<T>` class.

## Key Characteristics

- **Global access**: Access configuration from anywhere in the application
- **No injection required**: No need for dependency injection or passing instances
- **Thread-safe**: Built-in thread safety with lazy initialization
- **High performance**: Lazy loading with caching support
- **Static methods**: All access through static methods

## When to Use

- **Legacy applications**: When working with existing code that doesn't use DI
- **Utility classes**: When you need configuration in static methods
- **Simple applications**: When you want minimal setup and maximum convenience
- **Performance critical**: When you need the fastest possible configuration access
- **Cross-cutting concerns**: When configuration is needed across many unrelated classes

## Pros

- ✅ **Extremely simple**: No dependency injection setup required
- ✅ **Global access**: Access from anywhere in the codebase
- ✅ **High performance**: Lazy initialization with thread safety
- ✅ **Minimal setup**: Just configure once at startup
- ✅ **Static friendly**: Works great with static methods and utilities
- ✅ **Memory efficient**: Only one instance per configuration type

## Cons

- ❌ **Testing challenges**: Harder to mock for unit tests
- ❌ **Global state**: Can make code harder to reason about
- ❌ **Tight coupling**: Creates implicit dependencies
- ❌ **Configuration immutable**: Cannot reconfigure after first use

## Usage Example

```csharp
// Configure once at application startup
ConfigurationSingleton<ApplicationSettings>.Configure(
    configurationName: "appsettings",
    configurationDirectory: "./Config",
    format: ConfigurationFormat.JSON,
    cachingEnabled: true,
    defaultEnvironment: "dev"
);

// Access from anywhere in your application
var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();
var devConfig = ConfigurationSingleton<ApplicationSettings>.GetConfiguration("dev");
```

## Configuration Methods

### Basic Configuration
```csharp
ConfigurationSingleton<ApplicationSettings>.Configure(
    configurationName: "appsettings",
    configurationDirectory: "./Config",
    format: ConfigurationFormat.JSON
);
```

### Full Configuration
```csharp
ConfigurationSingleton<ApplicationSettings>.Configure(
    logger: logger,
    configurationName: "appsettings",
    configurationDirectory: "./Config",
    format: ConfigurationFormat.JSON,
    environmentFileSupport: true,
    environmentFileDirectory: "./",
    environmentFileName: ".env",
    cachingEnabled: true,
    defaultEnvironment: "dev"
);
```

## Access Methods

### Synchronous Access
```csharp
// Current environment
var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();

// Specific environment
var devConfig = ConfigurationSingleton<ApplicationSettings>.GetConfiguration("dev");
```

### Asynchronous Access
```csharp
// Current environment
var config = await ConfigurationSingleton<ApplicationSettings>.GetConfigurationAsync();

// Specific environment
var devConfig = await ConfigurationSingleton<ApplicationSettings>.GetConfigurationAsync("dev");
```

### Configuration Management
```csharp
// Check if caching is enabled
bool isCached = ConfigurationSingleton<ApplicationSettings>.IsConfigurationCached;

// Get current environment
string currentEnv = ConfigurationSingleton<ApplicationSettings>.CurrentEnvironment;

// Reload configuration
ConfigurationSingleton<ApplicationSettings>.ReloadConfiguration();
ConfigurationSingleton<ApplicationSettings>.ReloadConfiguration("prod");
```

## Service Usage (No Injection Required)

```csharp
public class DatabaseService
{
    public void Connect()
    {
        // Access configuration directly - no injection needed!
        var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();
        var connectionString = config.DatabaseConnectionString;
        // Use connection string...
    }
}

public static class Utilities
{
    public static string GetApiUrl()
    {
        // Works great in static methods too!
        var config = ConfigurationSingleton<ApplicationSettings>.GetConfiguration();
        return config.ApiBaseUrl;
    }
}
```

## Best Practices

1. **Configure early**: Call `Configure()` at the very start of your application
2. **Configure once**: Never call `Configure()` more than once
3. **Use caching**: Enable caching for better performance
4. **Handle exceptions**: The first access will throw if configuration files are missing
5. **Environment variables**: Use environment variables for environment detection
6. **Static utilities**: Great for utility classes and static methods

## Testing Considerations

Since the singleton makes testing more challenging, consider:

1. **Integration tests**: Use integration tests instead of unit tests where possible
2. **Test configuration**: Create separate test configuration files
3. **Environment isolation**: Use different environments for testing
4. **Reset between tests**: Be careful about state between tests

```csharp
// In integration tests
ConfigurationSingleton<ApplicationSettings>.Configure(
    configurationName: "appsettings.test",
    configurationDirectory: "./TestConfig"
);
```

## Performance Characteristics

- **First access**: Lazy initialization - slight overhead on first call
- **Subsequent access**: Extremely fast - direct field access
- **Thread safety**: Uses `Lazy<T>` with `ExecutionAndPublication` mode
- **Memory usage**: Minimal - only one instance per configuration type

## Example Files

- `SingletonExample.cs` - Complete example with all features demonstrated
- `README.md` - This documentation
```

All example files are now created with comprehensive demonstrations of each approach, including:

1. **Models**: Complete `ApplicationSettings` with nested configuration objects
2. **Config files**: Environment-specific JSON configuration files
3. **Approach 1**: Direct instance creation and management
4. **Approach 2**: Dependency injection with service registration
5. **Approach 3**: Singleton pattern for global access
6. **Documentation**: Detailed README files explaining when and how to use each approach

Each approach includes:
- ✅ Basic usage examples
- ✅ Environment-specific configuration
- ✅ Async/await support
- ✅ Service integration examples
- ✅ Best practices and recommendations
- ✅ Pros and cons analysis

The examples are ready to use and demonstrate all the key features of your Agenix Configuration library!
