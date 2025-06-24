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
using Agenix.Api.Exceptions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.FailAction.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;
using static Agenix.Core.Container.RepeatOnErrorUntilTrue.Builder;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class RepeatOnErrorIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void RepeatOnErrorContainer()
    {
        _gherkin.Given(CreateVariable("message", "Hello Test Framework!"));

        _gherkin.When(RepeatOnError().Name("Repeat just 1").Until("i = 5").Index("i")
            .Actions(Echo("${i}. Attempt: ${message}")));

        _gherkin.When(RepeatOnError().Name("Repeat just 1 with the sleep of 500 ms").Until("i = 5").AutoSleep(500)
            .Index("i").Actions(Echo("${i}. Attempt: ${message}")));

        _gherkin.When(Assert().Exception(typeof(AgenixSystemException))
            .When(
                RepeatOnError().Name("Repeat 3 times")
                    .Until("i = 3")
                    .AutoSleep(200)
                    .Index("i")
                    .Actions(Echo("${i}. Attempt: ${message}"), Fail())
            ));
    }
}
