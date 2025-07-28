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

using Agenix.Api.Exceptions;
using Agenix.Screenplay.Consequence;

namespace Agenix.Screenplay;

/// <summary>
///     Tracks and manages a collection of errors encountered during the evaluation
///     of consequences in the Screenplay pattern.
/// </summary>
/// <typeparam name="T">
///     The type parameter tied to the consequences being tracked.
/// </typeparam>
/// <remarks>
///     The <see cref="ErrorTally{T}" /> class collects failed consequences, allowing their
///     aggregation, reporting, and inspection to facilitate error handling and debugging within
///     a screenplay test scenario.
/// </remarks>
internal class ErrorTally<T>
{
    private readonly List<FailedConsequence<T>> _errors = [];
    private Type? _complaintType;

    public void RecordError(IConsequence<T> consequence, Exception cause)
    {
        _errors.Add(new FailedConsequence<T>(consequence, cause));

        // Get the complaint type from the consequence if it has one
        if (consequence is BaseConsequence<T> baseConsequence && _complaintType == null)
        {
            _complaintType = baseConsequence.ComplaintType;
        }
    }

    /// <summary>
    ///     Throws a summary exception if any errors have been recorded.
    /// </summary>
    /// <remarks>
    ///     If the collection of errors is empty, this method performs no action.
    ///     Otherwise, it consolidates the recorded errors into a single exception
    ///     and throws an instance of the configured complaint type or <see cref="AgenixSystemException" /> by default.
    /// </remarks>
    public void ReportAnyErrors()
    {
        if (_errors.Count == 0)
        {
            return;
        }

        ThrowSummaryExceptionFrom(ErrorCausesIn(_errors));
    }

    private void ThrowSummaryExceptionFrom(List<Exception> errorCauses)
    {
        var overallErrorMessage = string.Join(Environment.NewLine, ErrorMessagesIn(errorCauses));

        if (_complaintType != null)
        {
            var exception = CreateCustomException(_complaintType, overallErrorMessage, errorCauses.FirstOrDefault());
            if (exception != null)
            {
                throw exception;
            }
        }

        throw new AgenixSystemException(overallErrorMessage);
    }

    private static Exception? CreateCustomException(Type? exceptionType, string message, Exception? cause)
    {
        try
        {
            if (exceptionType == null)
            {
                return null;
            }

            // Try constructor with message and cause first
            if (cause != null)
            {
                var constructorWithCause = exceptionType.GetConstructor([typeof(string), typeof(Exception)]);
                if (constructorWithCause != null)
                {
                    var exception = (Exception)constructorWithCause.Invoke([message, cause]);
                    return exception;
                }
            }

            // Try constructor with just message
            var constructorWithMessage = exceptionType.GetConstructor([typeof(string)]);
            if (constructorWithMessage != null)
            {
                var exception = (Exception)constructorWithMessage.Invoke([message]);
                return exception;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== DEBUG: Exception during creation: {ex.Message}");
            Console.WriteLine($"=== DEBUG: Exception type: {ex.GetType().Name}");
            Console.WriteLine($"=== DEBUG: Stack trace: {ex.StackTrace}");
        }

        return null;
    }

    private static List<Exception> ErrorCausesIn(List<FailedConsequence<T>> failedConsequences)
    {
        return failedConsequences
            .Select(fc => fc.Cause)
            .ToList();
    }

    private static List<string> ErrorMessagesIn(List<Exception> errorCauses)
    {
        return errorCauses
            .Select(ex => ex.Message)
            .ToList();
    }

    /// <summary>
    ///     Determines whether any errors have been recorded in the collection.
    /// </summary>
    /// <returns>
    ///     True if one or more errors have been recorded, otherwise false.
    /// </returns>
    public bool HasErrors()
    {
        return _errors.Count != 0;
    }
}
