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

using Agenix.Api.Container;

namespace Agenix.Core.Container;

public abstract class AbstractIteratingContainerBuilder<T, TS> : AbstractTestContainerBuilder<T, TS>
    where T : ITestActionContainer
    where TS : class
{
    protected string condition;
    protected IterationEvaluator.IteratingConditionExpression conditionExpression;
    protected int index;
    protected string indexName = "i";
    protected int start = 1;

    // Adds a condition to this iterate container.
    /// <summary>
    ///     Adds a condition to this iterating container.
    /// </summary>
    /// <param name="cnd">The condition to be used for iteration evaluation.</param>
    /// <returns>The current instance of the iterating container builder.</returns>
    public AbstractIteratingContainerBuilder<T, TS> Condition(string cnd)
    {
        condition = cnd;
        return this;
    }

    // Adds a condition expression to this iterate container.
    /// <summary>
    ///     Adds a condition expression to this iterating container.
    /// </summary>
    /// <param name="cnd">The condition expression to be used for iteration evaluation.</param>
    /// <returns>The current instance of the iterating container builder.</returns>
    public AbstractIteratingContainerBuilder<T, TS> Condition(IterationEvaluator.IteratingConditionExpression cnd)
    {
        conditionExpression = cnd;
        return this;
    }

    // Sets the index variable name.
    /// <summary>
    ///     Sets the index variable name.
    /// </summary>
    /// <param name="name">The name to be assigned to the index variable.</param>
    /// <returns>The current instance of the iterating container builder.</returns>
    public AbstractIteratingContainerBuilder<T, TS> Index(string name)
    {
        indexName = name;
        return this;
    }

    // Sets the index start value.
    /// <summary>
    ///     Sets the index start value.
    /// </summary>
    /// <param name="idx">The starting index value.</param>
    /// <returns>The current instance of the iterating container builder.</returns>
    public AbstractIteratingContainerBuilder<T, TS> StartsWith(int idx)
    {
        start = idx;
        return this;
    }

    /// <summary>
    ///     Builds the iterate container by setting a default condition expression if none is provided.
    /// </summary>
    /// <returns>
    ///     An instance of the iterating action container.
    /// </returns>
    public override T Build()
    {
        if (condition == null && conditionExpression == null)
        {
            conditionExpression = (idx, context) => idx > 10;
        }

        return base.Build();
    }

    // Gets the condition.
    /// <summary>
    ///     Gets the condition string for this iterate container.
    /// </summary>
    /// <returns>
    ///     A string representing the condition used during iteration.
    /// </returns>
    public string GetCondition()
    {
        return condition;
    }

    // Gets the conditionExpression.
    /// <summary>
    ///     Retrieves the iterating condition expression.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="IterationEvaluator.IteratingConditionExpression" /> representing the condition used
    ///     during iteration.
    /// </returns>
    public IterationEvaluator.IteratingConditionExpression GetConditionExpression()
    {
        return conditionExpression;
    }

    // Gets the indexName.
    /// <summary>
    ///     Gets the index variable name.
    /// </summary>
    /// <returns>
    ///     The index variable name as a string.
    /// </returns>
    public string GetIndexName()
    {
        return indexName;
    }

    // Gets the index.
    /// <summary>
    ///     Gets the index value.
    /// </summary>
    /// <returns>
    ///     The current index as an integer.
    /// </returns>
    public int GetIndex()
    {
        return index;
    }

    // Gets the start index.
    /// <summary>
    ///     Gets the start index.
    /// </summary>
    /// <returns>
    ///     The start index as an integer.
    /// </returns>
    public int GetStart()
    {
        return start;
    }
}
