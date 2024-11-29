using Agenix.Core.Condition;

namespace Agenix.Core.Container;

/// WaitMessageConditionBuilder is responsible for constructing a message-based wait condition
/// within the Agenix framework. It extends the WaitConditionBuilder to provide additional
/// configuration specific to message-based conditions.
public class WaitMessageConditionBuilder(Wait.Builder<MessageCondition> builder)
    : WaitConditionBuilder<MessageCondition, WaitMessageConditionBuilder>(builder)
{
    /// Sets the name of the message condition to wait for.
    /// @param messageName The name of the message to set in the condition.
    /// @return The instance of WaitMessageConditionBuilder for method chaining.
    /// /
    public WaitMessageConditionBuilder Name(string messageName)
    {
        GetCondition().SetMessageName(messageName);
        return self;
    }
}