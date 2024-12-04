namespace Agenix.Core;

/// Behavior applies logic to given test action runner.
/// /
public delegate void TestBehavior(ITestActionRunner runner);

/// <summary>
///     Behavior applies logic to given test action runner.
/// </summary>
public interface ITestBehavior
{
    /// <summary>
    ///     Behavior building method.
    /// </summary>
    /// <param name="runner"></param>
    void Apply(ITestActionRunner runner);
}