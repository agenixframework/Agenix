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

using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Container.RepeatOnErrorUntilTrue.Builder;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
using Is = NHamcrest.Is;

namespace Agenix.Validation.NHamcrest.Tests.Container;

public class RepeatOnErrorTestActionBuilderTest : AbstractNUnitSetUp
{
    [Test]
    public async Task TestRepeatOnErrorBuilderWithHamcrestConditionExpression()
    {
        var builder = new DefaultTestCaseRunner(Context);
        builder.SetVariable("var", "foo");

        await builder.Run(
            RepeatOnError().AutoSleep(250)
                .Until("i gt 5")
                .Actions(Echo("${var}"), Sleep().Milliseconds(50), Echo("${var}"))
        );

        await builder.Run(
            RepeatOnError().AutoSleep(200)
                .Until(AssertThat(Is.EqualTo(5)).AsIteratingCondition())
                .Index("k")
                .StartsWith(2)
                .Actions(Echo("${var}"))
        );
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Context.GetVariable("i"), NUnit.Framework.Is.Not.Null);
            Assert.That(Context.GetVariable("i"), NUnit.Framework.Is.EqualTo("1"));
            Assert.That(Context.GetVariable("k"), NUnit.Framework.Is.Not.Null);
            Assert.That(Context.GetVariable("k"), NUnit.Framework.Is.EqualTo("2"));
        }

        var test = builder.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), NUnit.Framework.Is.EqualTo(2));
            Assert.That(test.GetActions()[0].GetType(), NUnit.Framework.Is.EqualTo(typeof(RepeatOnErrorUntilTrue)));
            Assert.That(test.GetActions()[0].Name, NUnit.Framework.Is.EqualTo("repeat-on-error"));
        }

        var container = (RepeatOnErrorUntilTrue)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.GetActionCount(), NUnit.Framework.Is.EqualTo(3));
            Assert.That(container.AutoSleep, NUnit.Framework.Is.EqualTo(250L));
            Assert.That(container.Condition, NUnit.Framework.Is.EqualTo("i gt 5"));
            Assert.That(container.GetStart(), NUnit.Framework.Is.EqualTo(1));
            Assert.That(container.GetIndexName(), NUnit.Framework.Is.EqualTo("i"));
            Assert.That(container.GetTestAction(0).GetType(), NUnit.Framework.Is.EqualTo(typeof(EchoAction)));
        }

        container = (RepeatOnErrorUntilTrue)test.GetActions()[1];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.GetActionCount(), NUnit.Framework.Is.EqualTo(1));
            Assert.That(container.AutoSleep, NUnit.Framework.Is.EqualTo(200L));
            Assert.That(container.GetStart(), NUnit.Framework.Is.EqualTo(2));
            Assert.That(container.GetIndexName(), NUnit.Framework.Is.EqualTo("k"));
            Assert.That(container.GetTestAction(0).GetType(), NUnit.Framework.Is.EqualTo(typeof(EchoAction)));
        }
    }
}
