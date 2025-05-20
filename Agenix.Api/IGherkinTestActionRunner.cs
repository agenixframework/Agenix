using System;
using Agenix.Api;

namespace Agenix.Core;

/// Interface that extends ITestActionRunner to support behavior-driven development (BDD) style methods.
/// Methods include Given, When, Then, and And as aliases to represent different stages of the Gherkin syntax in BDD.
/// /
public interface IGherkinTestActionRunner : ITestActionRunner
{
    /// Behavior driven style alias for run method.
    /// @param action The test action.
    /// @typeparam T The type of the test action.
    /// @return The executed test action.
    /// /
    T Given<T>(T action) where T : ITestAction
    {
        return Given(() => action);
    }

    /// <summary>
    ///     Behavior driven style alias for run method.
    /// </summary>
    /// <typeparam name="T">The type of the test action.</typeparam>
    /// <param name="action">The test action.</param>
    /// <returns>The executed test action.</returns>
    T Given<T>(Func<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    T Given<T>(ITestActionBuilder<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    /// Behavior driven style alias for run method.
    /// @param action The test action.
    /// @return The executed test action.
    /// /
    T When<T>(T action) where T : ITestAction
    {
        return When(() => action);
    }

    T When<T>(ITestActionBuilder<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    /// Behavior driven style alias for run method.
    /// @param builder The function that builds the test action.
    /// @return The executed test action.
    /// /
    T When<T>(Func<T> builder) where T : ITestAction
    {
        return Run(builder);
    }


    /// Behavior driven style alias for run method.
    /// @param action The test action.
    /// @return The executed test action.
    /// /
    T Then<T>(T action) where T : ITestAction
    {
        return Then(() => action);
    }

    /// Behavior driven style alias for run method.
    /// @param builder The test action builder function.
    /// @return The executed test action.
    /// /
    T Then<T>(Func<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    T Then<T>(ITestActionBuilder<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    /// Behavior driven style alias for run method.
    /// @param action The test action to be executed.
    /// @return The executed test action.
    /// /
    T And<T>(T action) where T : ITestAction
    {
        return And(() => action);
    }

    /// Behavior driven style alias for run method.
    /// @param builder Function that builds the test action.
    /// @return The executed test action.
    /// /
    T And<T>(Func<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    T And<T>(ITestActionBuilder<T> builder) where T : ITestAction
    {
        return Run(builder);
    }
}