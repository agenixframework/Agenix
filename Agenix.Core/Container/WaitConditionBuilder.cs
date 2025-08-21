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

using System;
using Agenix.Api;
using Agenix.Api.Condition;

namespace Agenix.Core.Container;

/// <summary>
///     An abstract builder class responsible for creating instances of a Wait action
///     with configurable conditions and intervals.
/// </summary>
/// <typeparam name="T">The type that implements the ICondition interface.</typeparam>
/// <typeparam name="TS">The specific type of the WaitConditionBuilder.</typeparam>
public abstract class WaitConditionBuilder<T, TS> : IAsyncTestActionBuilder<Wait>
    where T : ICondition where TS : WaitConditionBuilder<T, TS>
{
    /**
     * Parent wait action builder
     */
    private readonly Wait.Builder<T> _builder;

    /**
     * Self-reference
     */
    protected readonly TS Self;

    /**
     * Default constructor using fields.
     * @param builder
     */
    protected WaitConditionBuilder(Wait.Builder<T> builder)
    {
        _builder = builder;
        Self = (TS)this;
    }

    /// <summary>
    ///     Builds and returns a new instance of the <see cref="Wait" /> class
    ///     based on the current state of the builder configuration.
    /// </summary>
    /// <returns>A configured instance of the <see cref="Wait" /> class.</returns>
    public Wait Build()
    {
        return _builder.Build();
    }

    /// Sets the interval in milliseconds between each test of the condition.
    /// @param interval The interval to use in milliseconds.
    /// @return The modified instance of the WaitConditionBuilder.
    /// /
    public TS Interval(long interval)
    {
        return Interval(interval.ToString());
    }

    /// Sets the interval for condition testing.
    /// <param name="interval">The interval value to be used for condition testing.</param>
    /// <return>The modified instance of the WaitConditionBuilder.</return>
    public TS Interval(string interval)
    {
        _builder.Interval(interval);
        return Self;
    }

    /// Sets the interval in milliseconds between each test of the condition.
    /// <param name="milliseconds">The interval in milliseconds to use.</param>
    /// <return>The modified instance of the WaitConditionBuilder.</return>
    public TS Milliseconds(long milliseconds)
    {
        _builder.Milliseconds(milliseconds);
        return Self;
    }

    /// Sets the interval in milliseconds between each test of the condition.
    /// <param name="milliseconds">The interval in milliseconds to use.</param>
    /// <return>The modified instance of the WaitConditionBuilder.</return>
    public TS Milliseconds(string milliseconds)
    {
        _builder.Milliseconds(milliseconds);
        return Self;
    }

    /// Sets the duration in seconds for the wait condition.
    /// @param seconds The time in seconds to wait.
    /// @return The modified instance of the WaitConditionBuilder.
    /// /
    public TS Seconds(double seconds)
    {
        _builder.Seconds(seconds);
        return Self;
    }

    /// Sets the duration for the wait condition.
    /// @param duration The timespan representing the duration to wait.
    /// @return The modified instance of the WaitConditionBuilder.
    /// /
    public TS Time(TimeSpan duration)
    {
        _builder.Time(duration);
        return Self;
    }

    /// Retrieves the condition associated with this WaitConditionBuilder.
    /// @return The condition of type T.
    /// /
    public T GetCondition()
    {
        return _builder.GetCondition;
    }

    /// Gets the builder associated with the wait condition.
    /// @return The current instance of the Wait.Builder.
    /// /
    public Wait.Builder<T> GetBuilder()
    {
        return _builder;
    }
}
