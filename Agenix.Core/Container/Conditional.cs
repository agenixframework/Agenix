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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Validation.Matcher;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// Class executes nested test actions if the condition expression evaluates to true.
/// /
public class Conditional : AbstractActionContainer
{
    /// Represents a logger used for logging activities within the Conditional class.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Conditional));

    /// Represents the condition expression string that determines if the
    /// nested test actions should be executed within the Conditional class.
    private readonly string _condition;

    /**
     * Default constructor.
     */
    public Conditional(Builder builder) : base(builder.GetName() ?? "conditional", builder.GetDescription(),
        builder.GetActions())
    {
        _condition = builder.Condition;
        ConditionExpression = builder.Expression;
    }

    /**
     * Optional condition expression evaluates to true or false
     */
    public ConditionEvaluator.ConditionExpression ConditionExpression { get; }

    /// Executes the conditional logic within the given test context. If the condition evaluates to true,
    /// the nested test actions are executed.
    /// <param name="context">The current test context which contains the state and variables for the test execution.</param>
    /// /
    public override void DoExecute(TestContext context)
    {
        if (CheckCondition(context))
        {
            Log.LogDebug($"Condition [{_condition}] evaluates to true, executing nested actions");
            foreach (var actionBuilder in actions)
            {
                ExecuteAction(actionBuilder.Build(), context);
            }
        }
        else
        {
            Log.LogDebug($"Condition [{_condition}] evaluates to false, not executing nested actions");
        }
    }

    /// Evaluates condition expression and returns a boolean representation.
    /// @param context The current test context containing variables and functions.
    /// @return True if the condition expression evaluates to true, otherwise false.
    /// /
    private bool CheckCondition(TestContext context)
    {
        if (ConditionExpression != null)
        {
            return ConditionExpression.Invoke(context);
        }

        // replace dynamic content with each iteration
        var conditionString = context.ReplaceDynamicContentInString(_condition);
        if (ValidationMatcherUtils.IsValidationMatcherExpression(conditionString))
        {
            try
            {
                ValidationMatcherUtils.ResolveValidationMatcher("iteratingCondition", "", conditionString, context);
                return true;
            }
            catch (ValidationException)
            {
                return false;
            }
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
    public class Builder : AbstractTestContainerBuilder<Conditional, Builder>
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
