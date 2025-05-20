namespace Agenix.Api.Container;

/// <summary>
///     Represents an interface that defines a set of actions to be performed before test execution.
/// </summary>
public interface IBeforeTest : ITestActionContainer
{
    /// <summary>
    ///     Checks if this suite action should execute, according to suite name and included test groups.
    /// </summary>
    /// <param name="testName">The name of the test.</param>
    /// <param name="packageName">The name of the package.</param>
    /// <param name="includedGroups">The groups of tests included.</param>
    /// <returns>Returns true if the actions should execute; otherwise false.</returns>
    bool ShouldExecute(string testName, string packageName, string[] includedGroups);
}