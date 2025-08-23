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

using Agenix.Api.Util;

namespace Agenix.Screenplay.Consequence;

/// <summary>
///     Abstract base class representing a consequence that can be evaluated in the context of the Screenplay pattern.
/// </summary>
/// <typeparam name="T">The type associated with the consequence.</typeparam>
public abstract class BaseConsequence<T> : IConsequence<T>
{
    private readonly List<IPerformable> _setupActions = [];
    protected string _complaintDetails;
    protected Type _complaintType;
    protected Optional<string> Explanation = Optional<string>.Empty;
    protected Optional<string> SubjectText = Optional<string>.Empty;

    /// <summary>
    ///     Gets the type of the complaint associated with this consequence.
    ///     This property specifies the type that represents the complaint
    ///     to be raised if the consequence is not satisfied or an error occurs
    ///     during evaluation or setup.
    /// </summary>
    public Type ComplaintType => _complaintType;

    /// <summary>
    ///     Evaluates the consequence in the context of the specified actor.
    ///     This method is intended to determine whether the consequence is fulfilled
    ///     or to trigger necessary actions based on the evaluation.
    /// </summary>
    /// <param name="actor">The actor in the Screenplay pattern who is performing or evaluating the consequence.</param>
    public abstract Task EvaluateFor(Actor actor);

    /// <summary>
    ///     Specifies an alternative complaint type to use when the consequence is evaluated to an error.
    ///     This allows customization of the type of exception or complaint to be thrown in case of failure.
    /// </summary>
    /// <param name="complaintType">The type of complaint or exception to be used when the consequence fails.</param>
    /// <return>
    ///     An updated consequence of the specified complaint type configured.
    /// </return>
    public virtual IConsequence<T> OrComplainWith(Type complaintType)
    {
        return OrComplainWith(complaintType, null);
    }

    /// <summary>
    ///     Specifies the type of complaint and optional details for the consequence.
    ///     This method provides flexibility to customize the complaint triggered upon evaluation failure.
    /// </summary>
    /// <param name="complaintType">The type of the complaint to be used, typically represented as an exception type.</param>
    /// <param name="complaintDetails">Optional detailed message or additional information associated with the complaint.</param>
    /// <return>
    ///     The current consequence instance configured with the specified complaint type and details.
    /// </return>
    public virtual IConsequence<T> OrComplainWith(Type complaintType, string complaintDetails)
    {
        _complaintType = complaintType;
        _complaintDetails = complaintDetails;
        return this;
    }

    /// <summary>
    ///     Adds a specified performable action to the setup actions for the consequence.
    ///     This action will be performed in the context of the Screenplay pattern when evaluating the consequence.
    /// </summary>
    /// <param name="performable">The performable action to be executed during the setup phase of the consequence.</param>
    /// <return>
    ///     The current instance of the consequence, allowing method chaining.
    /// </return>
    public virtual Task<IConsequence<T>> WhenAttemptingTo(IPerformable performable)
    {
        _setupActions.Add(performable);
        return Task.FromResult<IConsequence<T>>(this);
    }

    /// <summary>
    ///     Specifies a reason or explanation for the consequence.
    ///     This can be used to provide additional context or clarify the purpose of the evaluated consequence.
    /// </summary>
    /// <param name="explanation">A string representing the explanation for the consequence.</param>
    /// <returns>The current instance of the consequence of the specified explanation applied.</returns>
    public virtual IConsequence<T> Because(string explanation)
    {
        Explanation = Optional<string>.Of(explanation);
        return this;
    }

    /// <summary>
    ///     Adds the specified setup actions to be performed prior to the evaluation of the consequence.
    /// </summary>
    /// <param name="setupActions">
    ///     An array of performable actions to be executed as preparatory tasks before evaluating the
    ///     consequence.
    /// </param>
    /// <returns>The current instance of the consequence to allow method chaining.</returns>
    public virtual Task<IConsequence<T>> After(params IPerformable[] setupActions)
    {
        _setupActions.AddRange(setupActions);
        return Task.FromResult<IConsequence<T>>(this);
    }

    /// <summary>
    ///     Processes the given exception and determines the type of error.
    ///     If the exception is an assertion error or a recognized system error, it may return the exception itself.
    ///     Otherwise, it returns null.
    /// </summary>
    /// <param name="actualError">The exception to process and evaluate.</param>
    /// <return>
    ///     The processed exception if it is an assertion error or recognized system error; otherwise, null.
    /// </return>
    protected static Exception ErrorFrom(Exception actualError)
    {
        return (IsErrorException(actualError) ? actualError : null)!;
    }

    /// <summary>
    ///     Evaluates whether the given exception is classified as an error exception.
    ///     Error exceptions include system exceptions, out-of-memory exceptions, and stack overflow exceptions.
    /// </summary>
    /// <param name="ex">The exception to classify as an error exception or not.</param>
    /// <return>
    ///     true if the exception is a system exception, out-of-memory exception, or stack overflow exception; otherwise,
    ///     false.
    /// </return>
    private static bool IsErrorException(Exception ex)
    {
        return ex is SystemException or OutOfMemoryException or StackOverflowException;
    }

    /// <summary>
    ///     Throws a complaint-specific exception if a complaint type has been specified.
    ///     Uses the provided exception as the cause of the complaint.
    /// </summary>
    /// <param name="actualError">The exception to include as the underlying cause of the complaint.</param>
    protected virtual void ThrowComplaintTypeErrorIfSpecified(Exception actualError)
    {
        if (_complaintType != null)
        {
            throw Complaint.From(_complaintType, _complaintDetails, actualError);
        }
    }

    /// <summary>
    ///     Retrieves the concatenated input values from the setup actions that implement the <see cref="IRecordsInputs" />
    ///     interface.
    ///     This method collects values recorded by such actions and returns them as a single comma-separated string.
    /// </summary>
    /// <returns>
    ///     A comma-separated string of input values from the recorded actions, or an empty string if no input values are
    ///     present.
    /// </returns>
    protected string InputValues()
    {
        return string.Join(",",
            _setupActions.Where(action => action is IRecordsInputs)
                .Cast<IRecordsInputs>()
                .Select(action => action.GetInputValues()));
    }

    /// <summary>
    ///     Appends the recorded input values to the provided message if input values exist;
    ///     otherwise, returns the original message unmodified.
    /// </summary>
    /// <param name="message">The base string to which recorded input values may be appended.</param>
    /// <returns>A string with appended recorded input values, or the original message if no input values exist.</returns>
    protected string AddRecordedInputValuesTo(string message)
    {
        return string.IsNullOrEmpty(InputValues()) ? message : $"{message} [{InputValues()}]";
    }

    /// <summary>
    ///     Executes setup actions required for the consequence in the context of the specified actor.
    ///     This method ensures that any preparatory tasks for the consequence are carried out before its evaluation.
    /// </summary>
    /// <param name="actor">The actor in the Screenplay pattern who executes the setup actions.</param>
    protected virtual async Task PerformSetupActionsAs(Actor actor)
    {
        await actor.AttemptsToAsync(Actor.ErrorHandlingMode.IGNORE_EXCEPTIONS, _setupActions.ToArray());
    }
}
