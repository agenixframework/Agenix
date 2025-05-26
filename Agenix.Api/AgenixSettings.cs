using System.Configuration;
using Agenix.Api.Message;

namespace Agenix.Api;

/// <summary>
///     The AgenixSettings class provides various constants and settings for configuring the Agenix core library.
/// </summary>
public sealed class AgenixSettings
{
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

    public const string LogMaskKeywordsEnv = "AGENIX_LOG_MASK_KEYWORDS";
    public const string LogMaskKeywordsDefault = "password,secret,secretKey";

    /// <summary>
    ///     Default logger modifier mask value
    /// </summary>
    public const string LogMaskValueProp = "agenix.logger.mask.value";

    public const string LogMaskValueEnv = "AGENIX_LOG_MASK_VALUE";
    public const string LogMaskValueDefault = "****";

    public const string LogModifierProp = "agenix.logger.modifier";
    public const string LogModifierEnv = "AGENIX_LOG_MODIFIER";

    public const string PrettyPrintProp = "agenix.message.pretty.print";
    public const string PrettyPrintEnv = "AGENIX_MESSAGE_PRETTY_PRINT";

    public const string TypeConverterProp = "agenix.type.converter";
    public const string TypeConverterEnv = "AGENIX_TYPE_CONVERTER";
    public const string TypeConverterDefault = "default";

    public const string AgenixFileEncodingProp = "agenix.file.encoding";
    public const string AgenixFileEncodingEnv = "AGENIX_FILE_ENCODING";

    public const string DefaultMessageTypeProp = "agenix.default.message.type";
    public const string DefaultMessageTypeEnv = "AGENIX_DEFAULT_MESSAGE_TYPE";

    public const string ReportAutoClearProp = "agenix.report.auto.clear";
    public const string ReportAutoClearEnv = "AGENIX_REPORT_AUTO_CLEAR";

    public const string ReportIgnoreErrorsProp = "agenix.report.ignore.errors";
    public const string ReportIgnoreErrorsEnv = "AGENIX_REPORT_IGNORE_ERRORS";

    public const string ReportDirectoryProp = "agenix.report.directory";
    public const string ReportDirectoryErrorsEnv = "AGENIX_REPORT_DIRECTORY";

    public const string TestNameVariableProp = "agenix.test.name.variable";
    public const string TestNameVariableEnv = "AGENIX_TEST_NAME_VARIABLE";

    public const string TestNameSpaceVariableProp = "agenix.test.namespace.variable";
    public const string TestNameSpaceVariableEnv = "AGENIX_TEST_NAMESPAE_VARIABLE";

    public const string DefaultConfigClassProp = "agenix.defalt.config.class";
    public const string DefaultConfigClassEnv = "AGENIX_DEFAULT_CONFIG_CLASS";
    public const string ReportDirectoryDefault = "agenix-reports";
    public const string TestNameVariableDefault = "agenix.test.name";
    public const string TestNameSpaceVariableDefault = "agenix.test.namespace";

    private const string MessageValidationStrictProp = "agenix.json.message.validation.strict";
    private const string MessageValidationStrictEnv = "AGENIX_JSON_MESSAGE_VALIDATION_STRICT";

    public const string HttpMessageBuilderForceHeaderUpdateEnabledProp =
        "agenix.http.message.builder.force.header.update.enabled";

    public const string HttpMessageBuilderForceHeaderUpdateEnabledEnv =
        "AGENIX_HTTP_MESSAGE_BUILDER_FORCE_HEADER_UPDATE_ENABLED";

    public const string OutboundSchemaValidationEnabledProp = "agenix.validation.outbound.schema.enabled";
    public const string OutboundSchemaValidationEnabledEnv = "AGENIX_VALIDATION_OUTBOUND_SCHEMA_ENABLED";

    public const string OutboundJsonSchemaValidationEnabledProp = "agenix.validation.outbound.json.schema.enabled";
    public const string OutboundJsonSchemaValidationEnabledEnv = "AGENIX_VALIDATION_OUTBOUND_JSON_SCHEMA_ENABLED";

    public const string OutboundXmlSchemaValidationEnabledProp = "agenix.validation.outbound.xml.schema.enabled";
    public const string OutboundXmlSchemaValidationEnabledEnv = "AGENIX_VALIDATION_OUTBOUND_XML_SCHEMA_ENABLED";

    //  
    // Flag to enable/disable fallback to default text equals validation
    //
    public const string PerformDefaultValidationProp = "agenix.perform.default.validation";
    public const string PerformDefaultValidationEnv = "AGENIX_PERFORM_DEFAULT_VALIDATION";
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
        return GetPropertyEnvOrDefault(
            DefaultConfigClassProp,
            DefaultConfigClassEnv,
            null);
    }

    /// <summary>
    ///     Indicates if JSON message validation should be performed in strict mode.
    /// </summary>
    public static bool JsonMessageValidationStrict()
    {
        return bool.Parse(GetPropertyEnvOrDefault(
            MessageValidationStrictProp,
            MessageValidationStrictEnv,
            MessageValidationStrictDefault
        ));
    }

    /// <summary>
    ///     Defines the default encoding used for Agenix file operations.
    /// </summary>
    public static string AgenixFileEncoding()
    {
        return GetPropertyEnvOrDefault(
            AgenixFileEncodingProp,
            AgenixFileEncodingEnv,
            "UTF-8");
    }

    /// <summary>
    ///     Default message type used for the application.
    /// </summary>
    public static string DefaultMessageType()
    {
        return GetPropertyEnvOrDefault(
            DefaultMessageTypeProp,
            DefaultMessageTypeEnv,
            nameof(MessageType.JSON)
        );
    }

    /// <summary>
    ///     Specifies whether the report should be automatically cleared after processing.
    /// </summary>
    public static bool ReportAutoClear()
    {
        return bool.Parse(GetPropertyEnvOrDefault(
            ReportAutoClearProp,
            ReportAutoClearEnv,
            ReportAutoClearDefault
        ));
    }

    /// <summary>
    ///     Indicates whether the HTTP message builder is configured to forcefully update
    ///     headers during the message building process.
    /// </summary>
    public static bool IsHttpMessageBuilderForceHeaderUpdateEnabled()
    {
        return bool.Parse(GetPropertyEnvOrDefault(
            HttpMessageBuilderForceHeaderUpdateEnabledProp,
            HttpMessageBuilderForceHeaderUpdateEnabledEnv,
            HttpMessageBuilderForceHeaderUpdateEnabledDefault
        ));
    }

    /// <summary>
    ///     Determines whether errors should be ignored in the report generation process.
    /// </summary>
    public static bool ReportIgnoreErrors()
    {
        return bool.Parse(GetPropertyEnvOrDefault(
            ReportIgnoreErrorsProp,
            ReportIgnoreErrorsEnv,
            ReportIgnoreErrorsDefault
        ));
    }

    /// <summary>
    ///     Directory where the report files are stored
    /// </summary>
    public static string ReportDirectory()
    {
        return GetPropertyEnvOrDefault(
            ReportDirectoryProp,
            ReportDirectoryErrorsEnv,
            ReportDirectoryDefault
        );
    }

    /// <summary>
    ///     Represents the name of the test variable.
    /// </summary>
    public static string TestNameVariable()
    {
        return GetPropertyEnvOrDefault(
            TestNameVariableProp,
            TestNameVariableEnv,
            TestNameVariableDefault
        );
    }

    /// <summary>
    ///     Specifies the default value for the test namespace variable configuration
    /// </summary>
    public static string TestNameSpaceVariable()
    {
        return GetPropertyEnvOrDefault(
            TestNameSpaceVariableProp,
            TestNameSpaceVariableEnv,
            TestNameSpaceVariableDefault
        );
    }


    /// <summary>
    ///     Get logger mask keywords.
    /// </summary>
    /// <returns>the set of mask keywords</returns>
    public static HashSet<string> GetLogMaskKeywords()
    {
        return GetPropertyEnvOrDefault(
                LogMaskKeywordsProp,
                LogMaskKeywordsEnv,
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
        return GetPropertyEnvOrDefault(LogMaskValueProp, LogMaskValueEnv, LogMaskValueDefault);
    }

    /// <summary>
    ///     Gets the type converter to use by default.
    /// </summary>
    /// <returns></returns>
    public static string GetTypeConverter()
    {
        return GetPropertyEnvOrDefault(TypeConverterProp, TypeConverterEnv, TypeConverterDefault);
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
        return ConfigurationManager.AppSettings[prop] ??
               Environment.GetEnvironmentVariable(env) ??
               def;
    }

    /// <summary>
    ///     Gets the logger modifier enabled/ disabled setting.
    /// </summary>
    /// <returns></returns>
    public static bool IsLogModifierEnabled()
    {
        var propertyValue = GetPropertyEnvOrDefault(
            LogModifierProp,
            LogModifierEnv,
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
        var propertyValue = GetPropertyEnvOrDefault(
            PerformDefaultValidationProp,
            PerformDefaultValidationEnv,
            PerformDefaultValidationDefault);

        return bool.Parse(propertyValue);
    }

    /// <summary>
    ///     Gets the message payload pretty print enabled/ disabled setting.
    /// </summary>
    /// <returns></returns>
    public static bool IsPrettyPrintEnabled()
    {
        var propertyValue = GetPropertyEnvOrDefault(
            PrettyPrintProp,
            PrettyPrintEnv,
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
        var propertyValue = GetPropertyEnvOrDefault(
            OutboundSchemaValidationEnabledProp,
            OutboundSchemaValidationEnabledEnv,
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
        var propertyValue = GetPropertyEnvOrDefault(
            OutboundJsonSchemaValidationEnabledProp,
            OutboundJsonSchemaValidationEnabledEnv,
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
        var propertyValue = GetPropertyEnvOrDefault(
            OutboundXmlSchemaValidationEnabledProp,
            OutboundXmlSchemaValidationEnabledEnv,
            OutboundXmlSchemaValidationEnabledDefault);

        return bool.Parse(propertyValue);
    }
}