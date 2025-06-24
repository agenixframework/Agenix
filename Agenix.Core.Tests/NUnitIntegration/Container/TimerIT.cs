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

using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Actions.StopTimerAction.Builder;
using static Agenix.Core.Container.FinallySequence.Builder;
using static Agenix.Core.Container.Timer.Builder;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class TimerIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void TimerTest()
    {
        _gherkin.Given(DoFinally().Actions(StopTimer("forkedTimer")));

        _gherkin.When(Timer()
            .TimerId("forkedTimer")
            .Interval(100)
            .Fork(true)
            .Actions(
                Echo(
                    "I'm going to run in the background and let some other test actions run (nested action run ${forkedTimer-index} times)"),
                Sleep().Milliseconds(50)
            ));

        _gherkin.When(Timer()
            .RepeatCount(3)
            .Interval(100)
            .Delay(50)
            .Actions(
                Sleep().Milliseconds(50),
                Echo(
                    "I'm going to repeat this message 3 times before the next test actions are executed")
            ));
        _gherkin.Then(Echo(
            "Test almost complete. Make sure all timers running in the background are stopped"));
    }
}
