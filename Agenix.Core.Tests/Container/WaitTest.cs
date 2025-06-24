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
using Agenix.Api.Condition;
using Agenix.Api.Exceptions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Container;

public class WaitTest
{
    private Mock<ICondition> conditionMock;
    private Mock<TestContext> contextMock;
    private long endTime;
    private long startTime;

    [SetUp]
    public void Setup()
    {
        contextMock = new Mock<TestContext>();
        conditionMock = new Mock<ICondition>();
    }

    [Test]
    public void ShouldSatisfyWaitConditionOnFirstAttempt()
    {
        var seconds = "10";
        var interval = "1000";

        // Assuming getWaitAction is a method/function that returns a Wait object
        var testling = GetWaitAction(seconds, interval);

        contextMock.Reset();
        conditionMock.Reset();
        PrepareContextMock("10000", interval);
        conditionMock.Setup(c => c.GetName()).Returns("check");
        conditionMock.Setup(c => c.IsSatisfied(contextMock.Object)).Returns(true);
        conditionMock.Setup(c => c.GetSuccessMessage(contextMock.Object)).Returns("Condition success!");

        StartTimer();
        testling.Execute(contextMock.Object);
        StopTimer();

        AssertConditionExecutedWithinSeconds("1");
    }

    [Test]
    public void ShouldSatisfyWaitConditionOnLastAttempt()
    {
        var seconds = "4";
        var interval = "1000";

        var testling = GetWaitAction(seconds, interval);

        contextMock.Reset();
        conditionMock.Reset();
        PrepareContextMock("4000", interval);

        conditionMock.SetupSequence(c => c.IsSatisfied(contextMock.Object))
            .Returns(false)
            .Returns(true);

        conditionMock.Setup(c => c.GetName()).Returns("check");
        conditionMock.Setup(c => c.GetSuccessMessage(contextMock.Object)).Returns("Condition success!");

        StartTimer();
        testling.Execute(contextMock.Object);
        StopTimer();

        AssertConditionExecutedWithinSeconds(seconds);
    }

    [Test]
    public void ShouldSatisfyWaitConditionWithBiggerIntervalThanTimeout()
    {
        var seconds = "1";
        var interval = "10000";

        var testling = GetWaitAction(seconds, interval);

        contextMock.Reset();
        conditionMock.Reset();
        PrepareContextMock("1000", interval);

        conditionMock.Setup(c => c.GetName()).Returns("check");
        conditionMock.Setup(c => c.IsSatisfied(contextMock.Object)).Returns(true);
        conditionMock.Setup(c => c.GetSuccessMessage(contextMock.Object)).Returns("Condition success!");

        StartTimer();
        testling.Execute(contextMock.Object);
        StopTimer();

        AssertConditionExecutedWithinSeconds(seconds);
    }

    [Test]
    public void ShouldNotSatisfyWaitCondition()
    {
        var seconds = "3";
        var interval = "1000";

        var testling = GetWaitAction(seconds, interval);

        contextMock.Reset();
        conditionMock.Reset();
        PrepareContextMock("3000", interval);

        conditionMock.Setup(c => c.GetName()).Returns("check");
        conditionMock.Setup(c => c.IsSatisfied(contextMock.Object)).Returns(false);
        conditionMock.Setup(c => c.GetErrorMessage(contextMock.Object)).Returns("Condition failed!");

        StartTimer();

        var exception = Assert.Throws<AgenixSystemException>(() => testling.Execute(contextMock.Object));
        ClassicAssert.NotNull(exception, "Expected CoreSystemException to be thrown");

        StopTimer();

        AssertConditionExecutedWithinSeconds(seconds);
    }

    [Test]
    public void ShouldNotSatisfyWaitConditionWithBiggerIntervalThanTimeout()
    {
        var seconds = "1";
        var interval = "10000";

        var testling = GetWaitAction(seconds, interval);

        contextMock.Reset();
        conditionMock.Reset();
        PrepareContextMock("1000", interval);

        conditionMock.Setup(c => c.GetName()).Returns("check");
        conditionMock.Setup(c => c.IsSatisfied(contextMock.Object)).Returns(false);
        conditionMock.Setup(c => c.GetErrorMessage(contextMock.Object)).Returns("Condition failed!");

        StartTimer();

        var exception = Assert.Throws<AgenixSystemException>(() => testling.Execute(contextMock.Object));
        ClassicAssert.NotNull(exception, "Expected CoreSystemException to be thrown");

        StopTimer();

        AssertConditionExecutedWithinSeconds(seconds);
    }

    private void PrepareContextMock(string waitTime, string interval)
    {
        contextMock.Setup(c => c.ReplaceDynamicContentInString(waitTime, It.IsAny<bool>())).Returns(waitTime);
        contextMock.Setup(c => c.ReplaceDynamicContentInString(interval, It.IsAny<bool>())).Returns(interval);
    }

    private Wait GetWaitAction(string waitTimeSeconds, string interval)
    {
        return new Wait.Builder<ICondition>()
            .Condition(conditionMock.Object)
            .Interval(interval)
            .Seconds(long.Parse(waitTimeSeconds))
            .Build();
    }

    private void AssertConditionExecutedWithinSeconds(string seconds)
    {
        const long tolerance = 500L;
        var totalExecutionTime = endTime - startTime;
        var permittedTime = int.Parse(seconds) * 1000L + tolerance;

        ClassicAssert.LessOrEqual(totalExecutionTime, permittedTime,
            $"Expected conditional check to execute in {permittedTime} milliseconds but took {totalExecutionTime} milliseconds");
    }

    private void StartTimer()
    {
        startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    private void StopTimer()
    {
        endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
