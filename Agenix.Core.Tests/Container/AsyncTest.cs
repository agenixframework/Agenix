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
using Agenix.Api;
using Agenix.Api.Exceptions;
using Agenix.Core.Container;
using Agenix.Core.Util;
using Moq;
using NUnit.Framework;
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
    // Creating mocks for the async TestAction interface
    private Mock<IAsyncTestAction> _action;
    private Mock<IAsyncTestAction> _error;
    private Mock<IAsyncTestAction> _success;

    // Setup is where we initialize our test setup, similar to the Java example's use of reset
    [SetUp]
    public void SetUp()
    {
        _action = new Mock<IAsyncTestAction>();
        _success = new Mock<IAsyncTestAction>();
        _error = new Mock<IAsyncTestAction>();
    }

    // C#
    [Test]
    public void TestSingleAction_WithManualResetEventSlim()
    {
        using var done = new ManualResetEventSlim(false);
        var timeout = TimeSpan.FromSeconds(5);

        // Ensure the success action signals completion
        _success
            .Setup(s => s.ExecuteAsync(Context, It.IsAny<CancellationToken>()))
            .Callback(() => done.Set())
            .Returns(Task.CompletedTask);

        // Ensure the error action would also signal (so the test doesn't hang on failure)
        _error
            .Setup(e => e.ExecuteAsync(Context, It.IsAny<CancellationToken>()))
            .Callback(() => done.Set())
            .Returns(Task.CompletedTask);

        // Optionally set up the main action; here it just completes immediately
        _action
            .Setup(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Build and start the Async container (fire-and-forget)
        var container = new Async.Builder()
            .Actions(_action.Object)
            .SuccessAction(_success.Object)
            .ErrorAction(_error.Object)
            .Build();

        _ = container.ExecuteAsync(Context);

        // Wait for either success or error to complete (avoid hanging indefinitely)
        if (!done.Wait(timeout))
        {
            Assert.Fail($"Timed out waiting for async container to complete within {timeout}.");
        }

        // Verifications
        _action.Verify(a => a.ExecuteAsync(Context, CancellationToken.None), Times.Once);
        _success.Verify(s => s.ExecuteAsync(Context, CancellationToken.None), Times.Once);
        _error.Verify(e => e.ExecuteAsync(Context, CancellationToken.None), Times.Never);
    }

    [Test]
    public async Task TestMultipleActionsAsync()
    {
        var action1 = new Mock<IAsyncTestAction>();
        var action2 = new Mock<IAsyncTestAction>();
        var action3 = new Mock<IAsyncTestAction>();

        using var resetEvent = new ManualResetEventSlim(false);
        var executedActions = 0;

        // Setup actions to signal completion
        action1.Setup(a => a.ExecuteAsync(It.IsAny<TestContext>(), CancellationToken.None))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref executedActions) == 3)
                {
                    resetEvent.Set();
                }
            })
            .Returns(Task.CompletedTask);

        action2.Setup(a => a.ExecuteAsync(It.IsAny<TestContext>(), CancellationToken.None))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref executedActions) == 3)
                {
                    resetEvent.Set();
                }
            })
            .Returns(Task.CompletedTask);

        action3.Setup(a => a.ExecuteAsync(It.IsAny<TestContext>(), CancellationToken.None))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref executedActions) == 3)
                {
                    resetEvent.Set();
                }
            })
            .Returns(Task.CompletedTask);

        // Build the Async container
        var container = new Async.Builder()
            .SuccessAction(_success.Object)
            .ErrorAction(_error.Object)
            .Actions(action1.Object, action2.Object, action3.Object)
            .Build();

        // Kick off execution and await it
        await container.DoExecute(Context);

        // Wait for all actions to complete with timeout (defensive, though DoExecute awaited)
        Assert.That(resetEvent.Wait(TimeSpan.FromSeconds(5)), Is.True,
            "Async actions did not complete within the expected time");

        // Verify that each action was executed once
        action1.Verify(a => a.ExecuteAsync(Context, CancellationToken.None), Times.Once);
        action2.Verify(a => a.ExecuteAsync(Context, CancellationToken.None), Times.Once);
        action3.Verify(a => a.ExecuteAsync(Context, CancellationToken.None), Times.Once);

        // Verify the success action was executed
        _success.Verify(s => s.ExecuteAsync(Context, CancellationToken.None), Times.Once);

        // Verify that the error action was never executed
        _error.Verify(e => e.ExecuteAsync(Context, CancellationToken.None), Times.Never);
    }

    [Test]
    public async Task TestFailingActionAsync()
    {
        var action1 = new Mock<IAsyncTestAction>();
        var action2 = new Mock<IAsyncTestAction>();
        var action3 = new Mock<IAsyncTestAction>();

        // Use ManualResetEventSlim for better synchronization
        var action1Executed = new ManualResetEventSlim(false);
        var failActionExecuted = new ManualResetEventSlim(false);
        var errorActionExecuted = new ManualResetEventSlim(false);

        // Define a faulty action.
        var failAction = new Mock<IAsyncTestAction>();
        failAction.Setup(f => f.ExecuteAsync(Context, CancellationToken.None))
            .Callback(() => failActionExecuted.Set())
            .ThrowsAsync(new AgenixSystemException("Generated error to interrupt test execution"));

        // Setup execution tracking
        action1.Setup(a => a.ExecuteAsync(Context, CancellationToken.None))
            .Callback(() => action1Executed.Set())
            .Returns(Task.CompletedTask);

        _error.Setup(e => e.ExecuteAsync(Context, CancellationToken.None))
            .Callback(() => errorActionExecuted.Set())
            .Returns(Task.CompletedTask);

        // Build the Async container including the failAction
        var container = new Async.Builder()
            .SuccessAction(_success.Object)
            .ErrorAction(_error.Object)
            .Actions(action1.Object, failAction.Object, action2.Object, action3.Object)
            .Build();

        try
        {
            // Execute and await the container
            await container.DoExecute(Context);

            // Wait for specific execution points with timeouts (defensive)
            Assert.That(action1Executed.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Action1 should execute within timeout");

            Assert.That(failActionExecuted.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "FailAction should execute within timeout");

            Assert.That(errorActionExecuted.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Error action should execute within timeout");

            // Check for exceptions in context
            Assert.That(Context.GetExceptions().Count, Is.EqualTo(1));
            Assert.That(Context.GetExceptions().First(), Is.InstanceOf<AgenixSystemException>());
            Assert.That(Context.GetExceptions().First().Message,
                Is.EqualTo("Generated error to interrupt test execution"));

            // Verify execution order and behavior
            action1.Verify(a => a.ExecuteAsync(Context, CancellationToken.None), Times.Once);
            action2.Verify(a => a.ExecuteAsync(Context, CancellationToken.None), Times.Never);
            action3.Verify(a => a.ExecuteAsync(Context, CancellationToken.None), Times.Never);

            _error.Verify(e => e.ExecuteAsync(Context, CancellationToken.None), Times.Once);
            _success.Verify(s => s.ExecuteAsync(Context, CancellationToken.None), Times.Never);
        }
        finally
        {
            // Clean up synchronization primitives
            action1Executed.Dispose();
            failActionExecuted.Dispose();
            errorActionExecuted.Dispose();
        }
    }

    [Test]
    public async Task TestWaitForFinishTimeout()
    {
        var actionStarted = new ManualResetEventSlim(false);
        var actionCanComplete = new ManualResetEventSlim(false);

        // Setup the action with controlled timing
        _action.Setup(a => a.ExecuteAsync(Context, CancellationToken.None))
            .Callback(() =>
            {
                actionStarted.Set(); // Signal that action has started
                actionCanComplete.Wait(); // Wait for permission to complete
            })
            .Returns(Task.CompletedTask);

        var container = new Async.Builder()
            .Actions(_action.Object)
            .Build();

        // Start execution but don't await yet to be able to assert timeout behavior
        var executionTask = container.DoExecute(Context);

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

        // Ensure the container finishes
        await executionTask;
    }

    [Test]
    public async Task TestWaitForFinishErrorAsync()
    {
        // Set up the action to throw an exception
        _action.Setup(a => a.ExecuteAsync(Context, CancellationToken.None))
            .ThrowsAsync(new AgenixSystemException("FAILED!"));

        var container = new Async.Builder()
            .Actions(_action.Object)
            .Build();

        try
        {
            await container.DoExecute(Context);
        }
        catch (AgenixSystemException)
        {
            // The container may surface startup errors immediately.
        }

        await WaitUtils.WaitForCompletion(container, Context, 2000);

        // Assert that the appropriate exception was tracked within the context
        Assert.That(Context.GetExceptions().Count, Is.EqualTo(1));
        Assert.That(Context.GetExceptions().First(), Is.InstanceOf<AgenixSystemException>());
        Assert.That(Context.GetExceptions().First().Message, Is.EqualTo("FAILED!"));
    }
}
