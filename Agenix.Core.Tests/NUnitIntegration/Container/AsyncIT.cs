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
using Agenix.Api.Log;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using static Agenix.Core.Actions.DefaultTestActionBuilder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Actions.StopTimeAction.Builder;
using static Agenix.Core.Actions.TraceVariablesAction.Builder;
using static Agenix.Core.Container.Async.Builder;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
[Platform(Exclude = "Linux", Reason = "Only runs on non-Linux platforms.")]
public class AsyncIT
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AsyncIT));
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void AsyncContainer()
    {
        _gherkin.When(Async().Actions(
            StopTime(),
            Sleep().Milliseconds(500),
            Echo("Hello Agenix!"),
            StopTime()
        ));

        _gherkin.When(Async().Actions(
            Echo("Hello Agenix!"),
            Action(context => context.SetVariable("anonymous", "anonymous")),
            Sleep().Milliseconds(500),
            Action(context => Log.LogInformation(context.GetVariable("anonymous")))
        ));

        _gherkin.When(Async().Actions(
            StopTime(),
            Sleep().Milliseconds(200),
            Echo("Hello Agenix!"),
            StopTime()
        ));

        _gherkin.When(Async().Actions(
            Echo("Hello Agenix!"),
            Action(context => context.SetVariable("anonymous", "anonymous")),
            Sleep().Milliseconds(200),
            Action(context => Log.LogInformation(context.GetVariable("anonymous")))
        ));

        _gherkin.When(Sleep().Milliseconds(500));

        _gherkin.When(TraceVariables("anonymous"));
    }
}
