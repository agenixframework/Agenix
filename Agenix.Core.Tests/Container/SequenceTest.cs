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

namespace Agenix.Core.Tests.Container;

public class SequenceTest : AbstractNUnitSetUp
{
    private readonly IAsyncTestAction _action = new Mock<IAsyncTestAction>().Object;

    [Test]
    public async Task TestSingleAction()
    {
        Mock.Get(_action).Reset();

        var sequenceAction = new Sequence.Builder()
            .Actions(_action)
            .Build();

        await sequenceAction.ExecuteAsync(Context);

        Mock.Get(_action).Verify(a => a.ExecuteAsync(Context, CancellationToken.None));
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

        var sequenceAction = new Sequence.Builder()
            .Actions(action1, action2, action3)
            .Build();

        await sequenceAction.ExecuteAsync(Context);

        Mock.Get(action1).Verify(a => a.ExecuteAsync(Context, CancellationToken.None));
        Mock.Get(action2).Verify(a => a.ExecuteAsync(Context, CancellationToken.None));
        Mock.Get(action3).Verify(a => a.ExecuteAsync(Context, CancellationToken.None));
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

        var sequenceAction = new Sequence.Builder()
            .Actions(new FailAction.Builder().Build(), action1, action2, action3)
            .Build();

        Assert.ThrowsAsync<AgenixSystemException>(async () => { await sequenceAction.ExecuteAsync(Context); });
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

        var sequenceAction = new Sequence.Builder()
            .Actions(action1, action2, action3, new FailAction.Builder().Build())
            .Build();

        Assert.ThrowsAsync<AgenixSystemException>(async () =>
        {
            await sequenceAction.ExecuteAsync(Context);
            Mock.Get(action1).Verify(a => a.ExecuteAsync(Context, CancellationToken.None));
            Mock.Get(action2).Verify(a => a.ExecuteAsync(Context, CancellationToken.None));
            Mock.Get(action3).Verify(a => a.ExecuteAsync(Context, CancellationToken.None));
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

        var sequenceAction = new Sequence.Builder()
            .Actions(action1, new FailAction.Builder().Build(), action2, action3)
            .Build();

        Assert.ThrowsAsync<AgenixSystemException>(async () =>
        {
            await sequenceAction.ExecuteAsync(Context);
            Mock.Get(action1).Verify(a => a.ExecuteAsync(Context, CancellationToken.None));
        });
    }
}
