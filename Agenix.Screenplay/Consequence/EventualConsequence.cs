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

using System.Diagnostics;
using NUnit.Framework;

namespace Agenix.Screenplay.Consequence;

/// <summary>
///     Represents a consequence that may take some time to be satisfied and should be retried within a timeout period.
///     This class implements eventual consistency patterns for assertions that may not immediately pass.
/// </summary>
/// <typeparam name="T">The type associated with the consequence.</typeparam>
public class EventualConsequence<T> : BaseConsequence<T>
{
    /// <summary>
    ///     Defines the short interval, in milliseconds, to pause between consecutive retry attempts.
    /// </summary>
    public const int AShortPeriodBetweenTries = 100;

    private readonly IConsequence<T> _consequenceThatMightTakeSomeTime;
    private readonly List<Type> _exceptionsToIgnore = [];
    private readonly List<IPerformable> _setupActions = [];
    private readonly long _timeoutInMilliseconds;

    private AssertionException _caughtAssertionError;
    private Exception _caughtRuntimeException;

    /// <summary>
    ///     Initializes a new instance of the EventualConsequence class with a consequence, timeout, and silence option.
    /// </summary>
    /// <param name="consequenceThatMightTakeSomeTime">The consequence to evaluate repeatedly until it passes or times out.</param>
    /// <param name="timeoutInMilliseconds">The maximum time to wait for the consequence to pass, in milliseconds.</param>
    public EventualConsequence(IConsequence<T> consequenceThatMightTakeSomeTime, long timeoutInMilliseconds = 5 * 1000L)
    {
        _consequenceThatMightTakeSomeTime = consequenceThatMightTakeSomeTime;
        _timeoutInMilliseconds = timeoutInMilliseconds;
    }

    /// <summary>
    ///     Creates a new EventualConsequence that will repeatedly evaluate the given consequence until it passes or times out.
    /// </summary>
    /// <typeparam name="TConsequence">The type associated with the consequence.</typeparam>
    /// <param name="consequenceThatMightTakeSomeTime">The consequence to evaluate eventually.</param>
    /// <returns>A new EventualConsequence instance.</returns>
    public static EventualConsequence<TConsequence> Eventually<TConsequence>(
        IConsequence<TConsequence> consequenceThatMightTakeSomeTime)
    {
        return new EventualConsequence<TConsequence>(consequenceThatMightTakeSomeTime);
    }

    /// <summary>
    ///     Creates a builder for configuring the timeout duration for this eventual consequence.
    /// </summary>
    /// <param name="amount">The timeout duration amount.</param>
    /// <returns>An EventualConsequenceBuilder for further configuration.</returns>
    public EventualConsequenceBuilder<T> WaitingForNoLongerThan(long amount)
    {
        return new EventualConsequenceBuilder<T>(_consequenceThatMightTakeSomeTime, amount);
    }

    /// <summary>
    ///     Evaluates the consequence for the specified actor, retrying until it passes or the timeout is reached.
    /// </summary>
    /// <param name="actor">The actor for whom the consequence is being evaluated.</param>
    public override async Task EvaluateFor(Actor actor)
    {
        // Reset any previous state in case this instance is reused
        _caughtAssertionError = null;
        _caughtRuntimeException = null;

        var stopwatch = Stopwatch.StartNew();

        do
        {
            try
            {
                await PerformSetupActionsAs(actor);
                await _consequenceThatMightTakeSomeTime.EvaluateFor(actor);
                return; // success
            }
            catch (AssertionException assertionError)
            {
                if (!ShouldIgnoreException(assertionError))
                {
                    _caughtAssertionError = assertionError;
                }
                // else: ignore and retry
            }
            catch (OperationCanceledException)
            {
                // Honor cancellation immediately
                throw;
            }
            catch (Exception runtimeException) when (!IsFatal(runtimeException))
            {
                if (!ShouldIgnoreException(runtimeException))
                {
                    _caughtRuntimeException = runtimeException;
                }
                // else: ignore and retry
            }
            // Let fatal exceptions bubble out

            // Time-aware delay
            var remaining = _timeoutInMilliseconds - stopwatch.ElapsedMilliseconds;
            if (remaining <= 0)
            {
                break;
            }

            var delay = (int)Math.Min(AShortPeriodBetweenTries, remaining);
            await Task.Delay(delay);
        } while (stopwatch.ElapsedMilliseconds < _timeoutInMilliseconds);

        ThrowAnyCaughtErrors();
    }

    private static bool IsFatal(Exception ex)
    {
        // Only treat truly unrecoverable errors as fatal; assertion failures should be retried
        return ex is OutOfMemoryException or StackOverflowException;
    }

    /// <summary>
    ///     Determines whether the specified exception should be ignored during evaluation.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception should be ignored; otherwise, false.</returns>
    private bool ShouldIgnoreException(Exception exception)
    {
        // Support inheritance (exact type or derived)
        return _exceptionsToIgnore.Any(t => t.IsInstanceOfType(exception));
    }

    /// <summary>
    ///     Throws any caught errors that occurred during evaluation attempts.
    /// </summary>
    private void ThrowAnyCaughtErrors()
    {
        if (_caughtAssertionError != null)
        {
            ThrowComplaintTypeErrorIfSpecified(_caughtAssertionError);
            throw _caughtAssertionError;
        }

        if (_caughtRuntimeException != null)
        {
            ThrowComplaintTypeErrorIfSpecified(_caughtRuntimeException);
            throw _caughtRuntimeException;
        }
    }

    /// <summary>
    ///     Returns a string representation of this eventual consequence.
    /// </summary>
    /// <returns>The string representation of the underlying consequence.</returns>
    public override string? ToString()
    {
        return _consequenceThatMightTakeSomeTime.ToString();
    }

    /// <summary>
    ///     Configures the consequence to ignore specific exception types during evaluation.
    /// </summary>
    /// <param name="exceptionsToIgnore">The exception types to ignore.</param>
    /// <returns>The current consequence instance.</returns>
    public IConsequence<T> IgnoringExceptions(params Type[] exceptionsToIgnore)
    {
        _exceptionsToIgnore.AddRange(exceptionsToIgnore);
        return this;
    }

    /// <summary>
    ///     Specifies the complaint type and details to use when the consequence fails.
    /// </summary>
    /// <param name="complaintType">The type of complaint to throw on failure.</param>
    /// <param name="complaintDetails">Additional details for the complaint.</param>
    /// <returns>The current consequence instance.</returns>
    public override IConsequence<T> OrComplainWith(Type complaintType, string complaintDetails)
    {
        _complaintType = complaintType;
        _complaintDetails = complaintDetails;
        return this;
    }

    /// <summary>
    ///     Specifies the complaint type to use when the consequence fails.
    /// </summary>
    /// <param name="complaintType">The type of complaint to throw on failure.</param>
    /// <returns>The current consequence instance.</returns>
    public override IConsequence<T> OrComplainWith(Type complaintType)
    {
        _complaintType = complaintType;
        return this;
    }

    /// <summary>
    ///     Creates a new EventualConsequence with an additional setup action.
    /// </summary>
    /// <param name="performable">The performable action to execute before evaluating the consequence.</param>
    /// <returns>A new EventualConsequence instance with the setup action.</returns>
    public override async Task<IConsequence<T>> WhenAttemptingTo(IPerformable performable)
    {
        return new EventualConsequence<T>(await _consequenceThatMightTakeSomeTime.WhenAttemptingTo(performable),
            _timeoutInMilliseconds);
    }

    /// <summary>
    ///     Creates a new EventualConsequence with an explanation.
    /// </summary>
    /// <param name="explanation">The explanation for this consequence.</param>
    /// <returns>A new EventualConsequence instance with the explanation.</returns>
    public override IConsequence<T> Because(string explanation)
    {
        return new EventualConsequence<T>(_consequenceThatMightTakeSomeTime.Because(explanation),
            _timeoutInMilliseconds);
    }

    /// <summary>
    ///     Throws a complaint-specific exception if a complaint type has been specified.
    ///     Uses the provided exception as the cause of the complaint.
    /// </summary>
    /// <param name="actualError">The exception to include as the underlying cause of the complaint.</param>
    private new void ThrowComplaintTypeErrorIfSpecified(Exception actualError)
    {
        if (_complaintType != null)
        {
            throw Complaint.From(_complaintType, _complaintDetails, actualError);
        }
    }

    /// <summary>
    ///     Adds the specified setup actions to be performed prior to the evaluation of the consequence.
    /// </summary>
    /// <param name="setupActions">
    ///     An array of performable actions to be executed as preparatory tasks before evaluating the
    ///     consequence.
    /// </param>
    /// <returns>A new EventualConsequence instance with the additional setup actions.</returns>
    public override Task<IConsequence<T>> After(params IPerformable[] setupActions)
    {
        _setupActions.AddRange(setupActions);
        return Task.FromResult<IConsequence<T>>(this);
    }

    /// <summary>
    ///     Performs the setup actions for the actor with exception handling.
    /// </summary>
    /// <param name="actor">The actor for whom to perform the setup actions.</param>
    protected override async Task PerformSetupActionsAs(Actor actor)
    {
        await actor.AttemptsToAsync(Actor.ErrorHandlingMode.IGNORE_EXCEPTIONS, _setupActions.ToArray());
    }
}
