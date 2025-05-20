using Agenix.Api.Context;

namespace Agenix.Core.Container;

/// <summary>
///     The <c>ConditionEvaluator</c> class is responsible for evaluating condition expressions against a given context.
/// </summary>
public class ConditionEvaluator(ConditionEvaluator.ConditionExpression condition)
{
    /// <summary>
    ///     Represents a delegate that defines a condition expression for evaluation.
    /// </summary>
    /// <param name="context">The context within which the condition expression is evaluated.</param>
    /// <returns>Returns true if the condition is met, otherwise false.</returns>
    public delegate bool ConditionExpression(TestContext context);

    /// <summary>
    ///     Represents a condition expression that can be evaluated within a specified context.
    /// </summary>
    /// <remarks>
    ///     This delegate is used to encapsulate a method that takes a <see cref="TestContext" /> object as a parameter
    ///     and returns a boolean value indicating whether the condition is met.
    /// </remarks>
    private ConditionExpression Condition { get; } = condition;

    /// <summary>
    ///     Evaluates the given condition expression using the provided context.
    /// </summary>
    /// <param name="context">The context in which the condition is evaluated.</param>
    /// <returns>Returns true if the condition is met, otherwise false.</returns>
    public bool Evaluate(TestContext context)
    {
        return Condition(context);
    }
}