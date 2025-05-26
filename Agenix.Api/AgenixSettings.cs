using System.Configuration;
using System.Reflection;
using System.Xml.Linq;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace Agenix.Api;

/// <summary>
///     The AgenixSettings class provides various constants and settings for configuring the Agenix core library.
/// </summary>
public sealed class AgenixSettings
{
    
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AgenixSettings));
    
    private static ConfigurationFormat _activeFormat = ConfigurationFormat.NONE;
    private static string? _activeConfigPath;
    private static IConfiguration? _configuration;

    /// <summary>
    /// Specifies the configuration file format used for loading and managing settings.
    /// </summary>
    /// <remarks>
    /// Represents the supported types of configuration formats:
    /// - NONE: No specific format is used.
    /// - JSON: Indicates that the configuration is in JSON format.
    /// - XML: Indicates that the configuration is in XML format.
    /// - INI: Indicates that the configuration is in INI format.
    /// This enum is used in conjunction with configuration-building functionalities to
    /// determine the appropriate file loading behavior based on file format.
    /// </remarks>
    private enum ConfigurationFormat
    {
        NONE,
        JSON,
        XML,
        INI
    }

    /// <summary>
    /// Provides a centralized configuration management utility for the Agenix application framework.
    /// This class contains constants, defaults, and methods to manage, retrieve, and modify
    /// configuration properties, including logging, validation, and encoding settings.
    /// </summary>
    static AgenixSettings()
    {
        DetectAndLoadConfiguration();
    }

    
    /// <summary>
    ///     Prefix/sufix used to identify variable expressions
    /// </summary>
    public const string VariablePrefix = "${";

    public const string VariableSuffix = "}";

    public const string VariableEscape = "//";

    /// <summary>
    ///     Placeholder used in messages to ignore elements
    /// </summary>
    public const string IgnorePlaceholder = "@Ignore@";

    /// <summary>
    ///     Prefix/suffix used to identify validation matchers
    /// </summary>
    public const string ValidationMatcherPrefix = "@";

    public const string ValidationMatcherSuffix = "@";

    /// <summary>
    ///     Default logger modifier keywords
    /// </summary>
    public const string LogMaskKeywordsProp = "agenix.logger.mask.keywords";
    public const string LogMaskKeywordsDefault = "password,secret,secretKey";

    /// <summary>
    ///     Default logger modifier mask value
    /// </summary>
    public const string LogMaskValueProp = "agenix.logger.mask.value";
    public const string LogMaskValueDefault = "****";

    public const string LogModifierProp = "agenix.logger.modifier";
    public const string PrettyPrintProp = "agenix.message.pretty.print";
    public const string TypeConverterProp = "agenix.type.converter";
    public const string TypeConverterDefault = "default";

    public const string AgenixFileEncodingProp = "agenix.file.encoding";
    public const string DefaultMessageTypeProp = "agenix.default.message.type";
    public const string ReportAutoClearProp = "agenix.report.auto.clear";
    public const string ReportIgnoreErrorsProp = "agenix.report.ignore.errors";
    public const string ReportDirectoryProp = "agenix.report.directory";
    public const string TestNameVariableProp = "agenix.test.name.variable";
    public const string TestNameSpaceVariableProp = "agenix.test.namespace.variable";
    public const string DefaultConfigClassProp = "agenix.defalt.config.class";
    public const string ReportDirectoryDefault = "agenix-reports";
    public const string TestNameVariableDefault = "agenix.test.name";
    public const string TestNameSpaceVariableDefault = "agenix.test.namespace";

    private const string MessageValidationStrictProp = "agenix.json.message.validation.strict";
    public const string HttpMessageBuilderForceHeaderUpdateEnabledProp =
        "agenix.http.message.builder.force.header.update.enabled";
    public const string OutboundSchemaValidationEnabledProp = "agenix.validation.outbound.schema.enabled";
    public const string OutboundJsonSchemaValidationEnabledProp = "agenix.validation.outbound.json.schema.enabled";
    public const string OutboundXmlSchemaValidationEnabledProp = "agenix.validation.outbound.xml.schema.enabled";
    private const string ApplicationPropertyFileProperty = "agenix-application";

    //  
    // Flag to enable/disable fallback to default text equals validation
    //
    public const string PerformDefaultValidationProp = "agenix.perform.default.validation";
    public static readonly string LogModifierDefault = bool.TrueString;
    public static readonly string PrettyPrintDefault = bool.TrueString;
    public static readonly string ReportAutoClearDefault = bool.TrueString;
    public static readonly string ReportIgnoreErrorsDefault = bool.TrueString;
    private static readonly string MessageValidationStrictDefault = bool.TrueString;
    public static readonly string HttpMessageBuilderForceHeaderUpdateEnabledDefault = bool.TrueString;
    public static readonly string OutboundSchemaValidationEnabledDefault = bool.FalseString;
    public static readonly string OutboundJsonSchemaValidationEnabledDefault = bool.FalseString;
    public static readonly string OutboundXmlSchemaValidationEnabledDefault = bool.FalseString;
    public static readonly string PerformDefaultValidationDefault = bool.FalseString;

    /// <summary>
    ///     Represents the default configuration class used in the application. This value is retrieved from either
    ///     environment variables or configuration properties.
    /// </summary>
    public static string DefaultConfigClass()
    {
        return GetProperty(DefaultConfigClassProp, null);
    }

    /// <summary>
    ///     Indicates if JSON message validation should be performed in strict mode.
    /// </summary>
    public static bool JsonMessageValidationStrict()
    {
        return bool.Parse(GetProperty(MessageValidationStrictProp, MessageValidationStrictDefault));
    }

    /// <summary>
    ///     Defines the default encoding used for Agenix file operations.
    /// </summary>
    public static string AgenixFileEncoding()
    {
        return GetProperty(AgenixFileEncodingProp, "UTF-8");
    }

    /// <summary>
    /// Retrieves the file name of the default Agenix application property file. If no specific property is configured,
    /// defaults to "agenix-application.json".
    /// </summary>
    /// <returns>
    /// The name of the Agenix application property file.
    /// </returns>
    public static string GetAgenixApplicationPropertyFile()
    {
        return GetProperty(ApplicationPropertyFileProperty, "agenix-application");
    }
    
    // Add a method to reload configuration
    public static void ReloadConfiguration()
    {
        Log.LogInformation("Reloading configuration...");
        DetectAndLoadConfiguration();
    }

    /// <summary>
    /// Represents a collection of supported configuration formats,
    /// their file extensions, and associated load actions.
    /// </summary>
    private static readonly (ConfigurationFormat Format, string Extension, Func<IConfigurationBuilder, string, IConfigurationBuilder> Action)[] Formats = 
    {
        (Format: ConfigurationFormat.JSON, Extension: ".json", 
            Action: (b, p) => b.AddJsonFile(p, optional: true, reloadOnChange: true)),
    
        (Format: ConfigurationFormat.XML, Extension: ".xml", 
            Action: (b, p) => b.AddXmlFile(p, optional: true, reloadOnChange: true)),
    
        (Format: ConfigurationFormat.INI, Extension: ".ini", 
            Action: (b, p) => b.AddIniFile(p, optional: true, reloadOnChange: true))
    };


    /// <summary>
    /// Detects the appropriate configuration file format (e.g., JSON, XML, INI) and loads the corresponding configuration
    /// settings into the application. If no configuration file is found, defaults to using environment variables only.
    /// </summary>
    private static void DetectAndLoadConfiguration()
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(assemblyLocation);
        
        foreach (var (format, extension, addAction) in Formats)
        {
            var configPath = Path.Combine(assemblyLocation, $"{ApplicationPropertyFileProperty}{extension}");
            if (!File.Exists(configPath)) continue;
            _activeFormat = format;
            _activeConfigPath = configPath;
            Log.LogInformation("Using configuration file: {ConfigPath}", configPath);
                
            // Add the configuration file to the builder
            addAction(configBuilder, configPath);

            // Add Environment Variables as fallback
            configBuilder.AddEnvironmentVariables();

            // Build the configuration
            _configuration = configBuilder.Build();

            // Load initial values into the ConfigurationManager if needed
            LoadConfigurationIntoAppSettings();
                
            break;
        }

        if (_activeFormat == ConfigurationFormat.NONE)
        {
            Log.LogWarning("No configuration file found. Using environment variables only.");
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }
    }


    /// <summary>
    /// Loads configuration values from the current configuration source into the application's AppSettings structure.
    /// This method iterates over all key-value pairs in the active configuration, validating that each value is not null or empty,
    /// and then assigns it to the application's property system using the appropriate setter.
    /// Any errors encountered during this process are logged for troubleshooting.
    /// </summary>
    private static void LoadConfigurationIntoAppSettings()
    {
        try
        {
            foreach (var kvp in _configuration.AsEnumerable()
                         .Where(x => !string.IsNullOrEmpty(x.Value)))
            {
                if (Log.IsEnabled(LogLevel.Trace))
                {
                    Log.LogTrace("Loading configuration: {Key}={Value}", kvp.Key, kvp.Value);
                }
                
                SetProperty(kvp.Key, kvp.Value);
            }
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error loading configuration into AppSettings");
        }
    }

    /// <summary>
    /// Sets a configuration property based on the specified key and value. The setting behavior is determined by the
    /// currently active configuration format (e.g., JSON, XML, Ini).
    /// </summary>
    /// <param name="key">
    /// The key identifying the configuration property to be set.
    /// </param>
    /// <param name="value">
    /// The value to assign to the configuration property identified by the key.
    /// </param>
    public static void SetProperty(string key, string value)
    {
        switch (_activeFormat)
        {
            case ConfigurationFormat.JSON:
                SetJsonProperty(key, value);
                break;
            case ConfigurationFormat.XML:
                SetXmlProperty(key, value);
                break;
            case ConfigurationFormat.INI:
                SetIniProperty(key, value);
                break;
            case ConfigurationFormat.NONE:
            default:
                SetAppConfigProperty(key, value);
                break;
        }
    }

    /// <summary>
    /// Sets or updates a property in a JSON configuration file. Supports nested properties
    /// using dot-separated keys.
    /// </summary>
    /// <param name="key">The key of the property to set or update, using dot notation for nested properties.</param>
    /// <param name="value">The value to assign to the specified property.</param>
    private static void SetJsonProperty(string key, string value)
    {
        try
        {
            var json = File.Exists(_activeConfigPath) 
                ? JObject.Parse(File.ReadAllText(_activeConfigPath)) 
                : new JObject();

            // Handle nested properties
            var parts = key.Split('.');
            var current = json;
            
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (current?[parts[i]] == null || current[parts[i]] is not { Type: JTokenType.Object })
                {
                    current[parts[i]] = new JObject();
                }
                current = (JObject)current[parts[i]];
            }

            current[parts[^1]] = value;

            File.WriteAllText(_activeConfigPath, json.ToString((Formatting.Indented)));
            Log.LogTrace("Updated JSON property {Key}={Value}", key, value);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to set JSON property {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Updates or adds a property in the XML configuration file for the application.
    /// </summary>
    /// <param name="key">
    /// The key of the property to update or add in the XML configuration file.
    /// </param>
    /// <param name="value">
    /// The value to be set for the specified property key in the XML configuration file.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown if the XML configuration file cannot be updated or saved.
    /// </exception>
    private static void SetXmlProperty(string key, string value)
    {
        try
        {
            XDocument doc;
            if (File.Exists(_activeConfigPath))
            {
                doc = XDocument.Load(_activeConfigPath);
            }
            else
            {
                doc = new XDocument(
                    new XElement("configuration",
                        new XElement("appSettings"))
                );
            }

            var appSettings = doc.Root?.Element("appSettings");
            if (appSettings == null)
            {
                doc.Root?.Add(new XElement("appSettings"));
                appSettings = doc.Root?.Element("appSettings");
            }

            var setting = appSettings?.Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == key);

            if (setting == null)
            {
                appSettings?.Add(
                    new XElement("add",
                        new XAttribute("key", key),
                        new XAttribute("value", value))
                );
            }
            else
            {
                setting.Attribute("value")!.Value = value;
            }

            doc.Save(_activeConfigPath);
            Log.LogTrace("Updated XML property {Key}={Value}", key, value);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to set XML property {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Updates a property within an INI configuration file. If the specified section or property does not exist,
    /// it will be created. The function operates on the active configuration path defined for the application.
    /// </summary>
    /// <param name="key">
    /// The full key of the property to update or create. Keys can include section names
    /// separated by a period, with the portion before the first period representing the section name.
    /// If no section is provided, the "Default" section is used.
    /// </param>
    /// <param name="value">
    /// The value to assign to the specified key within the configuration file.
    /// </param>
    /// <exception cref="IOException">
    /// Thrown if there is an issue reading or writing to the configuration file.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown if the application lacks the necessary permissions to access the configuration file.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown for any other errors occurring during the update process, including logging the failure.
    /// </exception>
    private static void SetIniProperty(string key, string value)
    {
        try
        {
            var lines = File.Exists(_activeConfigPath)
                ? File.ReadAllLines(_activeConfigPath).ToList()
                : [];

            var sections = key.Split('.');
            var section = sections.Length > 1 ? sections[0] : "Default";
            var propertyKey = sections.Length > 1 ? string.Join(".", sections.Skip(1)) : key;

            var sectionIndex = lines.FindIndex(l => l.Trim() == $"[{section}]");
            var propertyLine = $"{propertyKey}={value}";

            if (sectionIndex == -1)
            {
                // Add a new section
                if (lines.Count > 0) lines.Add("");
                lines.Add($"[{section}]");
                lines.Add(propertyLine);
            }
            else
            {
                // Find property in the section
                var nextSectionIndex = lines.FindIndex(sectionIndex + 1, 
                    l => l.StartsWith('[') && l.EndsWith(']'));
                if (nextSectionIndex == -1) nextSectionIndex = lines.Count;

                var propertyIndex = lines.FindIndex(sectionIndex + 1, nextSectionIndex - sectionIndex - 1,
                    l => l.StartsWith($"{propertyKey}="));

                if (propertyIndex == -1)
                {
                    lines.Insert(sectionIndex + 1, propertyLine);
                }
                else
                {
                    lines[propertyIndex] = propertyLine;
                }
            }

            File.WriteAllLines(_activeConfigPath, lines);
            Log.LogTrace("Updated INI property {Key}={Value}", key, value);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to set INI property {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Updates or adds a key-value pair in the application's app.config file.
    /// This method ensures that the configuration settings are saved and refreshed
    /// to allow changes to take effect.
    /// </summary>
    /// <param name="key">The key of the configuration property to be added or updated.</param>
    /// <param name="value">The value to be associated with the specified key.</param>
    /// <exception cref="Exception">Thrown if the operation fails to update the app.config file.</exception>
    private static void SetAppConfigProperty(string key, string value)
    {
        try
        {
            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            
            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
            }
            
            config.Save(ConfigurationSaveMode.Modified);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");

            if (Log.IsEnabled(LogLevel.Trace))
            {
                Log.LogTrace("Updated {config.FilePath} property {Key}={Value}", config.FilePath, key, value);
            }
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to set app.config property {Key}", key);
            throw;
        }
    }

    
    // Add a method to get all configuration entries
    /// <summary>
    /// Retrieves all configuration properties from the current configuration source, including their keys and values.
    /// </summary>
    /// <returns>
    /// An enumerable collection of key-value pairs representing the configuration properties.
    /// If no configuration source is available, returns an empty collection.
    /// </returns>
    public static IEnumerable<KeyValuePair<string, string?>> GetAllProperties()
    {
        return _configuration?.AsEnumerable() ?? [];
    }


    /// <summary>
    ///     Default message type used for the application.
    /// </summary>
    public static string DefaultMessageType()
    {
        return GetProperty(DefaultMessageTypeProp, nameof(MessageType.JSON));
    }

    /// <summary>
    ///     Specifies whether the report should be automatically cleared after processing.
    /// </summary>
    public static bool ReportAutoClear()
    {
        return bool.Parse(GetProperty(ReportAutoClearProp, ReportAutoClearDefault));
    }

    /// <summary>
    ///     Indicates whether the HTTP message builder is configured to forcefully update
    ///     headers during the message building process.
    /// </summary>
    public static bool IsHttpMessageBuilderForceHeaderUpdateEnabled()
    {
        return bool.Parse(GetProperty(
            HttpMessageBuilderForceHeaderUpdateEnabledProp,
            HttpMessageBuilderForceHeaderUpdateEnabledDefault
        ));
    }

    /// <summary>
    ///     Determines whether errors should be ignored in the report generation process.
    /// </summary>
    public static bool ReportIgnoreErrors()
    {
        return bool.Parse(GetProperty(ReportIgnoreErrorsProp, ReportIgnoreErrorsDefault));
    }

    /// <summary>
    ///     Directory where the report files are stored
    /// </summary>
    public static string ReportDirectory()
    {
        return GetProperty(ReportDirectoryProp, ReportDirectoryDefault
        );
    }

    /// <summary>
    ///     Represents the name of the test variable.
    /// </summary>
    public static string TestNameVariable()
    {
        return GetProperty(TestNameVariableProp, TestNameVariableDefault);
    }

    /// <summary>
    ///     Specifies the default value for the test namespace variable configuration
    /// </summary>
    public static string TestNameSpaceVariable()
    {
        return GetProperty(TestNameSpaceVariableProp, TestNameSpaceVariableDefault);
    }


    /// <summary>
    ///     Get logger mask keywords.
    /// </summary>
    /// <returns>the set of mask keywords</returns>
    public static HashSet<string> GetLogMaskKeywords()
    {
        return GetProperty(
                LogMaskKeywordsProp,
                LogMaskKeywordsDefault)
            .Split(',')
            .Select(keyword => keyword.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Get logger mask value.
    /// </summary>
    /// <returns></returns>
    public static string GetLogMaskValue()
    {
        return GetProperty(LogMaskValueProp, LogMaskValueDefault);
    }

    /// <summary>
    ///     Gets the type converter to use by default.
    /// </summary>
    /// <returns></returns>
    public static string GetTypeConverter()
    {
        return GetProperty(TypeConverterProp, TypeConverterDefault);
    }

    /// <summary>
    ///     Gets in the respective order, a system property, an environment variable, or the default
    /// </summary>
    /// <param name="prop">the name of the system property to get</param>
    /// <param name="env">the name of the environment variable to get</param>
    /// <param name="def">the default value</param>
    /// <returns>the first value encountered, which is not null. May return null, if default value is null.</returns>
    private static string GetPropertyEnvOrDefault(string prop, string env, string def)
    {
        return System.Configuration.ConfigurationManager.AppSettings[prop] ??
               Environment.GetEnvironmentVariable(env) ??
               def;
    }
    
    public static string GetProperty(string key, string defaultValue = "")
    {
        var value = _configuration?[key] ?? 
                    GetPropertyEnvOrDefault(key, ToEnvironmentVariableName(key), defaultValue);

        if (Log.IsEnabled(LogLevel.Trace))
        {
            Log.LogTrace("Getting property {Key}={Value}", key, value);
        }
        
        return value;
    }

    /// <summary>
    /// Converts a given property key to its corresponding environment variable name.
    /// The conversion involves replacing dot notation with underscores and transforming
    /// the string to uppercase.
    /// </summary>
    /// <param name="key">The property key to be converted into environment variable name format.</param>
    /// <returns>The transformed environment variable name that corresponds to the given property key.</returns>
    private static string ToEnvironmentVariableName(string key)
    {
        return key.ToUpperInvariant().Replace(".", "_");
    }
    
    /// <summary>
    ///     Gets the logger modifier enabled/ disabled setting.
    /// </summary>
    /// <returns></returns>
    public static bool IsLogModifierEnabled()
    {
        var propertyValue = GetProperty(
            LogModifierProp,
            LogModifierDefault);

        return bool.Parse(propertyValue);
    }

    /// <summary>
    ///     Determines whether the default validation process should be performed based on configuration properties
    ///     or environment variables. The value is retrieved as a boolean from the corresponding settings.
    /// </summary>
    /// <returns>
    ///     True if the default validation is enabled; otherwise, false.
    /// </returns>
    public static bool IsPerformDefaultValidation()
    {
        var propertyValue = GetProperty(
            PerformDefaultValidationProp,
            PerformDefaultValidationDefault);

        return bool.Parse(propertyValue);
    }

    /// <summary>
    ///     Gets the message payload pretty print enabled/ disabled setting.
    /// </summary>
    /// <returns></returns>
    public static bool IsPrettyPrintEnabled()
    {
        var propertyValue = GetProperty(
            PrettyPrintProp,
            PrettyPrintDefault);

        return bool.Parse(propertyValue);
    }

    /// <summary>
    ///     Determines whether outbound schema validation is enabled. This setting is controlled through
    ///     a combination of properties and environment variables, with a default fallback value.
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether outbound schema validation is currently enabled.
    /// </returns>
    public static bool IsOutboundSchemaValidationEnabled()
    {
        var propertyValue = GetProperty(
            OutboundSchemaValidationEnabledProp,
            OutboundSchemaValidationEnabledDefault);

        return bool.Parse(propertyValue);
    }

    /// <summary>
    ///     Indicates whether outbound JSON schema validation is enabled. This value is determined
    ///     based on configuration properties, environment variables, or a default value.
    /// </summary>
    /// <returns>
    ///     A boolean value where true indicates that outbound JSON schema validation is enabled,
    ///     and false indicates it is disabled.
    /// </returns>
    public static bool IsOutboundJsonSchemaValidationEnabled()
    {
        var propertyValue = GetProperty(
            OutboundJsonSchemaValidationEnabledProp,
            OutboundJsonSchemaValidationEnabledDefault);

        return bool.Parse(propertyValue);
    }

    /// <summary>
    ///     Determines whether the validation for outbound XML messages against a schema is enabled.
    ///     The value is fetched as a combination of property, environment variable, or default setting.
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether outbound XML schema validation is enabled.
    ///     Returns true if validation is enabled, otherwise false.
    /// </returns>
    public static bool IsOutboundXmlSchemaValidationEnabled()
    {
        var propertyValue = GetProperty(
            OutboundXmlSchemaValidationEnabledProp,
            OutboundXmlSchemaValidationEnabledDefault);

        return bool.Parse(propertyValue);
    }
}