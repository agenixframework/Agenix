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

using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Core.Actions;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.Tests.Actions;

public class ApplyTestBehaviorActionTest : AbstractNUnitSetUp
{
    private Mock<IAsyncTestAction> _mock;
    private Mock<IAsyncTestActionRunner> _runner;

    [SetUp]
    public void SetupMock()
    {
        _runner = new Mock<IAsyncTestActionRunner>();
        _mock = new Mock<IAsyncTestAction>();
    }

    [Test]
    public async Task ShouldApply()
    {
        var applyBehavior = new ApplyAsyncTestBehaviorAction.Builder()
            .Behavior(runner => runner.Run(_mock.Object))
            .On(_runner.Object)
            .Build();

        await applyBehavior.ExecuteAsync(Context);

        _runner.Verify(r => r.Run(_mock.Object));
    }
}
