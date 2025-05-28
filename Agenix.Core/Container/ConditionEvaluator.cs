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
