using Agenix.Api;
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/// <summary>
///     Represents an action that applies a test behavior using a specified runner.
/// </summary>
public class ApplyTestBehaviorAction(ApplyTestBehaviorAction.Builder builder)
    : AbstractTestAction(builder.GetName() ?? "apply-behaviour", builder.GetDescription())
{
    private readonly ITestBehavior _behavior = builder._behavior;
    private readonly ITestActionRunner _runner = builder._runner;

    public override void DoExecute(TestContext context)
    {
        _behavior.Apply(_runner);
    }

    /// <summary>
    ///     A builder class for creating instances of ApplyTestBehaviorAction.
    /// </summary>
    public class Builder : AbstractTestActionBuilder<ApplyTestBehaviorAction, Builder>, ITestActionBuilder<ApplyTestBehaviorAction>
    {
        internal ITestBehavior _behavior;
        internal ITestActionRunner _runner;

        /// <summary>
        ///     Creates a new instance of the Builder.
        /// </summary>
        /// <returns>A new instance of the Builder.</returns>
        public static Builder Apply()
        {
            return new Builder();
        }

        /// <summary>
        ///     Creates a new instance of the Builder with the provided behavior.
        /// </summary>
        /// <param name="behavior">The behavior to be applied to the builder.</param>
        /// <returns>A new instance of the Builder with the behavior set.</returns>
        public static Builder Apply(ITestBehavior behavior)
        {
            var builder = new Builder { _behavior = behavior };
            return builder;
        }

        /// <summary>
        ///     Creates a new instance of the Builder with the specified test behavior.
        /// </summary>
        /// <param name="behavior">The specific test behavior to be applied to the builder.</param>
        /// <returns>A new instance of the Builder with the specified test behavior set.</returns>
        public static Builder Apply(TestBehavior behavior)
        {
            return Apply(new DelegatingTestBehaviour(behavior));
        }

        /// <summary>
        ///     Sets the specified behavior for the builder.
        /// </summary>
        /// <param name="behavior">The behavior to be applied to the builder.</param>
        /// <returns>The builder instance with the behavior set.</returns>
        public Builder Behavior(ITestBehavior behavior)
        {
            _behavior = behavior;
            return this;
        }

        /// <summary>
        ///     Sets the specified behavior for the builder using the TestBehavior delegate.
        /// </summary>
        /// <param name="behavior">The TestBehavior delegate to be applied to the builder.</param>
        /// <returns>The builder instance with the behavior set through a DelegatingTestBehaviour.</returns>
        public Builder Behavior(TestBehavior behavior)
        {
            return Behavior(new DelegatingTestBehaviour(behavior));
        }

        /// <summary>
        ///     Assigns the provided test action runner to the builder.
        /// </summary>
        /// <param name="runner">The test action runner to be assigned.</param>
        /// <returns>The builder instance with the runner assigned.</returns>
        public Builder On(ITestActionRunner runner)
        {
            _runner = runner;
            return this;
        }

        /// <summary>
        ///     Constructs an instance of <see cref="ApplyTestBehaviorAction" /> using the current state of the builder.
        /// </summary>
        /// <returns>An instance of <see cref="ApplyTestBehaviorAction" />.</returns>
        public override ApplyTestBehaviorAction Build()
        {
            return new ApplyTestBehaviorAction(this);
        }
    }
}