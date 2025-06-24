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

using Agenix.Api.Container;
using Agenix.Core.Actions;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.Tests.Actions;

public class StopTimerActionTest : AbstractNUnitSetUp
{
    [Test]
    public void ShouldStopSpecificTimer()
    {
        const string timerId = "timer#1";

        var mock = new Mock<IStopTimer>();
        var timer = mock.Object;
        Context.RegisterTimer(timerId, timer);

        var stopTimer = new StopTimerAction.Builder()
            .Id(timerId)
            .Build();

        Assert.That(stopTimer.TimerId, Is.EqualTo(timerId));
        stopTimer.Execute(Context);

        mock.Verify(x => x.StopTimer(), Times.Once);
    }

    [Test]
    public void ShouldStopAllTimers()
    {
        const string timerId1 = "timer#1";
        const string timerId2 = "timer#2";

        var mock1 = new Mock<IStopTimer>();
        var timer1 = mock1.Object;
        Context.RegisterTimer(timerId1, timer1);

        var mock2 = new Mock<IStopTimer>();
        var timer2 = mock2.Object;
        Context.RegisterTimer(timerId2, timer2);

        var stopTimer = new StopTimerAction.Builder().Build();
        stopTimer.Execute(Context);

        mock1.Verify(x => x.StopTimer(), Times.Once);
        mock2.Verify(x => x.StopTimer(), Times.Once);
    }

    [Test]
    public void ShouldNotFailWhenStoppingTimerWithUnknownId()
    {
        var stopTimer = new StopTimerAction.Builder()
            .Id("some-unknown-timer")
            .Build();
        stopTimer.Execute(Context);
    }
}
