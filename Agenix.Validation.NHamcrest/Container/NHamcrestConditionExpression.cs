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
using Agenix.Core.Container;
using NHamcrest;

namespace Agenix.Validation.NHamcrest.Container;

/**
* Condition expression that evaluates a Hamcrest matcher. The matcher is invoked with given
* index of iterating test container.
*/
public class NHamcrestConditionExpression
{
    /// Represents a Hamcrest matcher used for evaluating conditions.
    /// /
    private readonly IMatcher<object> _conditionMatcher;

    /// Represents an optional value to be validated in conjunction with a Hamcrest matcher.
    /// /
    private readonly object _value;

    /// Represents a condition expression that evaluates a Hamcrest matcher, using an optional value for evaluation.
    /// Allows the creation and evaluation of conditions based on Hamcrest matchers.
    public NHamcrestConditionExpression(IMatcher<object> conditionMatcher) : this(conditionMatcher, null)
    {
    }

    public NHamcrestConditionExpression(IMatcher<object> conditionMatcher, object value)
    {
        _conditionMatcher = conditionMatcher;
        _value = value;
    }


    // Original methods that accept IMatcher<object>
    public static NHamcrestConditionExpression AssertThat(IMatcher<object> conditionMatcher)
    {
        return new NHamcrestConditionExpression(conditionMatcher);
    }

    public static NHamcrestConditionExpression AssertThat(object value, IMatcher<object> conditionMatcher)
    {
        return new NHamcrestConditionExpression(conditionMatcher, value);
    }

    // New generic methods that handle any IMatcher<T>
    public static NHamcrestConditionExpression AssertThat<T>(IMatcher<T> conditionMatcher)
    {
        return new NHamcrestConditionExpression(WrapMatcher(conditionMatcher));
    }

    public static NHamcrestConditionExpression AssertThat<T>(T value, IMatcher<T> conditionMatcher)
    {
        return new NHamcrestConditionExpression(WrapMatcher(conditionMatcher), value);
    }


    // Helper method to wrap any IMatcher<T> into an IMatcher<object>
    private static IMatcher<object> WrapMatcher<T>(IMatcher<T> matcher)
    {
        return new MatcherWrapper<T>(matcher);
    }


    public IterationEvaluator.IteratingConditionExpression AsIteratingCondition()
    {
        return Evaluate;
    }

    public ConditionEvaluator.ConditionExpression AsCondition()
    {
        return Evaluate;
    }


    public bool Evaluate(int index, TestContext context)
    {
        return _conditionMatcher.Matches(index);
    }

    public bool Evaluate(TestContext context)
    {
        return _conditionMatcher.Matches(_value is string
            ? context.ReplaceDynamicContentInString(_value.ToString())
            : _value);
    }

    // Private class to wrap an IMatcher<T> as an IMatcher<object>
    /// Wrapper class for an `IMatcher
    /// <T>
    ///     ` implementation, providing a way to treat it as an `IMatcher
    ///     <object>
    ///         `.
    ///         Enables seamless matching when types need to be converted from a specific generic type to `object`.
    ///         /
    private class MatcherWrapper<T>(IMatcher<T> innerMatcher) : IMatcher<object>
    {
        /// <summary>
        ///     Determines if the specified object matches the wrapped Hamcrest matcher.
        /// </summary>
        /// <param name="item">The object to evaluate against the condition matcher.</param>
        /// <returns>True if the object matches the condition; otherwise, false.</returns>
        public bool Matches(object item)
        {
            if (item is T typedItem)
            {
                return innerMatcher.Matches(typedItem);
            }

            return false;
        }

        /// Describes the wrapped matcher to the given description. Useful for providing a textual or detailed
        /// representation of the matcher's behavior or criteria.
        /// @param description A description instance where the details of the matcher will be appended.
        /// /
        public void DescribeTo(IDescription description)
        {
            innerMatcher.DescribeTo(description);
        }

        public void DescribeMismatch(object item, IDescription mismatchDescription)
        {
            if (item is T typedItem)
            {
                innerMatcher.DescribeMismatch(typedItem, mismatchDescription);
            }
            else
            {
                mismatchDescription.AppendText("was ").AppendValue(item);
            }
        }
    }
}
