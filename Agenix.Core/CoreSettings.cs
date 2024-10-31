using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Agenix.Core.Message;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace Agenix.Core;

public sealed class CoreSettings
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
    public const string IgnorePlaceholder = "@ignore@";

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
    public static string LogModifierDefault = bool.TrueString;
    public static string PrettyPrintDefault = bool.TrueString;

    public static string AgenixFileEncoding = GetPropertyEnvOrDefault(
        AgenixFileEncodingProp,
        AgenixFileEncodingEnv,
        "UTF-8");

    public static string DefaultMessageType = GetPropertyEnvOrDefault(
        DefaultMessageTypeProp,
        DefaultMessageTypeEnv,
        MessageType.JSON.ToString()
    );


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
    ///     Gets in the respective order, a system property, an environment variable or the default
    /// </summary>
    /// <param name="prop">the name of the system property to get</param>
    /// <param name="env">the name of the environment variable to get</param>
    /// <param name="def">the default value</param>
    /// <returns>first value encountered, which is not null. May return null, if default value is null.</returns>
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
}