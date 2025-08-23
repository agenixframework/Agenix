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

using Agenix.Api;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
using Is = NHamcrest.Is;

namespace Agenix.Validation.NHamcrest.Tests.Container;

public class RepeatOnErrorUntilTrueTest : AbstractNUnitSetUp
{
    private readonly Mock<IAsyncTestAction> _action = new();

    [Test]
    public void TestRepeatOnErrorNoSuccessHamcrestConditionExpression()
    {
        Assert.ThrowsAsync<AgenixSystemException>(async () =>
        {
            _action.Reset();

            var repeat = new RepeatOnErrorUntilTrue.Builder()
                .AutoSleep(0)
                .Condition(AssertThat(Is.EqualTo(5)).AsIteratingCondition())
                .Index("i")
                .Actions(_action.Object, new FailAction.Builder().Build())
                .Build();

            await repeat.ExecuteAsync(Context);
            _action.Verify(a => a.ExecuteAsync(Context, CancellationToken.None), Times.Exactly(4));
        });
    }
}
