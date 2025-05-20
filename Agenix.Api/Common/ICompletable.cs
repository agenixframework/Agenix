using Agenix.Api.Context;

namespace Agenix.Api.Common;

/// Represents an interface for actions that can determine their completed state.
/// This is particularly useful in testing scenarios where a test case relies on nested actions to
/// complete their execution before the test case is finalized.
/// Asynchronous actions may implement this interface to signal whether their processing or the
/// processing of forked operations has been completed.
/// /
public interface ICompletable
{
    /// Determines whether the test action has been completed. <param name="context">The context in which the test action is executed.</param> <return>True if the test action is finished; otherwise, false.</return>
    /// /
    bool IsDone(TestContext context);
}