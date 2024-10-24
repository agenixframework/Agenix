namespace Agenix.Core
{
    /// <summary>
    ///     The test context provides utility methods for replacing dynamic content(variables and functions) in string
    /// </summary>
    public interface ITestAction
    {
        /// <summary>
        ///     Main execution method doing all work
        /// </summary>
        void Execute(TestContext context);

        /// <summary>
        /// Name of test action injected as Spring bean name
        /// </summary>
        /// <returns>name as String</returns>
        string Name => GetType().Name;

        /// <summary>
        /// Checks if this action is disabled.
        /// </summary>
        /// <param name="context">the current test context.</param>
        /// <returns>true if action is marked disabled.</returns>
        bool IsDisabled(TestContext context)
        {
            return false;
        }
    }
}