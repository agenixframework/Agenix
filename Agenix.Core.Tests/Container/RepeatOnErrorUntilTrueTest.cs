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

using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Core.Tests.Container;

/// <summary>
///     Test class for executing scenarios using RepeatOnErrorUntilTrue container logic in the NUnit framework.
/// </summary>
/// <remarks>
///     Tests include verifying successful iteration on initial execution and handling errors when conditions are not met.
///     It also verifies the integration and behavior of mocked actions along with expression-based conditions.
/// </remarks>
public class RepeatOnErrorUntilTrueTest : AbstractNUnitSetUp
{
    private readonly Mock<ITestAction> _action = new();

    [Test]
    [TestCaseSource(nameof(ExpressionProvider))]
    public void TestSuccessOnFirstIteration(string expression)
    {
        _action.Reset();

        var repeat = new RepeatOnErrorUntilTrue.Builder()
            .Condition(expression)
            .Index("i")
            .Actions(context => { _action.Object.Execute(context); })
            .Build();

        repeat.Execute(Context);

        _action.Verify(a => a.Execute(Context));
    }

    public static IEnumerable<object[]> ExpressionProvider()
    {
        yield return ["i = 5"];
        yield return ["@greaterThan(4)@"];
    }

    [Test]
    public void TestRepeatOnErrorNoSuccess()
    {
        Assert.Throws<AgenixSystemException>(() =>
        {
            _action.Reset();

            var repeat = new RepeatOnErrorUntilTrue.Builder()
                .AutoSleep(0)
                .Condition("i = 5")
                .Index("i")
                .Actions(_action.Object, new FailAction.Builder().Build())
                .Build();

            repeat.Execute(Context);

            _action.Verify(a => a.Execute(Context), Times.Exactly(4));
        });
    }

    [Test]
    public void TestRepeatOnErrorNoSuccessConditionExpression()
    {
        Assert.Throws<AgenixSystemException>(() =>
        {
            _action.Reset();

            var repeat = new RepeatOnErrorUntilTrue.Builder()
                .AutoSleep(0)
                .Condition((index, ctx) => index == 5)
                .Index("i")
                .Actions(_action.Object, new FailAction.Builder().Build())
                .Build();

            repeat.Execute(Context);

            _action.Verify(a => a.Execute(Context), Times.Exactly(4));
        });
    }
}
