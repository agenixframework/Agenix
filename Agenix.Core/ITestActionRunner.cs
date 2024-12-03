using System;
using Agenix.Core.Actions;

namespace Agenix.Core;

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

    /// Runs given test action.
    /// @param action The test action to run.
    /// @return The result of the test action.
    /// /
    T Run<T>(ITestActionBuilder<T> builder) where T : ITestAction;

    /// Apply test behavior on this test action runner.
    /// @param behavior
    /// @return TestActionBuilder
    /// /
    ApplyTestBehaviorAction.Builder ApplyBehavior(TestBehavior behavior);

    /// Applies a specified test behavior using the given behavior interface.
    /// <param name="behavior">The test behavior to be applied.</param>
    /// <return>A builder for constructing the ApplyTestBehaviorAction with the specified behavior.</return>
    ApplyTestBehaviorAction.Builder ApplyBehavior(ITestBehavior behavior);
}