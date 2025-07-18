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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api.Exceptions;
using Agenix.Core.Container;
using Agenix.Core.Util;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ITestAction = Agenix.Api.ITestAction;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Container;

/// <summary>
///     Represents a suite of asynchronous unit tests for verifying the behavior of an
///     asynchronous action container. The tests ensure correct execution paths for single
///     and multiple actions, handle action success, and verify behavior in failure or timeout scenarios.
/// </summary>
/// <remarks>
///     This test class uses NUnit as the test framework and mocks for verifying interactions
///     with actions. It extends from <see cref="AbstractNUnitSetUp" />, preparing mocks in the
///     <see cref="SetUp" /> method before each test execution.
/// </remarks>
[Platform(Exclude = "Linux", Reason = "Only runs on non-Linux platforms.")]
public class AsyncTest : AbstractNUnitSetUp
{
    // Creating mocks for the TestAction class
    private Mock<ITestAction> _action;
    private Mock<ITestAction> _error;
    private Mock<ITestAction> _success;

    // Setup is where we initialize our test setup, similar to the Java example's use of reset
    [SetUp]
    public void SetUp()
    {
        _action = new Mock<ITestAction>();
        _success = new Mock<ITestAction>();
        _error = new Mock<ITestAction>();
    }

    [Test]
    public async Task TestSingleActionAsync()
    {
        // Building the Async container
        var container = new Async.Builder()
            .Actions(_action.Object)
            .SuccessAction(_success.Object)
            .ErrorAction(_error.Object)
            .Build();

        // Execute the container
        container.Execute(Context);

        // Wait for the asynchronous operation to complete (Replace with appropriate wait logic if needed)
        await WaitUtils.WaitForCompletion(container, Context);

        // Verify that the action was executed
        _action.Verify(a => a.Execute(Context), Times.Once);

        // Verify that the success action was executed
        _success.Verify(s => s.Execute(Context), Times.Once);

        // Verify that the error action was never executed
        _error.Verify(e => e.Execute(Context), Times.Never);
    }

    [Test]
    public void TestMultipleActionsAsync()
    {
        var action1 = new Mock<ITestAction>();
        var action2 = new Mock<ITestAction>();
        var action3 = new Mock<ITestAction>();

        using var resetEvent = new ManualResetEventSlim(false);
        var executedActions = 0;

        // Setup actions to signal completion
        action1.Setup(a => a.Execute(It.IsAny<TestContext>()))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref executedActions) == 3)
                    resetEvent.Set();
            });

        action2.Setup(a => a.Execute(It.IsAny<TestContext>()))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref executedActions) == 3)
                    resetEvent.Set();
            });

        action3.Setup(a => a.Execute(It.IsAny<TestContext>()))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref executedActions) == 3)
                    resetEvent.Set();
            });

        // Build the Async container
        var container = new Async.Builder()
            .SuccessAction(_success.Object)
            .ErrorAction(_error.Object)
            .Actions(action1.Object, action2.Object, action3.Object)
            .Build();

        // Execute the container
        container.Execute(Context);

        // Wait for all actions to complete with timeout
        Assert.That(resetEvent.Wait(TimeSpan.FromSeconds(5)), Is.True,
            "Async actions did not complete within the expected time");

        // Verify that each action was executed once
        action1.Verify(a => a.Execute(Context), Times.Once);
        action2.Verify(a => a.Execute(Context), Times.Once);
        action3.Verify(a => a.Execute(Context), Times.Once);

        // Verify the success action was executed
        _success.Verify(s => s.Execute(Context), Times.Once);

        // Verify that the error action was never executed
        _error.Verify(e => e.Execute(Context), Times.Never);
    }

    [Test]
    public async Task TestFailingActionAsync()
    {
        var action1 = new Mock<ITestAction>();
        var action2 = new Mock<ITestAction>();
        var action3 = new Mock<ITestAction>();

        // Define a faulty action.
        var failAction = new Mock<ITestAction>();
        failAction.Setup(f => f.Execute(Context))
            .Throws(new AgenixSystemException("Generated error to interrupt test execution"));

        // Use ManualResetEventSlim for better synchronization
        var action1Executed = new ManualResetEventSlim(false);
        var failActionExecuted = new ManualResetEventSlim(false);
        var errorActionExecuted = new ManualResetEventSlim(false);
        var containerCompleted = new ManualResetEventSlim(false);

        // Setup execution tracking
        action1.Setup(a => a.Execute(Context))
            .Callback(() => action1Executed.Set());

        failAction.Setup(f => f.Execute(Context))
            .Callback(() => failActionExecuted.Set())
            .Throws(new AgenixSystemException("Generated error to interrupt test execution"));

        _error.Setup(e => e.Execute(Context))
            .Callback(() => errorActionExecuted.Set());

        // Build the Async container including the failAction
        var container = new Async.Builder()
            .SuccessAction(_success.Object)
            .ErrorAction(_error.Object)
            .Actions(action1.Object, failAction.Object, action2.Object, action3.Object)
            .Build();

        try
        {
            // Execute the container
            container.Execute(Context);

            // Wait for specific execution points with timeouts
            Assert.That(action1Executed.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Action1 should execute within timeout");

            Assert.That(failActionExecuted.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "FailAction should execute within timeout");

            Assert.That(errorActionExecuted.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Error action should execute within timeout");

            // Wait for the asynchronous operation to complete
            await WaitUtils.WaitForCompletion(container, Context);

            // Check for exceptions in context
            ClassicAssert.AreEqual(1, Context.GetExceptions().Count);
            ClassicAssert.IsInstanceOf<AgenixSystemException>(Context.GetExceptions().First());
            ClassicAssert.AreEqual("Generated error to interrupt test execution",
                Context.GetExceptions().First().Message);

            // Verify execution order and behavior
            action1.Verify(a => a.Execute(Context), Times.Once);
            action2.Verify(a => a.Execute(Context), Times.Never);
            action3.Verify(a => a.Execute(Context), Times.Never);

            _error.Verify(e => e.Execute(Context), Times.Once);
            _success.Verify(s => s.Execute(Context), Times.Never);
        }
        finally
        {
            // Clean up synchronization primitives
            action1Executed.Dispose();
            failActionExecuted.Dispose();
            errorActionExecuted.Dispose();
            containerCompleted.Dispose();
        }
    }

    [Test]
    public void TestWaitForFinishTimeout()
    {
        var actionStarted = new ManualResetEventSlim(false);
        var actionCanComplete = new ManualResetEventSlim(false);

        // Setup the action with controlled timing
        _action.Setup(a => a.Execute(Context))
            .Callback(() =>
            {
                actionStarted.Set(); // Signal that action has started
                actionCanComplete.Wait(); // Wait for permission to complete
            });

        var container = new Async.Builder()
            .Actions(_action.Object)
            .Build();

        // Start the container execution
        var executionTask = Task.Run(() => container.Execute(Context));

        // Wait for action to start
        Assert.That(actionStarted.Wait(TimeSpan.FromSeconds(1)), Is.True,
            "Action should have started");

        // Now test the timeout - the action is guaranteed to be running
        Assert.ThrowsAsync<AgenixSystemException>(async () =>
        {
            await WaitUtils.WaitForCompletion(container, Context, 200);
        });

        // Clean up - allow action to complete
        actionCanComplete.Set();

        // Wait for execution to finish to avoid resource leaks
        Assert.That(executionTask.Wait(TimeSpan.FromSeconds(1)), Is.True);
    }

    [Test]
    public async Task TestWaitForFinishErrorAsync()
    {
        // Set up the action to throw an exception
        _action.Setup(a => a.Execute(Context)).Throws(new AgenixSystemException("FAILED!"));

        var container = new Async.Builder()
            .Actions(_action.Object)
            .Build();

        try
        {
            container.DoExecute(Context);
        }
        catch (AgenixSystemException)
        {
            // Handle if immediate effects needed, such as logging, though typically the system would capture this.
        }

        await WaitUtils.WaitForCompletion(container, Context, 2000);

        // Assert that the appropriate exception was tracked within the context
        ClassicAssert.AreEqual(1, Context.GetExceptions().Count);
        ClassicAssert.IsInstanceOf<AgenixSystemException>(Context.GetExceptions().First());
        ClassicAssert.AreEqual("FAILED!", Context.GetExceptions().First().Message);
    }
}
