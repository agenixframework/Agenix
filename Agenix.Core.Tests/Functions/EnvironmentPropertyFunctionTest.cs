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

using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Core.Functions.Core;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Functions;

public class EnvironmentPropertyFunctionTest : AbstractNUnitSetUp
{
    private readonly EnvironmentPropertyFunction _function = new();
    private readonly Mock<IConfiguration> _mockEnvironment = new();

    [SetUp]
    public void SetUpMethod()
    {
        _function.SetEnvironment(_mockEnvironment.Object);
    }

    [Test]
    public void TestFunction()
    {
        _mockEnvironment.Setup(env => env["foo.property"]).Returns("Agenix rocks!");
        ClassicAssert.AreEqual("Agenix rocks!", _function.Execute(["foo.property"], null));
    }

    [Test]
    public void TestFunctionDefaultValue()
    {
        ClassicAssert.AreEqual("This is a default",
            _function.Execute(["bar.property", "This is a default"], null));
    }

    [Test]
    public void TestPropertyNotFound()
    {
        Assert.Throws<AgenixSystemException>(() => _function.Execute(["bar.property"], null));
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute([], null));
    }

    [Test]
    public void ShouldLookupFunction()
    {
        ClassicAssert.True(IFunction.Lookup().ContainsKey("env"));
        ClassicAssert.AreEqual(typeof(EnvironmentPropertyFunction), IFunction.Lookup()["env"].GetType());
        ClassicAssert.AreEqual(typeof(EnvironmentPropertyFunction),
            new DefaultFunctionLibrary().GetFunction("env").GetType());
    }
}
