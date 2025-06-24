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

namespace Agenix.Api.Report;

/// <summary>
///     Failure stack element provides access to the detailed failure stack message and
///     the location in the test case XML where error happened.
/// </summary>
public class FailureStackElement
{
    /// <summary>
    ///     The name of the failed action
    /// </summary>
    private readonly string _actionName;

    /// <summary>
    ///     Line number in XML test case where error happened
    /// </summary>
    private readonly long _lineNumberStart;

    /// <summary>
    ///     Path to XML test file
    /// </summary>
    private readonly string _testFilePath;

    /// <summary>
    ///     Failing action in XML test case ends in this line
    /// </summary>
    private long _lineNumberEnd;

    /// <summary>
    ///     Default constructor using fields.
    /// </summary>
    /// <param name="testFilePath">file path of failed test.</param>
    /// <param name="actionName">the failed action name.</param>
    /// <param name="lineNumberStart">the line number where the error happened.</param>
    public FailureStackElement(string testFilePath, string actionName, long lineNumberStart)
    {
        _testFilePath = testFilePath;
        _actionName = actionName;
        _lineNumberStart = lineNumberStart;
    }

    /// <summary>
    ///     Gets the line number where error happened.
    /// </summary>
    /// <returns>the line number</returns>
    public long LineNumberStart => _lineNumberStart;

    /// <summary>
    ///     Gets the line number where failing action ends.
    /// </summary>
    /// <returns>the toLineNumber</returns>
    public long LineNumberEnd => _lineNumberEnd;

    /// <summary>
    ///     Gets the test file path for the failed test.
    /// </summary>
    /// <returns>the testFilePath</returns>
    public string TestFilePath => _testFilePath;

    /// <summary>
    ///     Constructs the stack trace message.
    /// </summary>
    /// <returns>the stack trace message.</returns>
    public string GetStackMessage()
    {
        if (_lineNumberEnd > 0 && _lineNumberStart != _lineNumberEnd)
        {
            return $"at {_testFilePath}({_actionName}:{_lineNumberStart}-{_lineNumberEnd})";
        }

        return $"at {_testFilePath}({_actionName}:{_lineNumberStart})";
    }

    /// <summary>
    ///     Sets the line number where failing action ends.
    /// </summary>
    /// <param name="toLineNumber">the toLineNumber to set</param>
    public void SetLineNumberEnd(long toLineNumber)
    {
        _lineNumberEnd = toLineNumber;
    }
}
