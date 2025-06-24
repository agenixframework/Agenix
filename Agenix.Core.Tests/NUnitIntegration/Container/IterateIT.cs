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
using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.DefaultTestActionBuilder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Container.Iterate.Builder;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class IterateIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void IterateContainer()
    {
        _gherkin.Given(CreateVariable("max", "3"));

        _gherkin.When(Iterate().Condition("i lt= agenix:RandomNumber(1)").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("i lt 20").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("(i lt 5) or (i = 5)").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("(i lt 5) and (i lt 3)").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("i = 0").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("${max} gt= i").Index("i").Actions(Echo("index is: ${i}")));

        var anonymous = Action(context => Console.WriteLine(context.GetVariable("index"))).Build();

        _gherkin.When(Iterate().Condition("i lt 5").Index("i")
            .Actions(CreateVariable("index", "${i}"), new FuncITestActionBuilder<ITestAction>(() => anonymous)));
    }
}
