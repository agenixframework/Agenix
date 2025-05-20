using Agenix.Api;
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/// Builder for creating default test actions.
/// This class provides a concrete implementation for building test actions using a delegate TestAction.
public class DefaultTestActionBuilder(TestAction action)
    : AbstractTestActionBuilder<ITestAction, DefaultTestActionBuilder>
{
    /// Static fluent entry method for creating a DefaultTestActionBuilder.
    /// @param action The ITestAction instance used to create the builder.
    /// @return A new instance of DefaultTestActionBuilder.
    /// /
    public static DefaultTestActionBuilder Action(TestAction action)
    {
        return new DefaultTestActionBuilder(action);
    }

    /// Builds an instance of ConcreteTestAction using the provided delegate.
    /// Sets the name and description for the constructed test action.
    /// @return An instance of AbstractTestAction populated with the assigned Name and Description.
    public override AbstractTestAction Build()
    {
        AbstractTestAction testAction = new ConcreteTestAction(action);

        testAction.SetName(GetName());
        testAction.SetDescription(GetDescription());

        return testAction;
    }

    /// Concrete implementation of AbstractTestAction.
    /// This class is responsible for executing a delegate instance of ITestAction.
    private class ConcreteTestAction(TestAction delegateInstance) : AbstractTestAction
    {
        public override void Execute(TestContext context)
        {
            delegateInstance?.Invoke(context);
        }

        public override void DoExecute(TestContext context)
        {
            Execute(context);
        }
    }
}