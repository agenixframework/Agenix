namespace Agenix.Core.Container;

/// Provides functionality to define actions that should be executed after a test suite.
/// /
public interface IAfterSuite : ITestActionContainer
{
    /// Checks if this suite actions should execute according to suite name and included test groups.
    /// @param suiteName The name of the suite to be checked for execution.
    /// @param includedGroups The groups of tests that are included for execution.
    /// @return True if the suite should execute based on the provided parameters; otherwise, false.
    /// /
    bool ShouldExecute(string suiteName, string[] includedGroups);
}