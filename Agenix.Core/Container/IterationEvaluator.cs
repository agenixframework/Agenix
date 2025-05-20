using Agenix.Api.Context;

namespace Agenix.Core.Container;

/// <summary>
///     Represents an evaluator that determines the outcome of iterations based on specified conditions.
/// </summary>
/// <remarks>
///     This class uses a delegate to encapsulate the logic for determining whether a particular iteration meets the
///     specified condition.
/// </remarks>
public class IterationEvaluator(IterationEvaluator.IteratingConditionExpression condition)
{
    /// <summary>
    ///     Delegate that defines the condition used for evaluating each iteration.
    ///     Accepts an index and a <see cref="TestContext" /> and returns a boolean indicating whether the condition is met.
    /// </summary>
    /// <param name="index">The current iteration index.</param>
    /// <param name="context">The context in which the evaluation is performed.</param>
    /// <returns>True if the condition is met, otherwise false.</returns>
    public delegate bool IteratingConditionExpression(int index, TestContext context);

    /// <summary>
    ///     Gets or sets the condition used to evaluate each iteration.
    ///     This condition is a delegate that accepts an index and a <see cref="TestContext" />
    ///     and returns a boolean indicating whether the condition is met.
    /// </summary>
    private IteratingConditionExpression Condition { get; } = condition;

    /// <summary>
    ///     Evaluates a condition based on the provided index and context.
    /// </summary>
    /// <param name="index">The index to use in the condition evaluation.</param>
    /// <param name="context">The context in which the condition is evaluated.</param>
    /// <returns>True if the condition is met, otherwise false.</returns>
    public bool EvaluateCondition(int index, TestContext context)
    {
        return Condition(index, context);
    }
}