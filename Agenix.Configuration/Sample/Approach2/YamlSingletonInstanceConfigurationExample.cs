using System.Configuration;
using Agenix.Configuration.Core.Models;
using Agenix.Configuration.Sample.Models;
using Agenix.Configuration.Singleton;

namespace Agenix.Configuration.Sample.Approach2;

/// <summary>
/// Represents an example implementation for configuring and using a YAML-based configuration system
/// leveraging the <see cref="ConfigurationSingleton{T}"/> class.
/// This class demonstrates how to load and access application settings
/// from YAML configuration files in a structured and reusable manner.
/// </summary>
public class YamlConfigurationExample
{
    /// <summary>
    /// Sets up the YAML configuration for the application, including necessary parameters
    /// such as the configuration name, directory, format, caching, and environment.
    /// This method uses the ConfigurationSingleton to initialize and manage the configuration
    /// in YAML format and provides access to the loaded configurations for further usage.
    /// </summary>
    public void SetupYamlConfiguration()
    {
        ConfigurationSingleton<ApplicationSettings>.Configure(new ConfigurationSingleton<ApplicationSettings>.ConfSettings
        {
            ConfigurationName = "appsettings",
            ConfigurationDirectory = "./Sample/Config",
            Format = ConfigurationFormat.YAML,
            CachingEnabled = true,
            DefaultEnvironment = "dev"
        });

        Console.WriteLine("Configuration has been set up...");

        var configManager = ConfigurationSingleton<ApplicationSettings>.Instance;
        Console.WriteLine("Got config manager instance...");
        Console.WriteLine($"Current Environment: {configManager.CurrentEnvironment}");

        var config = configManager.GetConfiguration();
        Console.WriteLine("Got configuration object...");

        //  Add detailed debugging
        Console.WriteLine("=== Configuration Details ===");
        Console.WriteLine($"Type of config: {config.GetType().Name}");
        Console.WriteLine($"Database Connection: '{config.DatabaseConnectionString}'");
        Console.WriteLine($"Database Connection Length: {config.DatabaseConnectionString.Length}");
        Console.WriteLine($"API Base URL: '{config.ApiBaseUrl}'");
        Console.WriteLine($"API Base URL Length: {config.ApiBaseUrl.Length}");
        Console.WriteLine($"Cache Provider: '{config.Cache.Provider}'");
        Console.WriteLine($"Log Level: '{config.Logging.Level}'");
    }
}
