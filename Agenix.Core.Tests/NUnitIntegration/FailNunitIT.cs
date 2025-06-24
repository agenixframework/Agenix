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
using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;

namespace Agenix.Core.Tests.NUnitIntegration;

[NUnitAgenixSupport]
public class FailNunitIT
{
    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private ITestActionRunner _runner;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    [Category("ShouldFailGroup")]
    public void FailTest()
    {
        Assert.Throws<TestCaseFailedException>(() =>
            _runner.Run(EchoAction.Builder.Echo("This test should fail because of unknown variable ${foo}")));
    }

    [Test]
    [Category("ShouldFailGroup")]
    public void FailRuntimeExceptionTest()
    {
        Assert.Throws<TestCaseFailedException>(() =>
            _runner.Run(DefaultTestActionBuilder.Action(_ =>
                throw new Exception("This test should fail because of unknown variable ${foo}"))));
    }
}
