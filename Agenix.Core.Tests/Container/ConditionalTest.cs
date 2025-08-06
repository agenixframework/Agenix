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

using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Container;

public class ConditionalTest : AbstractNUnitSetUp
{
    private readonly IAsyncTestAction _action = new Mock<IAsyncTestAction>().Object;

    [Test]
    public async Task TestConditionFalse()
    {
        Mock.Get(_action).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("1 = 0")
            .Actions(_action)
            .Build();

        await conditionalAction.DoExecute(Context);

        Mock.Get(_action).Verify(a => a.ExecuteAsync(It.IsAny<TestContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task TestConditionMatcherFalse()
    {
        Mock.Get(_action).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("@LowerThan(-1)@")
            .Actions(_action)
            .Build();

        await conditionalAction.DoExecute(Context);

        Mock.Get(_action).Verify(a => a.ExecuteAsync(It.IsAny<TestContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task TestSingleAction()
    {
        Mock.Get(_action).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(_action)
            .Build();

        await conditionalAction.DoExecute(Context);

        Mock.Get(_action).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task TestMatcherSingleAction()
    {
        Mock.Get(_action).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("@Empty()@")
            .Actions(_action)
            .Build();

        await conditionalAction.DoExecute(Context);

        Mock.Get(_action).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task TestMultipleActions()
    {
        var action1 = new Mock<IAsyncTestAction>().Object;
        var action2 = new Mock<IAsyncTestAction>().Object;
        var action3 = new Mock<IAsyncTestAction>().Object;

        Mock.Get(action1).Reset();
        Mock.Get(action2).Reset();
        Mock.Get(action3).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(action1, action2, action3)
            .Build();

        await conditionalAction.DoExecute(Context);

        Mock.Get(action1).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()), Times.Once);
        Mock.Get(action2).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()), Times.Once);
        Mock.Get(action3).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void TestFirstActionFailing()
    {
        var action1 = new Mock<IAsyncTestAction>().Object;
        var action2 = new Mock<IAsyncTestAction>().Object;
        var action3 = new Mock<IAsyncTestAction>().Object;

        Mock.Get(action1).Reset();
        Mock.Get(action2).Reset();
        Mock.Get(action3).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(new FailAction.Builder().Build(), action1, action2, action3)
            .Build();

        Assert.ThrowsAsync<AgenixSystemException>(async () => { await conditionalAction.DoExecute(Context); });
    }

    [Test]
    public void TestLastActionFailing()
    {
        var action1 = new Mock<IAsyncTestAction>().Object;
        var action2 = new Mock<IAsyncTestAction>().Object;
        var action3 = new Mock<IAsyncTestAction>().Object;

        Mock.Get(action1).Reset();
        Mock.Get(action2).Reset();
        Mock.Get(action3).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(action1, action2, action3, new FailAction.Builder().Build())
            .Build();

        Assert.ThrowsAsync<AgenixSystemException>(async () =>
        {
            await conditionalAction.DoExecute(Context);
            // The following verifications are not reached if an exception is thrown here.
            Mock.Get(action1).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()));
            Mock.Get(action2).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()));
            Mock.Get(action3).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()));
        });
    }

    [Test]
    public void TestFailingAction()
    {
        var action1 = new Mock<IAsyncTestAction>().Object;
        var action2 = new Mock<IAsyncTestAction>().Object;
        var action3 = new Mock<IAsyncTestAction>().Object;

        Mock.Get(action1).Reset();
        Mock.Get(action2).Reset();
        Mock.Get(action3).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(action1, new FailAction.Builder().Build(), action2, action3)
            .Build();

        Assert.ThrowsAsync<AgenixSystemException>(async () =>
        {
            await conditionalAction.DoExecute(Context);
            // The following verifications are not reached if an exception is thrown here.
            Mock.Get(action1).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()));
            Mock.Get(action2).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()));
            Mock.Get(action3).Verify(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()));
        });
    }
}
