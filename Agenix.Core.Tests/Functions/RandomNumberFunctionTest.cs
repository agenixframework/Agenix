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
using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Functions;

public class RandomNumberFunctionTest : AbstractNUnitSetUp
{
    private readonly RandomNumberFunction _function = new();

    [Test]
    public void TestRandomStringFunction()
    {
        var parameters = new List<string> { "3" };

        ClassicAssert.Less(int.Parse(_function.Execute(parameters, Context)), 1000);

        parameters = new List<string> { "3", "false" };
        var generated = _function.Execute(parameters, Context);

        ClassicAssert.LessOrEqual(generated.Length, 3);
        ClassicAssert.Greater(generated.Length, 0);
    }

    [Test]
    public void TestLeadingZeroNumbers()
    {
        var generated = RandomNumberFunction.CheckLeadingZeros("0001", true);
        Console.WriteLine(generated);
        ClassicAssert.Greater(int.Parse(generated.Substring(0, 1)), 0);

        generated = RandomNumberFunction.CheckLeadingZeros("0009", true);
        ClassicAssert.AreEqual(generated.Length, 4);

        generated = RandomNumberFunction.CheckLeadingZeros("00000", true);
        ClassicAssert.AreEqual(generated.Length, 5);
        ClassicAssert.Greater(int.Parse(generated.Substring(0, 1)), 0);
        ClassicAssert.IsTrue(generated.EndsWith("0000"));

        generated = RandomNumberFunction.CheckLeadingZeros("009809", true);
        ClassicAssert.AreEqual(generated.Length, 6);
        ClassicAssert.Greater(int.Parse(generated.Substring(0, 1)), 0);
        ClassicAssert.IsTrue(generated.EndsWith("09809"));

        generated = RandomNumberFunction.CheckLeadingZeros("01209", true);
        ClassicAssert.AreEqual(generated.Length, 5);
        ClassicAssert.Greater(int.Parse(generated.Substring(0, 1)), 0);
        ClassicAssert.IsTrue(generated.EndsWith("1209"));

        generated = RandomNumberFunction.CheckLeadingZeros("1209", true);
        ClassicAssert.AreEqual(generated.Length, 4);
        ClassicAssert.AreEqual(generated, "1209");

        generated = RandomNumberFunction.CheckLeadingZeros("00000", false);
        ClassicAssert.AreEqual(generated.Length, 1);
        ClassicAssert.AreEqual(generated, "0");

        generated = RandomNumberFunction.CheckLeadingZeros("0009", false);
        ClassicAssert.AreEqual(generated.Length, 1);
        ClassicAssert.AreEqual(generated, "9");

        generated = RandomNumberFunction.CheckLeadingZeros("01209", false);
        ClassicAssert.AreEqual(generated.Length, 4);
        ClassicAssert.AreEqual(generated, "1209");

        generated = RandomNumberFunction.CheckLeadingZeros("1209", false);
        ClassicAssert.AreEqual(generated.Length, 4);
        ClassicAssert.AreEqual(generated, "1209");
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
