namespace FleetPay.Core.Validation.Matcher
{
    /// <summary>
    ///     Default registry automatically adds default validation matcher library.
    /// </summary>
    public class DefaultValidationMatcherRegistry : ValidationMatcherRegistry
    {
        /// <summary>
        ///     Constructor initializes with default validation matcher library.
        /// </summary>
        public DefaultValidationMatcherRegistry()
        {
            AddValidationMatcherLibrary(new DefaultValidationMatcherLibrary());
        }
    }
}