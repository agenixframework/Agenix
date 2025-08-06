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
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Moq;
using NUnit.Framework;
using static Agenix.Core.Container.Parallel;

namespace Agenix.Core.Tests.Container;

public class ParallelTest : AbstractNUnitSetUp
{
    private readonly Mock<IAsyncTestAction> _actionMock = new();

    [Test]
    public async Task TestSingleAction()
    {
        var parallelAction = new Builder().Build();

        _actionMock.Reset();
        _actionMock
            .Setup(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var actionList = new List<IAsyncTestAction> { _actionMock.Object };

        parallelAction.SetActions(actionList);
        await parallelAction.DoExecute(Context);

        _actionMock.Verify(action => action.ExecuteAsync(Context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task TestParallelMultipleActions()
    {
        var parallelAction = new Builder().Build();

        _actionMock.Reset();
        _actionMock
            .Setup(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var actionList = new List<IAsyncTestAction>
        {
            new EchoAction.Builder().Build(), _actionMock.Object, new EchoAction.Builder().Build()
        };

        parallelAction.SetActions(actionList);

        await parallelAction.DoExecute(Context);

        _actionMock.Verify(action => action.ExecuteAsync(Context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void TestParallelActions()
    {
        var parallelAction = new Builder().Build();

        var actionList = new List<IAsyncTestAction>
        {
            new EchoAction.Builder().Build(), new EchoAction.Builder().Build(), new EchoAction.Builder().Build()
        };

        var sleep = new SleepAction.Builder()
            .Milliseconds(300L)
            .Build();
        actionList.Add(sleep);

        parallelAction.SetActions(actionList);

        Assert.That(async () => await parallelAction.DoExecute(Context), Throws.Nothing);
    }

    [Test]
    public void TestOneActionThatIsFailing()
    {
        var parallelAction = new Builder().Build();

        var actionList = new List<IAsyncTestAction> { new FailAction.Builder().Build() };

        parallelAction.SetActions(actionList);

        Assert.ThrowsAsync<AgenixSystemException>(async () => await parallelAction.DoExecute(Context));
    }

    [Test]
    public void TestOnlyActionFailingActions()
    {
        var parallelAction = new Builder().Build();

        var actionList = new List<IAsyncTestAction>
        {
            new FailAction.Builder().Build(), new FailAction.Builder().Build(), new FailAction.Builder().Build()
        };

        parallelAction.SetActions(actionList);

        Assert.ThrowsAsync<ParallelContainerException>(async () => await parallelAction.DoExecute(Context));
    }

    [Test]
    public void TestSingleFailingAction()
    {
        var parallelAction = new Builder().Build();

        var actionList = new List<IAsyncTestAction>
        {
            new EchoAction.Builder().Build(), new FailAction.Builder().Build(), new EchoAction.Builder().Build()
        };

        parallelAction.SetActions(actionList);

        Assert.ThrowsAsync<AgenixSystemException>(async () => await parallelAction.DoExecute(Context));
    }

    [Test]
    public void TestSomeFailingActions()
    {
        var parallelAction = new Builder().Build();

        _actionMock.Reset();
        _actionMock
            .Setup(a => a.ExecuteAsync(Context, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var actionList = new List<IAsyncTestAction>
        {
            new EchoAction.Builder().Build(),
            new FailAction.Builder().Build(),
            _actionMock.Object,
            new FailAction.Builder().Build()
        };

        parallelAction.SetActions(actionList);

        Assert.ThrowsAsync<ParallelContainerException>(async () => await parallelAction.DoExecute(Context));

        _actionMock.Verify(action => action.ExecuteAsync(Context, It.IsAny<CancellationToken>()), Times.Once);
    }
}
