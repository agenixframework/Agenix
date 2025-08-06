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

/// Represents the result of a test execution, encapsulating the outcome and associated metadata.
public class TestResult
{
    /// Defines the possible outcomes of a test execution.
    /// Represents the result status, which can be success, failure, or skipped.
    public enum RESULT
    {
        /// Represents the successful outcome of a test execution.
        /// Indicates that the test was executed without any errors or failures.
        SUCCESS,

        /// Represents a failed outcome of a test execution.
        /// Indicates that the test encountered errors or did not complete as expected.
        FAILURE,

        /// Represents a skipped outcome of a test execution.
        /// Indicates that the test was not executed, possibly due to preconditions not being met or explicit skipping logic.
        SKIP
    }

    /// Represents the outcome of a test execution, including result details, metadata, and execution context.
    public TestResult()
    {
    }

    /// Represents the result of test execution, encapsulating information about the test’s outcome and additional metadata.
    private TestResult(RESULT result, string testName, string className)
    {
        Result = result;
        TestName = testName;
        ClassName = className;
    }

    /// Represents the outcome of a test, indicating whether it was successful, failed, or skipped.
    public RESULT Result { get; }

    /// The name of the test.
    /// /
    public string TestName { get; }

    /// The name of the class where the test case is defined.
    /// /
    public string ClassName { get; }

    /// A collection of key-value pairs representing additional data or metadata
    /// associated with the test result.
    public Dictionary<string, object> Parameters { get; } = new();

    /// Represents the exception that caused a test failure.
    /// /
    public Exception Cause { get; private set; }

    /// Provides a message describing the error associated with a failed test result.
    /// /
    public string ErrorMessage { get; private set; }

    /// Represents the stack trace information associated with a test failure.
    public string FailureStack { get; }

    /// Describes the type of failure that occurred during the test execution.
    /// /
    public string FailureType { get; private set; }

    /// Represents the duration of the test execution.
    /// /
    public TimeSpan Duration { get; private set; }

    /// Creates a new test result representing a successful execution.
    /// <param name="name">The name of the test case.</param>
    /// <param name="className">The class name of the test case.</param>
    /// <return>Returns a new instance of TestResult representing success.</return>
    /// /
    public static TestResult Success(string name, string className)
    {
        return new TestResult(RESULT.SUCCESS, name, className);
    }

    /// Represents a successful test execution result, providing the test name, class name,
    /// and any associated parameters.
    /// <param name="name">The name of the test that was executed successfully.</param>
    /// <param name="className">The name of the class containing the executed test.</param>
    /// <param name="parameters">Additional parameters or metadata related to the test execution.</param>
    /// <return>A new instance of TestResult representing a successful test execution.</return>
    /// /
    public static TestResult Success(string name, string className, Dictionary<string, object> parameters)
    {
        return new TestResult(RESULT.SUCCESS, name, className)
            .WithParameters(parameters);
    }

    /// Creates a new instance of TestResult for a skipped test.
    /// <param name="name">The name of the skipped test.</param>
    /// <param name="className">The class name in which the skipped test resides.</param>
    /// <return>A new TestResult instance indicating a skipped test.</return>
    /// /
    public static TestResult Skipped(string name, string className)
    {
        return new TestResult(RESULT.SKIP, name, className);
    }

    /// <summary>
    ///     Creates a new test result with parameters for a skipped test.
    /// </summary>
    /// <param name="name">The name of the test case.</param>
    /// <param name="className">The name of the class containing the test case.</param>
    /// <param name="parameters">Additional parameters associated with the test case.</param>
    /// <returns>A TestResult instance representing a skipped test with specified parameters.</returns>
    public static TestResult Skipped(string name, string className, Dictionary<string, object> parameters)
    {
        return new TestResult(RESULT.SKIP, name, className)
            .WithParameters(parameters);
    }

    /// Creates a new test result for a failed execution.
    /// <param name="name">The name of the test.</param>
    /// <param name="className">The name of the class containing the test.</param>
    /// <param name="cause">The exception that caused the test to fail.</param>
    /// <return>A new instance of <c>TestResult</c> representing the failed test.</return>
    /// /
    public static TestResult Failed(string name, string className, Exception cause)
    {
        return new TestResult(RESULT.FAILURE, name, className)
            .WithCause(cause)
            .WithErrorMessage(cause?.Message ?? "");
    }

    /// Creates a new instance of a failed test result.
    /// <param name="name">The name of the test.</param>
    /// <param name="className">The name of the class containing the test.</param>
    /// <param name="errorMessage">The error message associated with the failure.</param>
    /// <return>The instance of a failed test result.</return>
    /// /
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

    /// <summary>
    ///     Determines whether the test result represents a successful outcome.
    /// </summary>
    /// <returns>
    ///     True if the test result is marked as successful, otherwise false.
    /// </returns>
    public bool IsSuccess()
    {
        return RESULT.SUCCESS.Equals(Result);
    }

    /// <summary>
    ///     Determines whether the test result indicates failure.
    /// </summary>
    /// <returns>True if the test result is a failure; otherwise, false.</returns>
    public bool IsFailed()
    {
        return RESULT.FAILURE.Equals(Result);
    }

    /// Determines whether the test result is marked as skipped.
    /// <return>
    ///     True if the test result is skipped; otherwise, false.
    /// </return>
    /// /
    public bool IsSkipped()
    {
        return RESULT.SKIP.Equals(Result);
    }

    /// Sets the failure type of the test result and returns the updated instance.
    /// <param name="failureType">Specifies the type or category of the failure.</param>
    /// <return>The updated instance of the test result with the specified failure type.</return>
    /// /
    public TestResult WithFailureType(string failureType)
    {
        FailureType = failureType;
        return this;
    }

    /// <summary>
    ///     Sets the duration of the test result and returns the updated instance.
    /// </summary>
    /// <param name="duration">The duration to assign to the test result.</param>
    /// <returns>The updated <see cref="TestResult" /> instance with the specified duration.</returns>
    public TestResult WithDuration(TimeSpan duration)
    {
        Duration = duration;
        return this;
    }

    /// Sets the given parameters for the current test result.
    /// This method updates the existing parameters or adds new ones to the test result,
    /// allowing additional contextual information to be included.
    /// <param name="parameters">
    ///     A dictionary containing key-value pairs representing the parameters to be added or updated in
    ///     the test result.
    /// </param>
    /// <return>Returns the current instance of the TestResult with the updated parameters.</return>
    /// /
    public TestResult WithParameters(Dictionary<string, object> parameters)
    {
        foreach (var param in parameters)
        {
            Parameters[param.Key] = param.Value;
        }

        return this;
    }

    /// Associates a cause (exception) with the test result.
    /// <param name="cause">The exception representing the cause of the failure.</param>
    /// <return>The current instance of the <see cref="TestResult" /> with the cause set.</return>
    /// /
    public TestResult WithCause(Exception cause)
    {
        Cause = cause;
        return this;
    }

    /// Adds an error message to the test result.
    /// <param name="errorMessage">The error message describing the issue.</param>
    /// <returns>The updated test result instance with the specified error message.</returns>
    /// /
    public TestResult WithErrorMessage(string errorMessage)
    {
        ErrorMessage = errorMessage;
        return this;
    }

    /// <summary>
    ///     Returns a string representation of the test result, including its name, parameters, result, and duration.
    /// </summary>
    /// <returns>A string providing detailed information about the test result instance.</returns>
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
