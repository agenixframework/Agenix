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

using System.Text;
using Agenix.Api.Report;

namespace Agenix.Api.Exceptions;

/// <summary>
///     Basic custom runtime/ system exception for all errors in Agenix
/// </summary>
[Serializable]
public class AgenixSystemException : SystemException
{
    private List<FailureStackElement> _failureStack = [];

    /// <summary>
    ///     Default constructor.
    /// </summary>
    public AgenixSystemException()
    {
    }

    public AgenixSystemException(string message) : base(message)
    {
    }

    public AgenixSystemException(string message, Exception cause) : base(message, cause)
    {
    }

    public override string Message => base.Message + GetFailureStackAsString();

    public string GetMessage()
    {
        return Message;
    }

    /// <summary>
    ///     Get formatted string representation of failure stack information.
    /// </summary>
    /// <returns></returns>
    public string GetFailureStackAsString()
    {
        var builder = new StringBuilder();

        foreach (var failureStackElement in GetFailureStack())
        {
            builder.Append("\n\t");
            builder.Append(failureStackElement.GetStackMessage());
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Sets the custom failure stack holding line number information inside test case.
    /// </summary>
    /// <param name="failureStack"></param>
    public void SetFailureStack(List<FailureStackElement> failureStack)
    {
        _failureStack = failureStack;
    }

    /// <summary>
    ///     Gets the custom failure stack with line number information where the testcase failed.
    /// </summary>
    /// <returns>the failureStack</returns>
    public Stack<FailureStackElement> GetFailureStack()
    {
        var stack = new Stack<FailureStackElement>();

        foreach (var failureStackElement in _failureStack)
        {
            stack.Push(failureStackElement);
        }

        return stack;
    }
}
