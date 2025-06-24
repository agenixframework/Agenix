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
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.FailAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;
using static Agenix.Core.Container.Iterate.Builder;
using static Agenix.Core.Container.Parallel.Builder;
using static Agenix.Core.Container.Sequence.Builder;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class ParallelIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void ParallelContainer()
    {
        _gherkin.When(Parallel().Actions(
            Sleep().Milliseconds(150),
            Sequential().Actions(
                Sleep().Milliseconds(100),
                Echo("1")
            ),
            Echo("2"),
            Echo("3"),
            Iterate()
                .Condition("i lt= 5").Index("i").Actions(Echo("10"))
        ));

        _gherkin.When(Assert().Exception(typeof(AgenixSystemException))
            .When(
                Parallel().Actions(
                    Sleep().Milliseconds(150),
                    Sequential().Actions(
                        Sleep().Milliseconds(100),
                        Fail("This went wrong too"),
                        Echo("1")
                    ),
                    Echo("2"),
                    Fail("This went wrong too"),
                    Echo("3"),
                    Iterate()
                        .Condition("i lt= 5").Index("i").Actions(Echo("10"))
                )));
    }
}
