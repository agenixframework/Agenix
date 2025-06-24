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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.Core.Functions.Core;
using Moq;
using NUnit.Framework;
using static Agenix.Core.Actions.DefaultTestActionBuilder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.TestCase;

public class DefaultTestCaseTest : AbstractNUnitSetUp
{
    private DefaultTestCase _fixture;

    [SetUp]
    public void Initialize()
    {
        _fixture = new DefaultTestCase();
    }

    [Test]
    public void FailEmptyTestResult()
    {
        Assert.That(_fixture.GetTestResult(), Is.Null);

        var throwableMock = new Mock<Exception>().Object;
        _fixture.Fail(throwableMock);

        var result = _fixture.GetTestResult();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<TestResult>());
        Assert.That(result.Cause, Is.EqualTo(throwableMock));
        Assert.That(result.Result, Is.EqualTo(TestResult.RESULT.FAILURE));
        Assert.That(result.Duration, Is.Not.Null);
    }

    [Test]
    public void FailOverridesOtherTestResult()
    {
        _fixture.SetTestResult(TestResult.Success("failOverridesOtherTestResult", GetType().Name));

        var throwableMock = new Mock<Exception>().Object;
        _fixture.Fail(throwableMock);

        var result = _fixture.GetTestResult();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<TestResult>());
        Assert.That(result.Cause, Is.EqualTo(throwableMock));
        Assert.That(result.Result, Is.EqualTo(TestResult.RESULT.FAILURE));
        Assert.That(result.Duration, Is.Not.Null);
    }

    [Test]
    public void TestExecution()
    {
        _fixture.SetName("MyTestCase");

        _fixture.AddTestAction(new EchoAction.Builder().Build());

        _fixture.Execute(Context);
        _fixture.Finish(Context);

        VerifyDurationHasBeenMeasured(_fixture.GetTestResult());
    }

    [Test]
    public void TestWaitForFinish()
    {
        _fixture.SetName("MyTestCase");

        var actionStarted = new ManualResetEventSlim(false);
        var actionCompleted = new ManualResetEventSlim(false);
        var startTime = DateTime.UtcNow;

        _fixture.AddTestAction(new ConcreteAsyncTestAction
        {
            DoExecuteAsyncFunc = _ =>
            {
                try
                {
                    actionStarted.Set(); // Signal that the action has started
                    Thread.Sleep(500);
                    actionCompleted.Set(); // Signal that the action has completed
                }
                catch (Exception e)
                {
                    throw new AgenixSystemException(e.Message);
                }

                return Task.CompletedTask;
            }
        });

        var echoAction = new EchoAction.Builder().Build();
        _fixture.AddTestAction(echoAction);

        _fixture.Execute(Context);

        // Wait for the async action to start before calling Finish
        Assert.That(actionStarted.Wait(TimeSpan.FromSeconds(2)), Is.True,
            "Async action should have started");

        _fixture.Finish(Context);

        // Verify that the action actually completed
        Assert.That(actionCompleted.Wait(TimeSpan.FromSeconds(1)), Is.True,
            "Async action should have completed");

        var duration = VerifyDurationHasBeenMeasured(_fixture.GetTestResult());

        // More precise timing assertion
        var actualElapsed = DateTime.UtcNow - startTime;
        Assert.That(duration, Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(500)),
            $"TestResult duration should be at least 500ms, but was {duration.TotalMilliseconds}ms. " +
            $"Actual elapsed time: {actualElapsed.TotalMilliseconds}ms");
    }

    [Test]
    public void TestWaitForFinishTimeout()
    {
        _fixture.SetName("MyTestCase");
        _fixture.Timeout = 500;

        var echoAction = new EchoAction.Builder().Build();

        var actionStarted = new ManualResetEventSlim(false);
        var timeoutTested = new ManualResetEventSlim(false);

        _fixture.AddTestAction(new ConcreteAsyncTestAction
        {
            DoExecuteAsyncFunc = _ =>
            {
                try
                {
                    actionStarted.Set(); // Signal that the async action has started

                    // Wait for the timeout test to complete, or until a reasonable timeout
                    // This ensures the action stays "running" during the timeout test
                    timeoutTested.Wait(TimeSpan.FromSeconds(10));
                }
                catch (Exception e)
                {
                    throw new AgenixSystemException(e.Message);
                }

                return Task.CompletedTask;
            }
        });
        _fixture.AddTestAction(echoAction);

        // Start execution
        _fixture.Execute(Context);

        // Wait for the async action to start before testing timeout
        Assert.That(actionStarted.Wait(TimeSpan.FromSeconds(2)), Is.True,
            "Async action should have started");

        // Now test the timeout behavior - we know the action is running
        var exception = Assert.Throws(typeof(TestCaseFailedException), () =>
        {
            _fixture.Finish(Context);
        });

        // Signal that timeout test is complete so async action can finish
        timeoutTested.Set();

        Debug.Assert(exception != null, nameof(exception) + " != null");
        Assert.That(exception.Message,
            Is.EqualTo("Failed to wait for the test container to finish properly - timeout exceeded"));
    }

    [Test]
    public void TestExecutionWithVariables()
    {
        _fixture.SetName("MyTestCase");
        Dictionary<string, object> variables = new()
        {
            { "name", "Agenix" },
            { "framework", "${name}" },
            { "hello", "agenix:Concat('Hello ', ${name}, '!')" },
            { "goodbye", "Goodbye ${name}!" },
            { "welcome", "Welcome ${name}, today is agenix:CurrentDate()!" }
        };
        _fixture.SetVariableDefinitions(variables);

        _fixture.AddTestAction(Action(context =>
        {
            Assert.That(context.GetVariables()[AgenixSettings.TestNameVariable()], Is.EqualTo("MyTestCase"));
            Assert.That(context.GetVariables()[AgenixSettings.TestNameSpaceVariable()],
                Is.EqualTo(typeof(DefaultTestCase).Namespace));
            Assert.That(context.GetVariable("${name}"), Is.EqualTo("Agenix"));
            Assert.That(context.GetVariable("${framework}"), Is.EqualTo("Agenix"));
            Assert.That(context.GetVariable("${hello}"), Is.EqualTo("Hello Agenix!"));
            Assert.That(context.GetVariable("${goodbye}"), Is.EqualTo("Goodbye Agenix!"));
            Assert.That(context.GetVariable("${welcome}"), Is.EqualTo("Welcome Agenix, today is " +
                                                                      new CurrentDateFunction().Execute([], Context) +
                                                                      "!"));
        }));

        _fixture.Execute(Context);
        _fixture.Finish(Context);
        VerifyDurationHasBeenMeasured(_fixture.GetTestResult());
    }

    [Test]
    public void TestUnknownVariable()
    {
        _fixture.SetName("MyTestCase");
        const string message = "Hello TestFramework!";

        _fixture.SetVariableDefinitions(new Dictionary<string, object> { { "text", message } });

        _fixture.AddTestAction(Action(context => Assert.That(context.GetVariable("${unknown}"), Is.EqualTo(message))));

        Assert.Throws(typeof(TestCaseFailedException), () =>
        {
            _fixture.Execute(Context);
            _fixture.Finish(Context);
            VerifyDurationHasBeenMeasured(_fixture.GetTestResult());
        });
    }

    [Test]
    public void TestExceptionInContext()
    {
        _fixture.SetName("MyTestCase");

        _fixture.AddTestAction(Action(context =>
            context.AddException(new AgenixSystemException("This failed in forked action"))).Build());

        _fixture.AddTestAction(new EchoAction.Builder().Message("Everything is fine!").Build());

        var exception = Assert.Throws(typeof(TestCaseFailedException), () => { _fixture.Execute(Context); });
        _fixture.Finish(Context);

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("This failed in forked action"));
    }

    [Test]
    public void TestExceptionInContextInFinish()
    {
        _fixture.SetName("MyTestCase");

        _fixture.AddTestAction(Action(context =>
            context.AddException(new AgenixSystemException("This failed in forked action"))).Build());

        _fixture.Execute(Context);
        var exception = Assert.Throws(typeof(TestCaseFailedException), () => { _fixture.Finish(Context); });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("This failed in forked action"));
    }

    [Test]
    public void TestFinalActions()
    {
        _fixture.SetName("MyTestCase");
        _fixture.AddTestAction(new EchoAction.Builder().Message("Everything is fine!").Build());
        _fixture.AddFinalAction(new EchoAction.Builder().Build());

        _fixture.Execute(Context);
        _fixture.Finish(Context);

        VerifyDurationHasBeenMeasured(_fixture.GetTestResult());
    }

    /// <summary>
    ///     Verifies that the duration has been measured for the given test result.
    /// </summary>
    /// <param name="fixture">The test result to verify.</param>
    /// <returns>The duration of the test result.</returns>
    private static TimeSpan VerifyDurationHasBeenMeasured(TestResult fixture)
    {
        Assert.That(fixture, Is.Not.Null);
        Assert.That(fixture.Duration, Is.Not.Null);
        return fixture.Duration;
    }

    private class ConcreteAsyncTestAction : AbstractAsyncTestAction
    {
        public Func<TestContext, Task> DoExecuteAsyncFunc { get; init; }

        public override Task DoExecuteAsync(TestContext context)
        {
            return DoExecuteAsyncFunc(context);
        }
    }
}
