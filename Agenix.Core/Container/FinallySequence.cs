using Agenix.Api;
using Agenix.Api.Context;

namespace Agenix.Core.Container;

/// Represents a sequence of actions that will execute a set of operations
/// and ensures that a final operation is performed at the end.
public class FinallySequence : Sequence
{
    /// Represents a sequence of actions that will execute a set of operations
    /// and ensures that a final operation is performed at the end.
    /// /
    public FinallySequence(Builder builder) : base(
        new Sequence.Builder()
            .Description(builder.GetDescription())
            .Name(builder.GetName())
            .Actions(builder.GetActions().ToArray())
    )
    {
    }

    /// Builder class for constructing FinallySequence instances.
    /// /
    public new class Builder : AbstractTestContainerBuilder<FinallySequence, dynamic>, ITestAction
    {
        public void Execute(TestContext context)
        {
            DoBuild().Execute(context);
        }

        /// Fluent API action building entry method used in C# DSL.
        /// @return A new instance of the Builder class for constructing FinallySequence instances.
        /// /
        public static Builder DoFinally()
        {
            return new Builder();
        }

        protected override FinallySequence DoBuild()
        {
            return new FinallySequence(this);
        }
    }
}