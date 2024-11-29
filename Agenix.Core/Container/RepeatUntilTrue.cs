namespace Agenix.Core.Container;

// Typical implementation of repeat iteration loop. Nested test actions are executed until
// aborting condition evaluates to true.
// Index is incremented each iteration and stored as test variable accessible in the nested test actions
// as normal variable. Index starts with 1 by default.
public class RepeatUntilTrue(RepeatUntilTrue.Builder builder) : AbstractIteratingActionContainer(
    builder.GetName() ?? "repeat",
    builder.GetDescription(),
    builder.GetActions(),
    builder.GetCondition(),
    builder.GetConditionExpression(),
    builder.GetIndexName(),
    builder.GetIndex(),
    builder.GetStart())
{
    // Default constructor.

    /// <summary>
    ///     Executes a single iteration of the repeat-until loop.
    /// </summary>
    /// <param name="context">The context in which the test is executed, allowing access to variables and functions.</param>
    protected override void ExecuteIteration(TestContext context)
    {
        do
        {
            ExecuteActions(context);
            index++;
        } while (!CheckCondition(context));
    }

    // Action builder.
    /// <summary>
    ///     Initiates the creation of a repeat-until loop and provides methods to configure it.
    /// </summary>
    public class Builder : AbstractIteratingContainerBuilder<RepeatUntilTrue, Builder>
    {
        // Fluent API action building entry method used in C# DSL.
        /// <summary>
        ///     Initiates the creation of a repeat-until loop.
        /// </summary>
        /// <returns>An instance of the <see cref="RepeatUntilTrue.Builder" /> class.</returns>
        public static Builder Repeat()
        {
            return new Builder();
        }

        // Adds a condition to this iterate container.
        /// <summary>
        ///     Evaluates the specified condition expression within the context of a repeat-until loop.
        /// </summary>
        /// <param name="cnd">The condition expression to evaluate during the iteration.</param>
        /// <returns>An instance of the RepeatUntilTrue.Builder class with the condition set.</returns>
        public Builder Until(string cnd)
        {
            Condition(cnd);
            return this;
        }

        // Adds a condition expression to this iterate container.
        /// <summary>
        ///     Evaluates the specified condition expression within the context of a repeat-until loop.
        /// </summary>
        /// <param name="cnd">The condition expression to evaluate during the iteration.</param>
        /// <returns>An instance of the <see cref="RepeatUntilTrue.Builder" /> class with the condition set.</returns>
        public Builder Until(IterationEvaluator.IteratingConditionExpression cnd)
        {
            Condition(cnd);
            return this;
        }

        /// <summary>
        ///     Constructs a new instance of <see cref="RepeatUntilTrue" />.
        /// </summary>
        /// <returns>A new instance of <see cref="RepeatUntilTrue" />.</returns>
        protected override RepeatUntilTrue DoBuild()
        {
            return new RepeatUntilTrue(this);
        }
    }
}