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
///     Represents a consequence that failed during its execution in the context of the Screenplay pattern.
/// </summary>
/// <typeparam name="T">The type of value associated with the consequence that failed.</typeparam>
public class FailedConsequence<T>
{
    private readonly Exception? _errorCause;
    private readonly Exception? _runtimeExceptionCause;

    /// <summary>
    ///     Represents a consequence that failed during its execution in the context of the Screenplay pattern.
    /// </summary>
    /// <typeparam name="T">The type of value associated with the consequence that failed.</typeparam>
    /// <remarks>
    ///     This class encapsulates information regarding the failure of a specific consequence,
    ///     tracking both the underlying cause and the consequence itself.
    /// </remarks>
    public FailedConsequence(IConsequence<T> consequence, Exception? cause)
    {
        Consequence = consequence;
        if (IsErrorException(cause))
        {
            _errorCause = cause;
            _runtimeExceptionCause = null;
        }
        else if (cause != null)
        {
            _errorCause = null;
            _runtimeExceptionCause = cause;
        }
    }

    /// <summary>
    ///     Gets the consequence instance that failed during its evaluation.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IConsequence{T}" /> representing the failed consequence.
    /// </value>
    public IConsequence<T> Consequence { get; }

    /// <summary>
    ///     Gets the exception that caused the failure of the consequence.
    /// </summary>
    /// <value>
    ///     An <see cref="Exception" /> instance representing the cause of the failure.
    ///     This can be either a runtime exception or an error exception, depending on the context.
    /// </value>
    public Exception Cause => _runtimeExceptionCause ?? _errorCause;

    /// <summary>
    ///     Throws the exception that caused the current <c>FailedConsequence</c>.
    /// </summary>
    /// <exception cref="SystemException">
    ///     Thrown if the cause is identified as a system exception.
    /// </exception>
    /// <exception cref="OutOfMemoryException">
    ///     Thrown if the cause is identified as an out-of-memory exception.
    /// </exception>
    /// <exception cref="StackOverflowException">
    ///     Thrown if the cause is identified as a stack overflow exception.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown if the cause is a general runtime exception.
    /// </exception>
    public void ThrowException()
    {
        if (_runtimeExceptionCause != null)
        {
            throw _runtimeExceptionCause;
        }

        throw _errorCause;
    }

    private static bool IsErrorException(Exception? ex)
    {
        return ex is SystemException or OutOfMemoryException or StackOverflowException;
    }
}
