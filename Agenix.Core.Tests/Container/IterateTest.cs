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
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Core.Tests.Container;

public class IterateTest : AbstractNUnitSetUp
{
    private readonly ITestAction _action = new Mock<ITestAction>().Object;

    public static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData("i lt= 5");
            yield return new TestCaseData("@LowerThan(6)@");
        }
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void TestIteration(string expression)
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Condition(expression)
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("5"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(5));
    }

    [Test]
    public void TestStep()
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Step(2)
            .Condition("i lt= 10")
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("9"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(5));
    }

    [Test]
    public void TestStart()
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Step(2)
            .StartsWith(2)
            .Condition("i lt= 10")
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("10"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(5));
    }

    [Test]
    public void TestNoIterationBasedOnCondition()
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Condition("i lt 0")
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.Throws<KeyNotFoundException>(() =>
        {
            var variable = Context.GetVariables()["i"];
        });
    }

    [Test]
    public void TestIterationWithIndexManipulation()
    {
        var incrementTestAction = DefaultTestActionBuilder
            .Action(context =>
            {
                var end = long.Parse(context.GetVariable("end"));
                context.SetVariable("end", (end - 25).ToString());
            })
            .Build();

        Mock.Get(_action).Reset();

        Context.SetVariable("end", 100);

        var iterate = new Iterate.Builder()
            .Condition("i lt ${end}")
            .Index("i")
            .Actions(_action, incrementTestAction)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("4"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(4));
    }

    [Test]
    public void TestIterationConditionExpression()
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Condition((index, context) => index <= 5)
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("5"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(5));
    }
}
