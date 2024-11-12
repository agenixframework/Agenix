namespace Agenix.Core.Container;

/*
 * Interface representing actions that should be executed before a test suite runs.
 */
/// <summary>
///     Interface representing actions that should be executed before a test suite runs.
/// </summary>
public interface IBeforeSuite : ITestActionContainer
{
    /// <summary>
    ///     Checks if this suite actions should execute according to suite name and included test groups.
    /// </summary>
    /// <param name="suiteName">The name of the test suite.</param>
    /// <param name="includedGroups">The groups included in the test suite.</param>
    /// <return>True if the actions should execute, otherwise false.</return>
    bool ShouldExecute(string suiteName, string[] includedGroups);
}