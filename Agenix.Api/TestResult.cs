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

namespace Agenix.Api;

/// Represents the result of a test execution including its outcome
/// (success, failure, skipped), associated metadata, and details.
/// /
public class TestResult
{
    /// Defines the possible outcomes of a test execution.
    /// Represents the result status, which can be success, failure, or skipped.
    public enum RESULT
    {
        SUCCESS,
        FAILURE,
        SKIP
    }

    public TestResult()
    {
    }

    /**
     * Constructor with only the required arguments.
     */
    private TestResult(RESULT result, string testName, string className)
    {
        Result = result;
        TestName = testName;
        ClassName = className;
    }

    /**
     * Actual result
     */
    public RESULT Result { get; }

    /**
     * Name of the test
     */
    public string TestName { get; }

    /**
     * Fully qualified class name of the test
     */
    public string ClassName { get; }

    /**
     * Optional test parameters
     */
    public Dictionary<string, object> Parameters { get; } = new();

    /**
     * Failure cause
     */
    public Exception Cause { get; private set; }

    /**
     * Error message
     */
    public string ErrorMessage { get; private set; }

    /**
     * Failure stack trace
     */
    public string FailureStack { get; }

    /**
     * Failure type information
     */
    public string FailureType { get; private set; }

    /**
     * Execution duration
     */
    public TimeSpan Duration { get; private set; }

    /**
     * Create new test result for successful execution.
     */
    public static TestResult Success(string name, string className)
    {
        return new TestResult(RESULT.SUCCESS, name, className);
    }

    /**
     * Create new test result with parameters for successful execution.
     */
    public static TestResult Success(string name, string className, Dictionary<string, object> parameters)
    {
        return new TestResult(RESULT.SUCCESS, name, className)
            .WithParameters(parameters);
    }

    /**
     * Create new test result for skipped test.
     */
    public static TestResult Skipped(string name, string className)
    {
        return new TestResult(RESULT.SKIP, name, className);
    }

    /**
     * Create new test result with parameters for skipped test.
     */
    public static TestResult Skipped(string name, string className, Dictionary<string, object> parameters)
    {
        return new TestResult(RESULT.SKIP, name, className)
            .WithParameters(parameters);
    }

    /**
     * Create new test result for failed execution.
     */
    public static TestResult Failed(string name, string className, Exception cause)
    {
        return new TestResult(RESULT.FAILURE, name, className)
            .WithCause(cause)
            .WithErrorMessage(cause?.Message ?? "");
    }

    /**
     * Create new test result for failed execution.
     */
    public static TestResult Failed(string name, string className, string errorMessage)
    {
        return new TestResult(RESULT.FAILURE, name, className)
            .WithErrorMessage(errorMessage);
    }

    /**
     * Create new test result with parameters for failed execution.
     */
    public static TestResult Failed(string name, string className, Exception cause,
        Dictionary<string, object> parameters)
    {
        return new TestResult(RESULT.FAILURE, name, className)
            .WithParameters(parameters)
            .WithCause(cause)
            .WithErrorMessage(cause?.Message ?? "");
    }

    public bool IsSuccess()
    {
        return RESULT.SUCCESS.Equals(Result);
    }

    public bool IsFailed()
    {
        return RESULT.FAILURE.Equals(Result);
    }

    public bool IsSkipped()
    {
        return RESULT.SKIP.Equals(Result);
    }

    public TestResult WithFailureType(string failureType)
    {
        FailureType = failureType;
        return this;
    }

    public TestResult WithDuration(TimeSpan duration)
    {
        Duration = duration;
        return this;
    }

    public TestResult WithParameters(Dictionary<string, object> parameters)
    {
        foreach (var param in parameters)
        {
            Parameters[param.Key] = param.Value;
        }

        return this;
    }

    public TestResult WithCause(Exception cause)
    {
        Cause = cause;
        return this;
    }

    public TestResult WithErrorMessage(string errorMessage)
    {
        ErrorMessage = errorMessage;
        return this;
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder()
            .Append(GetType().Name)
            .Append('[')
            .Append("testName=").Append(TestName);

        if (Parameters.Count > 0)
        {
            stringBuilder.Append(", parameters=[")
                .Append(string.Join(", ", Parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")))
                .Append(']');
        }

        stringBuilder.Append(", result=").Append(Result);

        stringBuilder.Append(", durationMs=").Append(Duration.TotalMilliseconds);

        return stringBuilder.Append(']')
            .ToString();
    }
}
