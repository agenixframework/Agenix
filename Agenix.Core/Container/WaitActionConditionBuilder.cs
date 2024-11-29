using Agenix.Core.Condition;

namespace Agenix.Core.Container;

/// Provides a builder for constructing a WaitActionCondition. Inherits from WaitConditionBuilder
/// and specializes the process for handling ActionConditions. This class is used to define
/// the action that will be tested and waited on, similar to other condition-based wait builders in the system.
public class WaitActionConditionBuilder : WaitConditionBuilder<ActionCondition, WaitActionConditionBuilder>
{
    /// Constructs an instance of WaitActionConditionBuilder using the specified builder.
    /// @param builder The builder for constructing the WaitActionConditionBuilder instance.
    /// /
    public WaitActionConditionBuilder(Wait.Builder<ActionCondition> builder) : base(builder)
    {
    }

    /// Sets the test action to execute and wait for.
    /// @param action The test action to be set.
    /// @return The updated WaitActionConditionBuilder with the specified test action set.
    /// /
    public WaitActionConditionBuilder Action(ITestAction action)
    {
        GetCondition().SetAction(action);
        return this;
    }

    /// Sets the test action using the specified action builder and returns the updated WaitActionConditionBuilder.
    /// @param action The test action builder that will provide the test action to be set.
    /// @return The updated WaitActionConditionBuilder with the specified test action set.
    /// /
    public WaitActionConditionBuilder Action(ITestActionBuilder<ITestAction> action)
    {
        GetCondition().SetAction(action.Build());
        return this;
    }
}