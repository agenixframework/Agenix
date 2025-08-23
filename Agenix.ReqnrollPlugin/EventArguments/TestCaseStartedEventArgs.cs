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
///     Provides data for the event raised when a test case starts execution.
/// </summary>
public class TestCaseStartedEventArgs(IAsyncTestCaseRunner testCaseRunner) : EventArgs
{
    /// <summary>
    ///     Provides data for the TestCaseStarted event, which is raised when a test case begins execution.
    /// </summary>
    public TestCaseStartedEventArgs(IAsyncTestCaseRunner testCaseRunner, FeatureContext featureContext,
        ScenarioContext scenarioContext)
        : this(testCaseRunner)
    {
        FeatureContext = featureContext;
        ScenarioContext = scenarioContext;
    }

    /// <summary>
    ///     Gets the test case runner responsible for executing the associated test case.
    ///     This property provides access to the <see cref="IAsyncTestCaseRunner" />, which includes
    ///     methods for managing the test case lifecycle such as starting and stopping the execution.
    /// </summary>
    public IAsyncTestCaseRunner TestCaseRunner { get; } = testCaseRunner;

    /// <summary>
    ///     Gets the feature context associated with the currently executing test case.
    ///     This property provides access to contextual information about the feature being tested,
    ///     allowing for the storage and retrieval of data shared across scenarios within the feature.
    /// </summary>
    public FeatureContext FeatureContext { get; }

    /// <summary>
    ///     Gets the scenario context associated with the current test case execution.
    ///     This property provides access to scenario-specific contextual information,
    ///     such as variables and state management, that is relevant during the test execution lifecycle.
    /// </summary>
    public ScenarioContext ScenarioContext { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the current test case startup process should be canceled.
    ///     This property allows hooks or event handlers to signal that a test case should not proceed with execution,
    ///     providing a mechanism to dynamically interrupt the normal flow based on specific conditions.
    /// </summary>
    public bool Canceled { get; set; }
}
