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
using Agenix.Api;
using Reqnroll;

namespace Agenix.ReqnrollPlugin.EventArguments;

/// <summary>
///     Provides data for an event that is triggered when a test case has finished execution.
/// </summary>
public class TestCaseFinishedEventArgs(IAsyncTestCaseRunner testCaseRunner) : EventArgs
{
    /// <summary>
    ///     Represents event arguments for the completion of a test case execution.
    /// </summary>
    public TestCaseFinishedEventArgs(IAsyncTestCaseRunner testCaseRunner, FeatureContext featureContext,
        ScenarioContext scenarioContext)
        : this(testCaseRunner)
    {
        FeatureContext = featureContext;
        ScenarioContext = scenarioContext;
    }

    /// <summary>
    ///     Represents an instance of the asynchronous test case runner that is responsible for executing test cases.
    /// </summary>
    /// <remarks>
    ///     The TestCaseRunner instance is used to manage and execute test cases asynchronously within a testing framework.
    ///     It provides the core functionality for starting, stopping, and managing the lifecycle and behavior of test cases.
    /// </remarks>
    public IAsyncTestCaseRunner TestCaseRunner { get; } = testCaseRunner;

    /// <summary>
    ///     Represents the contextual data associated with a specific test feature.
    /// </summary>
    /// <remarks>
    ///     The FeatureContext property provides information about the current feature being executed,
    ///     including metadata, state, and shared data relevant to the feature's test lifecycle.
    ///     It can be utilized to manage feature-specific dependencies or configurations.
    /// </remarks>
    public FeatureContext FeatureContext { get; }

    /// <summary>
    ///     Represents contextual information about the current scenario being executed within a test case.
    /// </summary>
    /// <remarks>
    ///     ScenarioContext provides access to scenario-specific data and state during the execution of a test.
    ///     It is commonly used for sharing data or configuration between steps of a scenario.
    /// </remarks>
    public ScenarioContext ScenarioContext { get; }

    /// <summary>
    ///     Indicates whether the process of handling a finished test case is canceled.
    /// </summary>
    /// <remarks>
    ///     The Canceled property is used to determine if subsequent actions related to a finished test case
    ///     should proceed or be aborted. For example, this property may be utilized to stop the test case runner
    ///     or prevent finalization processes during event handling if the value is set to true.
    /// </remarks>
    public bool Canceled { get; set; }
}
