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

using Agenix.Api.Context;

namespace Agenix.Core.Container;

/// The Iterate class executes nested test actions in loops, continuing iteration as long as the looping condition evaluates to true.
/// Each loop increments an index variable, which is accessible inside the nested test actions as a normal test variable. Iteration starts with an index value of 1 and increments with a default step of 1.
/// The class provides a way to construct an instance with a customizable step value using a builder pattern.
/// /
public class Iterate : AbstractIteratingActionContainer
{
    /// The increment step used to advance the loop index in each iteration.
    /// /
    private readonly int _step;

    /// Represents an action container that iterates over a set of actions.
    /// /
    public Iterate(Builder builder) : base(builder.GetName() ?? "iterate", builder.GetDescription(),
        builder.GetActions(), builder.GetCondition(), builder.GetConditionExpression(), builder.GetIndexName(),
        builder.GetIndex(), builder.GetStart())
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
    public class Builder : AbstractIteratingContainerBuilder<Iterate, Builder>
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
