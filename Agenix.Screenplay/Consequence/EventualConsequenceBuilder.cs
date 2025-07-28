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

namespace Agenix.Screenplay.Consequence;

/// <summary>
///     Builder class for configuring the timeout duration of an EventualConsequence.
///     Provides fluent methods to specify timeout values in different time units.
/// </summary>
/// <typeparam name="T">The type associated with the consequence.</typeparam>
public class EventualConsequenceBuilder<T>
{
    private readonly long _amount;
    private readonly IConsequence<T> _consequence;

    /// <summary>
    ///     Initializes a new instance of the EventualConsequenceBuilder class.
    /// </summary>
    /// <param name="consequence">The consequence to be wrapped with eventual behavior.</param>
    /// <param name="amount">The timeout amount in the unit that will be specified by the builder methods.</param>
    public EventualConsequenceBuilder(IConsequence<T> consequence, long amount)
    {
        _consequence = consequence;
        _amount = amount;
    }

    /// <summary>
    ///     Creates an EventualConsequence with the timeout specified in milliseconds.
    /// </summary>
    /// <returns>A new EventualConsequence instance with the timeout in milliseconds.</returns>
    public EventualConsequence<T> Milliseconds()
    {
        return new EventualConsequence<T>(_consequence, _amount);
    }

    /// <summary>
    ///     Creates an EventualConsequence with the timeout specified in seconds.
    /// </summary>
    /// <returns>A new EventualConsequence instance with the timeout converted to milliseconds.</returns>
    public EventualConsequence<T> Seconds()
    {
        return new EventualConsequence<T>(_consequence, _amount * 1000);
    }
}
