namespace Agenix.Core.Condition;

/// Tests whether a condition is satisfied.
public interface ICondition
{
    /// Gets the condition name.
    /// @return The name of the condition.
    /// /
    string GetName();

    /// Tests the condition and returns true if it is satisfied.
    /// <param name="context">The test context in which the condition is evaluated.</param>
    /// <return>True if the condition is satisfied; otherwise, false.</return>
    bool IsSatisfied(TestContext context);

    /// Constructs a success message for the current condition.
    /// <param name="context">The test context used to evaluate the condition.</param>
    /// <return>A success message indicating that the condition has been satisfied.</return>
    string GetSuccessMessage(TestContext context);

    /// Constructs an error message for the condition.
    /// <param name="context">The test context related to the condition.</param>
    /// <return>The error message specific to this condition.</return>
    string GetErrorMessage(TestContext context);
}