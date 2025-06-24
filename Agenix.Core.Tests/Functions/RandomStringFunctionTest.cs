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
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Functions;

public class RandomStringFunctionTest : AbstractNUnitSetUp
{
    private readonly RandomStringFunction _function = new();

    [Test]
    public void TestRandomStringFunction()
    {
        var parameters = new List<string> { "3" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);

        parameters = ["3", RandomStringFunction.Uppercase];

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);

        parameters = ["3", RandomStringFunction.Lowercase];

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);

        parameters = ["3", RandomStringFunction.Mixed];

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);

        parameters = new List<string> { "3", "UNKNOWN" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);
    }

    [Test]
    public void TestWithNumbers()
    {
        var parameters = new List<string> { "10", RandomStringFunction.Uppercase, "true" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 10);

        parameters = new List<string> { "10", RandomStringFunction.Lowercase, "true" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 10);

        parameters = new List<string> { "10", RandomStringFunction.Mixed, "true" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 10);

        parameters = new List<string> { "10", "UNKNOWN", "true" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 10);
    }

    [Test]
    public void TestWrongParameterUsage()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string> { "-1" }, Context));
    }

    [Test]
    public void TestNoParameterUsage()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string>(), Context));
    }

    [Test]
    public void TestTooManyParameters()
    {
        var parameters = new List<string> { "3", RandomStringFunction.Uppercase, "true", "too much" };
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(parameters, Context));
    }
}
