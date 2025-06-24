#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
