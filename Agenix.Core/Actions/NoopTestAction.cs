using Agenix.Api;
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/**
 * Special test action doing nothing but implementing the test action interface. This is useful during C# dsl fluent API
 * that needs to return a test action, but this should not be included or executed during the test run. See test behavior applying
 * test actions.
 */
public class NoopTestAction : ITestAction
{
    public void Execute(TestContext context)
    {
        // do nothing
    }

    public string GetName()
    {
        return "noop";
    }
}