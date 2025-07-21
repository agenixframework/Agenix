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

namespace Agenix.Core.Tests.Functions;

public class RandomNumberFunctionTest : AbstractNUnitSetUp
{
    private readonly RandomNumberFunction _function = new();

    [Test]
    public void TestRandomStringFunction()
    {
        var parameters = new List<string> { "3" };

        Assert.That(int.Parse(_function.Execute(parameters, Context)), Is.LessThan(1000));

        parameters = ["3", "false"];
        var generated = _function.Execute(parameters, Context);

        Assert.That(generated.Length, Is.LessThanOrEqualTo(3));
        Assert.That(generated.Length, Is.GreaterThan(0));
    }

    [Test]
    public void TestLeadingZeroNumbers()
    {
        var generated = RandomNumberFunction.CheckLeadingZeros("0001", true);
        Console.WriteLine(generated);
        Assert.That(int.Parse(generated[..1]), Is.GreaterThan(0));

        generated = RandomNumberFunction.CheckLeadingZeros("0009", true);
        Assert.That(generated.Length, Is.EqualTo(4));

        generated = RandomNumberFunction.CheckLeadingZeros("00000", true);
        Assert.That(generated.Length, Is.EqualTo(5));
        Assert.That(int.Parse(generated[..1]), Is.GreaterThan(0));
        Assert.That(generated.EndsWith("0000"), Is.True);

        generated = RandomNumberFunction.CheckLeadingZeros("009809", true);
        Assert.That(generated.Length, Is.EqualTo(6));
        Assert.That(int.Parse(generated[..1]), Is.GreaterThan(0));
        Assert.That(generated.EndsWith("09809"), Is.True);

        generated = RandomNumberFunction.CheckLeadingZeros("01209", true);
        Assert.That(generated.Length, Is.EqualTo(5));
        Assert.That(int.Parse(generated[..1]), Is.GreaterThan(0));
        Assert.That(generated.EndsWith("1209"), Is.True);

        generated = RandomNumberFunction.CheckLeadingZeros("1209", true);
        Assert.That(generated.Length, Is.EqualTo(4));
        Assert.That(generated, Is.EqualTo("1209"));

        generated = RandomNumberFunction.CheckLeadingZeros("00000", false);
        Assert.That(generated.Length, Is.EqualTo(1));
        Assert.That(generated, Is.EqualTo("0"));

        generated = RandomNumberFunction.CheckLeadingZeros("0009", false);
        Assert.That(generated.Length, Is.EqualTo(1));
        Assert.That(generated, Is.EqualTo("9"));

        generated = RandomNumberFunction.CheckLeadingZeros("01209", false);
        Assert.That(generated.Length, Is.EqualTo(4));
        Assert.That(generated, Is.EqualTo("1209"));

        generated = RandomNumberFunction.CheckLeadingZeros("1209", false);
        Assert.That(generated.Length, Is.EqualTo(4));
        Assert.That(generated, Is.EqualTo("1209"));
    }

    [Test]
    public void TestWrongParameterUsage()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(["-1"], Context));
    }

    [Test]
    public void TestNoParameterUsage()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute([], Context));
    }

    [Test]
    public void TestTooManyParameters()
    {
        var parameters = new List<string> { "3", RandomStringFunction.Uppercase, "true", "too much" };
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(parameters, Context));
    }
}
