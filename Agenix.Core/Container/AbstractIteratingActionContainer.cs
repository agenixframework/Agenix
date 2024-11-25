using System;
using System.Text.RegularExpressions;
using Agenix.Core.Util;
using Agenix.Core.Validation.Matcher;

namespace Agenix.Core.Container;

/// AbstractIteratingActionContainer is an abstract class that extends AbstractActionContainer.
/// It serves as a base for containers that need to execute a specific action iteratively
/// with conditional checks.
/// /
public abstract class AbstractIteratingActionContainer : AbstractActionContainer
{
    /**
     * Boolean expression string
     */
    protected readonly string condition;

    /**
     * Optional condition expression evaluates to true or false
     */
    protected readonly IterationEvaluator.IteratingConditionExpression conditionExpression;

    /**
     * Name of index variable
     */
    protected readonly string indexName;

    /**
     * Cache start index for further container executions - e.g. in loop
     */
    protected readonly int start;

    /**
     * Looping index
     */
    protected int index;

    public AbstractIteratingActionContainer(string name,
        AbstractIteratingContainerBuilder<ITestActionContainer, dynamic>
            builder) : base(name, builder)
    {
        condition = builder.GetCondition();
        conditionExpression = builder.GetConditionExpression();
        indexName = builder.GetIndexName();
        index = builder.GetIndex();
        start = builder.GetStart();
    }

    /// Executes actions in a loop starting from a specified index.
    /// <param name="context">TestContext holding variable information.</param>
    public override void DoExecute(TestContext context)
    {
        index = start;
        ExecuteIteration(context);
    }

    /// Execute embedded actions in loop.
    /// @param context TestContext holding variable information.
    /// /
    protected abstract void ExecuteIteration(TestContext context);

    /// Executes the nested test actions.
    /// @param context The test context holding variable information for execution.
    /// /
    protected void ExecuteActions(TestContext context)
    {
        context.SetVariable(indexName, index.ToString());

        foreach (var actionBuilder in actions) ExecuteAction(actionBuilder.Build(), context);
    }

    /// Check aborting condition.
    /// @param context TestContext holding variable information.
    /// @return true if the condition is met, otherwise false.
    /// /
    protected bool CheckCondition(TestContext context)
    {
        if (conditionExpression != null) return conditionExpression.Invoke(index, context);

        // replace dynamic content with each iteration
        var conditionString = condition;
        var temp = TestContextFactory.CopyOf(context);
        temp.SetVariable(indexName, index.ToString());
        conditionString = temp.ReplaceDynamicContentInString(conditionString);

        if (ValidationMatcherUtils.IsValidationMatcherExpression(conditionString))
            try
            {
                ValidationMatcherUtils.ResolveValidationMatcher("iteratingCondition", index.ToString(), conditionString,
                    context);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        if (conditionString.Contains(indexName))
            conditionString = Regex.Replace(conditionString, indexName, index.ToString());

        return BooleanExpressionParser.Evaluate(conditionString);
    }

    /// Determines if the iteration is considered complete based on the test context.
    /// @param context The TestContext instance holding test variable information.
    /// @return A boolean indicating whether the iteration is done.
    /// /
    public override bool IsDone(TestContext context)
    {
        return base.IsDone(context) || !CheckCondition(context);
    }

    /// Retrieves the condition expression used for iteration evaluation.
    /// @return the condition expression of type IterationEvaluator.IteratingConditionExpression
    /// /
    public IterationEvaluator.IteratingConditionExpression GetConditionExpression()
    {
        return conditionExpression;
    }

    /// Gets the name of the index variable used in the iteration.
    /// @return the name of the index variable
    /// /
    public string GetIndexName()
    {
        return indexName;
    }

    /// Gets the index.
    /// @return the index
    /// /
    public int GetIndex()
    {
        return index;
    }

    /// Gets the start index.
    /// @return The start index used for container executions.
    /// /
    public int GetStart()
    {
        return start;
    }
}