#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

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
