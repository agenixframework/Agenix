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

using Agenix.Api;
using Agenix.Api.Report;

namespace Agenix.Screenplay.Fact;

/// <summary>
///     Listens to events during the lifecycle of test execution and connects an <see cref="Actor" /> to a
///     <see cref="IFact" />.
///     Implements the <see cref="ITestListener" /> interface to respond to various test lifecycle stages.
/// </summary>
public class FactLifecycleListener : ITestListener
{
    private readonly Actor _actor;
    private readonly IFact _fact;

    /// <summary>
    ///     Listens to lifecycle events for facts during test execution and acts as a bridge between an Actor and a Fact.
    ///     Implements the <see cref="ITestListener" /> interface to handle test lifecycle events.
    /// </summary>
    public FactLifecycleListener(Actor actor, IFact fact)
    {
        _actor = actor;
        _fact = fact;
    }

    /// <summary>
    ///     Invoked when a test begins execution. Provides a mechanism to perform any initialization or logging
    ///     required before the test runs. Part of the <see cref="ITestListener" /> process.
    /// </summary>
    /// <param name="test">The <see cref="ITestCase" /> instance representing the test that is starting.</param>
    public void OnTestStart(ITestCase test)
    {
        // Not required
    }

    /// <summary>
    ///     Called after a test completes its execution. Performs teardown operations by using the associated fact and actor.
    ///     Implements the <see cref="ITestListener" /> interface to handle the completion of a test lifecycle.
    /// </summary>
    /// <param name="test">The test case that has finished execution.</param>
    public void OnTestFinish(ITestCase test)
    {
        _fact.Teardown(_actor);
    }

    /// <summary>
    ///     Invoked when a test successfully completes its execution.
    ///     Allows for handling or logging of test success events during the test lifecycle.
    /// </summary>
    /// <param name="test">The test case that has successfully completed.</param>
    public void OnTestSuccess(ITestCase test)
    {
        // Not required
    }

    /// <summary>
    ///     Handles the event that occurs when a test case fails during execution.
    ///     This method captures details about the failed test case and the associated exception.
    /// </summary>
    /// <param name="test">The test case that has encountered a failure.</param>
    /// <param name="cause">The exception detailing the cause of the test case failure.</param>
    public void OnTestFailure(ITestCase test, Exception cause)
    {
        // Not Required
    }

    /// <summary>
    ///     Handles the event when a test is skipped in the lifecycle of a test execution.
    ///     This occurs if a test cannot be executed due to certain preconditions not being met or other reasons.
    /// </summary>
    /// <param name="test">The test case that was skipped.</param>
    public void OnTestSkipped(ITestCase test)
    {
        // Not Required
    }
}
