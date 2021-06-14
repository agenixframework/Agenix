namespace FleetPay.Core
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
    }
}