namespace Agenix.Core;

/// <summary>
///     Interface that marks a test case to support group bindings.
///     These bindings are used to enable or disable tests based on group settings.
/// </summary>
public interface ITestGroupAware
{
    /// <summary>
    ///     Gets the groups associated with the test case.
    /// </summary>
    /// <returns>
    ///     An array of strings representing the groups.
    /// </returns>
    string[] GetGroups();

    /// <summary>
    ///     Sets the test groups associated with the test case.
    /// </summary>
    /// <param name="groups">
    ///     An array of strings representing the groups to be associated with the test case.
    /// </param>
    void SetGroups(string[] groups);
}