using Agenix.Api.Context;

namespace Agenix.Api;

/// <summary>
///     The test context provides utility methods for replacing dynamic content(variables and functions) in string
/// </summary>
public interface ITestAction
{
    /// <summary>
    ///     Name of test action injected as Spring bean name
    /// </summary>
    /// <returns>name as String</returns>
    string Name => GetType().Name;

    /// <summary>
    ///     Main execution method doing all work
    /// </summary>
    void Execute(TestContext context);

    /// <summary>
    ///     Checks if this action is disabled.
    /// </summary>
    /// <param name="context">the current test context.</param>
    /// <returns>true if action is marked disabled.</returns>
    bool IsDisabled(TestContext context)
    {
        return false;
    }
}

/// <summary>
///     Delegate for creating an instance of an ITestAction based on a given TestContext.
/// </summary>
public delegate void TestAction(TestContext context);