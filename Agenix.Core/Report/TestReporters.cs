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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Report;

namespace Agenix.Core.Report;

/// <summary>
///     Legacy synchronous reporters orchestrator kept for backward compatibility with existing tests.
///     Prefer using <see cref="AsyncTestReporters" /> for new code.
/// </summary>
public class TestReporters : IAsyncTestListener, IAsyncTestSuiteListener
{
    private readonly List<IAsyncTestReporter> _testReporters = [];
    private TestResults _testResults = new();

    /// <summary>
    ///     Gets or sets a value indicating whether the report should be automatically cleared after processing.
    /// </summary>
    public bool AutoClear { get; set; } = AgenixSettings.ReportAutoClear();

    /// <summary>
    ///     Handles the initiation of a test case.
    ///     This method processes the starting event of a test case, setting up any necessary preconditions or logging
    ///     mechanisms.
    /// </summary>
    /// <param name="test">
    ///     The test case being started, provided as an instance implementing <see cref="IAsyncTestCase" />.
    ///     Cannot be null.
    /// </param>
    /// <returns>A task representing the asynchronous operation of starting the test case.</returns>
    public Task OnTestStart(IAsyncTestCase test)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the completion of a test case.
    ///     This method processes the finalization of a test case by adding its result to the collection of test results if
    ///     available.
    /// </summary>
    /// <param name="test">
    ///     The test case that has finished, provided as an instance implementing <see cref="IAsyncTestCase" />.
    ///     Can be null.
    /// </param>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    public Task OnTestFinish(IAsyncTestCase test)
    {
        if (test.GetTestResult() != null)
        {
            _testResults.AddResult(test.GetTestResult());
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the event when a test case completes successfully.
    ///     This method processes the successful completion of a test case during execution.
    /// </summary>
    /// <param name="test">The test case that succeeded, provided as an instance implementing <see cref="IAsyncTestCase" />.</param>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    public Task OnTestSuccess(IAsyncTestCase test)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the event when a test case fails during execution.
    ///     This method processes the failure of test cases that encounter errors or exceptions during execution.
    /// </summary>
    /// <param name="test">The test case that failed, provided as an instance implementing <see cref="IAsyncTestCase" />.</param>
    /// <param name="cause">The exception representing the reason behind the test failure.</param>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    public Task OnTestFailure(IAsyncTestCase test, Exception cause)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the event when a test is skipped during execution.
    ///     This method is invoked for test cases that are not executed
    ///     due to specific conditions or configurations.
    /// </summary>
    /// <param name="test">The test case that was skipped, represented as an instance of <see cref="IAsyncTestCase" />.</param>
    /// <returns>A completed task to indicate the asynchronous operation is finished.</returns>
    public Task OnTestSkipped(IAsyncTestCase test)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Invoked at the start of test suite execution or reporting process.
    ///     This method optionally clears existing test results based on the AutoClear setting.
    /// </summary>
    /// <returns>A completed <see cref="Task" /> representing the initialization process.</returns>
    public Task OnStart()
    {
        if (AutoClear)
        {
            _testResults = new TestResults();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Invoked when all test operations or suite executions finish with a failure.
    ///     This method ensures the generation of test reports after an unsuccessful conclusion.
    /// </summary>
    /// <param name="cause">The exception representing the cause of the failure.</param>
    /// <returns>A completed <see cref="Task" /> representing the asynchronous operation of the report generation process.</returns>
    public async Task OnFinishFailure(Exception cause)
    {
        await GenerateReports();
    }

    /// <summary>
    ///     Invoked upon the successful completion of all test operations or suite executions.
    ///     This method ensures the generation of test reports after a successful conclusion.
    /// </summary>
    /// <returns>A completed <see cref="Task" /> representing the completion of the report generation process.</returns>
    public async Task OnFinishSuccess()
    {
        await GenerateReports();
    }

    /// <summary>
    ///     Invoked after the completion of all test executions or suite operations.
    ///     Used for any finalization or cleanup activities required post-execution.
    /// </summary>
    /// <returns>A completed <see cref="Task" /> that represents the completion of finalization processes.</returns>
    public Task OnFinish()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the failure scenario when a process or test suite fails to start.
    /// </summary>
    /// <param name="cause">The exception that caused the failure.</param>
    /// <returns>A completed <see cref="Task" /> that represents the failure handling operation.</returns>
    public Task OnStartFailure(Exception cause)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Indicates the successful start of a process or test suite.
    /// </summary>
    /// <returns>A completed <see cref="Task" /> representing the successful start operation.</returns>
    public Task OnStartSuccess()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Registers a test reporter to the collection of reporters.
    ///     The added reporter will participate in the test reporting process.
    /// </summary>
    /// <param name="testReporter">The test reporter to add to the collection.</param>
    public void AddTestReporter(IAsyncTestReporter testReporter)
    {
        _testReporters.Add(testReporter);
    }

    /// <summary>
    ///     Retrieves the list of test reporters currently registered with the orchestrator.
    /// </summary>
    /// <returns>A list of <see cref="IAsyncTestReporter" /> representing the registered test reporters.</returns>
    public List<IAsyncTestReporter> GetTestReporters()
    {
        return [.. _testReporters];
    }

    /// <summary>
    ///     Sets the auto-clear setting for the test reporters, which determines
    ///     whether test results are automatically cleared at the start of a test run.
    /// </summary>
    /// <param name="autoClear">A boolean indicating whether auto-clear should be enabled or disabled.</param>
    public void SetAutoClear(bool autoClear)
    {
        AutoClear = autoClear;
    }

    /// <summary>
    ///     Retrieves the current set of test results stored within the instance.
    /// </summary>
    /// <returns>A <c>TestResults</c> object containing the collection of test results.</returns>
    public TestResults GetTestResults()
    {
        return _testResults;
    }

    /// <summary>
    ///     Generates reports by iterating through the list of test reporters
    ///     and invoking their respective <c>GenerateReport</c> methods with the current test results.
    /// </summary>
    /// <returns>A <c>Task</c> that represents the asynchronous operation of generating reports.</returns>
    public async Task GenerateReports()
    {
        foreach (var reporter in _testReporters)
        {
            await reporter.GenerateReport(_testResults);
        }
    }
}
