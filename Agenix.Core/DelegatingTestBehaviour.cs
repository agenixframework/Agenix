using Agenix.Api;

namespace Agenix.Core;

/// <summary>
///     Represents a test behavior that executes by delegating work to another specified test action.
/// </summary>
/// <param name="testBehavior">The test behavior to which the work is delegated.</param>
public class DelegatingTestBehaviour(TestBehavior testBehavior) : ITestBehavior
{
    /// <summary>
    ///     Executes the behavior using the provided test action runner.
    /// </summary>
    /// <param name="runner">The test action runner to use for execution.</param>
    public void Apply(ITestActionRunner runner)
    {
        testBehavior.Invoke(runner);
    }
}