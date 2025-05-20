using Agenix.Api;
using Agenix.Api.Context;

namespace Agenix.Core;

/// <summary>
///     Executes test actions by delegating the execution work to another specified test action.
/// </summary>
/// <param name="testAction">The test action to which the execution work is delegated.</param>
public class DelegatingTestAction(TestAction testAction) : ITestAction
{
    /// <summary>
    ///     Executes the test action using the provided test context.
    /// </summary>
    /// <param name="context">The test context to use for execution.</param>
    public void Execute(TestContext context)
    {
        testAction(context);
    }
}