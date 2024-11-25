namespace Agenix.Core.Container;

/// The Iterate class executes nested test actions in loops, continuing iteration as long as the looping condition evaluates to true.
/// Each loop increments an index variable, which is accessible inside the nested test actions as a normal test variable. Iteration starts with an index value of 1 and increments with a default step of 1.
/// The class provides a way to construct an instance with a customizable step value using a builder pattern.
/// /
public class Iterate : AbstractIteratingActionContainer
{
    /**
     * Index increment step
     */
    private readonly int _step;

    /**
     * Default constructor.
     */
    public Iterate(Builder builder) : base("iterate", builder)
    {
        _step = builder._step;
    }

    /// Executes nested test actions in loops as long as the looping condition evaluates to true.
    /// Each iteration increments an index variable used within the nested test actions.
    /// @param context The context containing variables and configuration for the test actions.
    /// /
    protected override void ExecuteIteration(TestContext context)
    {
        while (CheckCondition(context))
        {
            ExecuteActions(context);

            index += _step;
        }
    }

    /// Gets the step.
    /// @return the step
    /// /
    public int GetStep()
    {
        return _step;
    }

    /// A builder class for constructing Iterate action instances in a fluent manner.
    /// This class provides methods to configure specific properties of the Iterate action, such as the step size for each iteration.
    /// The typical flow involves configuring the desired settings through method chaining and then building the action.
    /// /
    public class Builder : AbstractIteratingContainerBuilder<ITestActionContainer, dynamic>
    {
        public int _step = 1;

        /// Constructs a new instance of the Iterate action container.
        /// @param builder The builder used to configure this action container.
        /// /
        public static Builder Iterate()
        {
            return new Builder();
        }

        /// Sets the step for each iteration.
        /// @param step The step value to use for iteration.
        /// @return The builder instance for method chaining.
        /// /
        public Builder Step(int step)
        {
            _step = step;
            return this;
        }

        /// Builds and returns an instance of Iterate.
        /// @return An instance of Iterate.
        /// /
        protected override Iterate DoBuild()
        {
            return new Iterate(this);
        }
    }
}