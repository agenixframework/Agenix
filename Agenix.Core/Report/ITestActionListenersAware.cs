namespace Agenix.Core.Report;

/// Provides functionality to be aware of test action listeners.
internal interface ITestActionListenersAware
{
    // Sets the test action listeners
    /// Sets the test action listeners.
    /// <param name="testActionListeners">
    ///     An instance of TestActionListeners that handles broadcasting of test action events to
    ///     all registered listeners.
    /// </param>
    void SetTestActionListeners(TestActionListeners testActionListeners);
}