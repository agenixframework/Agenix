using Agenix.Core.Exceptions;
using Agenix.Core.Util;
using Agenix.Core.Validation.Matcher;
using log4net;

namespace Agenix.Core.Container;

/// Class executes nested test actions if condition expression evaluates to true.
/// /
public class Conditional : AbstractActionContainer
{
    /**
     * Logger
     */
    private static readonly ILog _log = LogManager.GetLogger(typeof(Conditional));

    /**
     * Boolean condition expression string
     */
    private readonly string _condition;

    /**
     * Default constructor.
     */
    public Conditional(Builder builder) : base("conditional", builder)
    {
        _condition = builder.Condition;
        ConditionExpression = builder.Expression;
    }

    /**
     * Optional condition expression evaluates to true or false
     */
    private ConditionEvaluator.ConditionExpression ConditionExpression { get; }

    /// Executes the conditional logic within the given test context. If the condition evaluates to true,
    /// the nested test actions are executed.
    /// <param name="context">The current test context which contains the state and variables for the test execution.</param>
    /// /
    public override void DoExecute(TestContext context)
    {
        if (CheckCondition(context))
        {
            _log.Debug($"Condition [{_condition}] evaluates to true, executing nested actions");
            foreach (var actionBuilder in actions) ExecuteAction(actionBuilder.Build(), context);
        }
        else
        {
            _log.Debug($"Condition [{_condition}] evaluates to false, not executing nested actions");
        }
    }

    /// Evaluates condition expression and returns a boolean representation.
    /// @param context The current test context containing variables and functions.
    /// @return True if the condition expression evaluates to true, otherwise false.
    /// /
    private bool CheckCondition(TestContext context)
    {
        if (ConditionExpression != null)
            return ConditionExpression.Invoke(context);

        // replace dynamic content with each iteration
        var conditionString = context.ReplaceDynamicContentInString(_condition);
        if (ValidationMatcherUtils.IsValidationMatcherExpression(conditionString))
            try
            {
                ValidationMatcherUtils.ResolveValidationMatcher("iteratingCondition", "", conditionString, context);
                return true;
            }
            catch (ValidationException)
            {
                return false;
            }

        return BooleanExpressionParser.Evaluate(conditionString);
    }

    /// Determines if the current action container is completed based on the context.
    /// Checks if the base implementation indicates completion or if the condition for this specific action container evaluates to false.
    /// <param name="context">The context containing variables and states used to evaluate the condition.</param>
    /// <return>True if the action container is completed, false otherwise.</return>
    /// /
    public override bool IsDone(TestContext context)
    {
        return base.IsDone(context) || !CheckCondition(context);
    }

    /// Builder class for constructing Conditional objects.
    /// Provides methods to set conditions and build the Conditional object.
    public class Builder : AbstractTestContainerBuilder<ITestActionContainer, dynamic>
    {
        public string Condition { get; private set; }
        public ConditionEvaluator.ConditionExpression Expression { get; private set; }

        /// Represents a container that executes actions based on specified conditions.
        /// /
        public static Builder Conditional()
        {
            return new Builder();
        }

        /// Condition which allows execution if evaluates to true.
        /// <param name="expression">Condition expression as a string.</param>
        /// <return>Returns the Builder object with updated condition.</return>
        /// /
        public Builder When(string expression)
        {
            Condition = expression;
            return this;
        }

        /// Condition which allows execution if evaluates to true.
        /// @param expression ConditionEvaluator.ConditionExpression that defines the condition to be evaluated.
        /// @return A Builder instance with the specified condition set.
        /// /
        public Builder When(ConditionEvaluator.ConditionExpression expression)
        {
            Expression = expression;
            return this;
        }

        /// Builds and returns a new Conditional object using the current state of the builder.
        /// <return> A new Conditional object constructed based on the configuration provided to the builder. </return>
        /// /
        protected override Conditional DoBuild()
        {
            return new Conditional(this);
        }
    }
}