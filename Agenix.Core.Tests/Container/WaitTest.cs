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
using System.Threading.Tasks;
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
    private Mock<ICondition> _conditionMock;
    private Mock<TestContext> _contextMock;
    private long _endTime;
    private long _startTime;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<TestContext>();
        _conditionMock = new Mock<ICondition>();
    }

    [Test]
    public async Task ShouldSatisfyWaitConditionOnFirstAttempt()
    {
        var seconds = "10";
        var interval = "1000";

        // Assuming getWaitAction is a method/function that returns a Wait object
        var testling = GetWaitAction(seconds, interval);

        _contextMock.Reset();
        _conditionMock.Reset();
        PrepareContextMock("10000", interval);
        _conditionMock.Setup(c => c.GetName()).Returns("check");
        _conditionMock.Setup(c => c.IsSatisfied(_contextMock.Object)).ReturnsAsync(true);
        _conditionMock.Setup(c => c.GetSuccessMessage(_contextMock.Object)).Returns("Condition success!");

        StartTimer();
        await testling.ExecuteAsync(_contextMock.Object);
        StopTimer();

        AssertConditionExecutedWithinSeconds("1");
    }

    [Test]
    public async Task ShouldSatisfyWaitConditionOnLastAttempt()
    {
        const string seconds = "4";
        const string interval = "1000";

        var testling = GetWaitAction(seconds, interval);

        _contextMock.Reset();
        _conditionMock.Reset();
        PrepareContextMock("4000", interval);

        _conditionMock.SetupSequence(c => c.IsSatisfied(_contextMock.Object))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        _conditionMock.Setup(c => c.GetName()).Returns("check");
        _conditionMock.Setup(c => c.GetSuccessMessage(_contextMock.Object)).Returns("Condition success!");

        StartTimer();
        await testling.ExecuteAsync(_contextMock.Object);
        StopTimer();

        AssertConditionExecutedWithinSeconds(seconds);
    }

    [Test]
    public async Task ShouldSatisfyWaitConditionWithBiggerIntervalThanTimeout()
    {
        const string seconds = "1";
        const string interval = "10000";

        var testling = GetWaitAction(seconds, interval);

        _contextMock.Reset();
        _conditionMock.Reset();
        PrepareContextMock("1000", interval);

        _conditionMock.Setup(c => c.GetName()).Returns("check");
        _conditionMock.Setup(c => c.IsSatisfied(_contextMock.Object)).ReturnsAsync(true);
        _conditionMock.Setup(c => c.GetSuccessMessage(_contextMock.Object)).Returns("Condition success!");

        StartTimer();
        await testling.ExecuteAsync(_contextMock.Object);
        StopTimer();

        AssertConditionExecutedWithinSeconds(seconds);
    }

    [Test]
    public void ShouldNotSatisfyWaitCondition()
    {
        const string seconds = "3";
        const string interval = "1000";

        var testling = GetWaitAction(seconds, interval);

        _contextMock.Reset();
        _conditionMock.Reset();
        PrepareContextMock("3000", interval);

        _conditionMock.Setup(c => c.GetName()).Returns("check");
        _conditionMock.Setup(c => c.IsSatisfied(_contextMock.Object)).ReturnsAsync(false);
        _conditionMock.Setup(c => c.GetErrorMessage(_contextMock.Object)).Returns("Condition failed!");

        StartTimer();

        var exception = Assert.ThrowsAsync<AgenixSystemException>(() => testling.ExecuteAsync(_contextMock.Object));
        ClassicAssert.NotNull(exception, "Expected CoreSystemException to be thrown");

        StopTimer();

        AssertConditionExecutedWithinSeconds(seconds);
    }

    [Test]
    public void ShouldNotSatisfyWaitConditionWithBiggerIntervalThanTimeout()
    {
        const string seconds = "1";
        const string interval = "10000";

        var testling = GetWaitAction(seconds, interval);

        _contextMock.Reset();
        _conditionMock.Reset();
        PrepareContextMock("1000", interval);

        _conditionMock.Setup(c => c.GetName()).Returns("check");
        _conditionMock.Setup(c => c.IsSatisfied(_contextMock.Object)).ReturnsAsync(false);
        _conditionMock.Setup(c => c.GetErrorMessage(_contextMock.Object)).Returns("Condition failed!");

        StartTimer();

        var exception = Assert.ThrowsAsync<AgenixSystemException>(() => testling.ExecuteAsync(_contextMock.Object));
        ClassicAssert.NotNull(exception, "Expected CoreSystemException to be thrown");

        StopTimer();

        AssertConditionExecutedWithinSeconds(seconds);
    }

    private void PrepareContextMock(string waitTime, string interval)
    {
        _contextMock.Setup(c => c.ReplaceDynamicContentInString(waitTime, It.IsAny<bool>())).Returns(waitTime);
        _contextMock.Setup(c => c.ReplaceDynamicContentInString(interval, It.IsAny<bool>())).Returns(interval);
    }

    private Wait GetWaitAction(string waitTimeSeconds, string interval)
    {
        return new Wait.Builder<ICondition>()
            .Condition(_conditionMock.Object)
            .Interval(interval)
            .Seconds(long.Parse(waitTimeSeconds))
            .Build();
    }

    private void AssertConditionExecutedWithinSeconds(string seconds)
    {
        const long tolerance = 500L;
        var totalExecutionTime = _endTime - _startTime;
        var permittedTime = int.Parse(seconds) * 1000L + tolerance;

        ClassicAssert.LessOrEqual(totalExecutionTime, permittedTime,
            $"Expected conditional check to execute in {permittedTime} milliseconds but took {totalExecutionTime} milliseconds");
    }

    private void StartTimer()
    {
        _startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    private void StopTimer()
    {
        _endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
