namespace Agenix.Api.Container;

/// Interface representing an after-test action container.
/// Provides a method to determine if the contained actions should execute based on
/// the suite name and test groups.
/// /
public interface IAfterTest : ITestActionContainer
{
    /// Checks if this suite action should execute, according to suite name and included test groups.
    /// @param testName The name of the test.
    /// @param packageName The name of the package.
    /// @param includedGroups The groups included in the test suite.
    /// @return A boolean indicating whether the suite actions should execute.
    /// /
    bool ShouldExecute(string testName, string packageName, string[] includedGroups);
}