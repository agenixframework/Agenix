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

using System.IO;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Exceptions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.DefaultTestActionBuilder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.FailAction.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class AssertIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinAsyncTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public async Task AssertAction()
    {
        await _gherkin.Given(CreateVariable("failMessage", "Something went wrong!"));

        await _gherkin.Then(Assert().Exception(typeof(AgenixSystemException)).When(Fail("Fail once")));

        await _gherkin.Then(Assert().Exception(typeof(AgenixSystemException))
            .Message("Fail again")
            .When(Fail("Fail again")));

        await _gherkin.Then(Assert().Exception(typeof(AgenixSystemException))
            .Message("${failMessage}")
            .When(Fail("${failMessage}")));

        await _gherkin.Then(Assert().Exception(typeof(AgenixSystemException))
            .Message("@Contains('wrong')@")
            .When(Fail("${failMessage}")));

        await _gherkin.Then(Assert().Exception(typeof(ValidationException))
            .When(Assert().Exception(typeof(IOException))
                .When(Fail("Fail another time"))));

        await _gherkin.Then(Assert().Exception(typeof(ValidationException))
            .When(Assert().Exception(typeof(AgenixSystemException))
                .Message("Fail again")
                .When(Fail("Fail with nice error message"))));

        await _gherkin.Then(Assert().Exception(typeof(ValidationException))
            .When(Assert().Exception(typeof(AgenixSystemException))
                .Message("Fail again")
                .When(Echo("Nothing fails here"))));

        await _gherkin.Then(Assert().Exception(typeof(AgenixSystemException))
            .Message("Unknown variable 'foo'")
            .When(Action(context =>
            {
                context.GetVariable("foo");
                return Task.CompletedTask;
            })));
    }
}
