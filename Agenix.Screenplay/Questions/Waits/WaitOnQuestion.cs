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

using Agenix.Validation.NHamcrest.Validation.Matcher;
using NHamcrest;

namespace Agenix.Screenplay.Questions.Waits;

/// <summary>
///     A wait action that waits for a question to match a specified matcher within a timeout period.
/// </summary>
/// <typeparam name="TQ">The type of answer returned by the question.</typeparam>
public class WaitOnQuestion<TQ> : WaitWithTimeout
{
    private readonly IMatcher<TQ> _matcher;
    private readonly IQuestion<TQ> _question;

    /// <summary>
    ///     Initializes a new instance of the WaitOnQuestion class.
    /// </summary>
    /// <param name="question">The question to ask repeatedly until the matcher condition is met.</param>
    /// <param name="matcher">The matcher that defines the expected condition.</param>
    public WaitOnQuestion(IQuestion<TQ> question, IMatcher<TQ> matcher)
    {
        _question = question;
        _matcher = matcher;

        Timeout = TimeSpan.FromMilliseconds(3000);
    }

    /// <summary>
    ///     Performs the wait operation asynchronously for the specified actor.
    ///     Polls the question periodically until it matches the provided matcher or the timeout elapses.
    /// </summary>
    /// <typeparam name="T">The type of actor performing this action.</typeparam>
    /// <param name="actor">The actor performing this wait operation.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default)
    {
        var endTime = DateTime.UtcNow.Add(Timeout);
        var pollInterval = TimeSpan.FromMilliseconds(100);

        TQ lastAnswer = default!;

        while (DateTime.UtcNow < endTime)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancellationToken);
            }

            lastAnswer = await _question.AnsweredBy(actor);
            if (_matcher.Matches(lastAnswer))
            {
                return; // Condition met
            }

            await Task.Delay(pollInterval, cancellationToken);
        }

        // Timeout reached: perform a final assertion to produce an informative failure message
        lastAnswer = await _question.AnsweredBy(actor);
        MatcherAssert.AssertThat(lastAnswer, _matcher);
    }
}
