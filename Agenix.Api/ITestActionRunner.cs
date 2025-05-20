using Agenix.Core;

namespace Agenix.Api;

/// Interface for running test actions.
/// /
public interface ITestActionRunner
{
    /// Runs the provided test action.
    /// @param action The test action to be executed.
    /// @return The executed test action.
    /// /
    T Run<T>(T action) where T : ITestAction;

    /// Runs the provided test action.
    /// @param action The test action to be executed.
    /// @return The executed test action.
    T Run<T>(Func<T> action) where T : ITestAction;

    /// Runs a given test action.
    /// @param action The test action to run.
    /// @return The result of the test action.
    /// /
    T Run<T>(ITestActionBuilder<T> builder) where T : ITestAction;

    /// Applies a specified test behavior using the given behavior interface.
    /// <param name="behavior">The test behavior to be applied.</param>
    /// <return>A builder for constructing the ApplyTestBehaviorAction with the specified behavior.</return>
    ITestActionBuilder<ITestAction> ApplyBehavior(ITestBehavior behavior);

    /// Applies a specified test behavior to the test action runner.
    /// <param name="behavior">The test behavior to be applied.</param>
    /// <return>A builder for creating a test action with the applied behavior.</return>
    ITestActionBuilder<ITestAction> ApplyBehavior(TestBehavior behavior);
}