namespace Agenix.Core.Report;

/// Defines a contract for components aware of test suite listeners.
/// /
public interface ITestSuiteListenerAware
{
    /// Adds a new suite listener.
    /// @param suiteListener An instance of ITestSuiteListener to be added.
    /// /
    void AddTestSuiteListener(ITestSuiteListener suiteListener);
}