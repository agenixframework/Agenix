using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Agenix.Core
{
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
        /// Default logger modifier keywords
        /// </summary>
        public const string LogMaskKeywordsProp = "agenix.logger.mask.keywords";
        public const string LogMaskKeywordsEnv = "AGENIX_LOG_MASK_KEYWORDS";
        public const string LogMaskKeywordsDefault = "password,secret,secretKey";

        /// <summary>
        /// Default logger modifier mask value
        /// </summary>
        public const string LogMaskValueProp = "agenix.logger.mask.value";
        public const string LogMaskValueEnv = "AGENIX_LOG_MASK_VALUE";
        public const string LogMaskValueDefault = "****";

        /// <summary>
        /// Get logger mask keywords.
        /// </summary>
        /// <returns>the set of mask keywords</returns>
        public static HashSet<string> GetLogMaskKeywords()
        {
            return CoreSettings.GetPropertyEnvOrDefault(
                        LogMaskKeywordsProp,
                        LogMaskKeywordsEnv,
                        LogMaskKeywordsDefault)
                    .Split(',')
                    .Select(keyword => keyword.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public static String GetLogMaskValue()
        {
            return GetPropertyEnvOrDefault(LogMaskValueProp, LogMaskValueEnv, LogMaskKeywordsDefault);
        }

        /// <summary>
        /// Gets in the respective order, a system property, an environment variable or the default
        /// </summary>
        /// <param name="prop">the name of the system property to get</param>
        /// <param name="env">the name of the environment variable to get</param>
        /// <param name="def">the default value</param>
        /// <returns>first value encountered, which is not null. May return null, if default value is null.</returns>
        private static string GetPropertyEnvOrDefault(string prop, string env, string def)
        {
            return ConfigurationManager.AppSettings[prop] ??
                   System.Environment.GetEnvironmentVariable(env) ??
                   def;
        }
    }
}