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
    private readonly Exception _errorCause;
    private readonly Exception _runtimeExceptionCause;

    public FailedConsequence(IConsequence<T> consequence, Exception cause)
    {
        Consequence = consequence;
        if (IsErrorException(cause))
        {
            _errorCause = cause;
            _runtimeExceptionCause = null;
        }
        else if (cause is Exception)
        {
            _errorCause = null;
            _runtimeExceptionCause = cause;
        }
        else
        {
            _errorCause = null;
            _runtimeExceptionCause = cause;
        }
    }

    public IConsequence<T> Consequence { get; }

    public Exception Cause => _runtimeExceptionCause ?? _errorCause;

    public void ThrowException()
    {
        if (_runtimeExceptionCause != null)
        {
            throw _runtimeExceptionCause;
        }

        throw _errorCause;
    }

    private bool IsErrorException(Exception ex)
    {
        // In C#, we'll consider certain exception types as "errors"
        return ex is SystemException ||
               ex is OutOfMemoryException ||
               ex is StackOverflowException;
    }
}
