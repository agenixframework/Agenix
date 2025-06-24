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
using Agenix.Validation.Json.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Functions;

public class JsonPathFunctionTest : AbstractNUnitSetUp
{
    private readonly JsonPathFunction _function = new();

    private readonly string _jsonSource = @"{ 'person': { 'name': 'Sheldon', 'age': '29' } }";

    [Test]
    public void TestExecuteJsonPath()
    {
        var parameters = new List<string> { _jsonSource, "$.person.name" };
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("Sheldon"));
    }

    [Test]
    public void TestExecuteJsonPathWithKeySet()
    {
        var parameters = new List<string> { _jsonSource, "$.person.KeySet()" };
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("name, age"));
    }

    [Test]
    public void TestExecuteJsonPathWithValues()
    {
        var parameters = new List<string> { _jsonSource, "$.person.Values()" };
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("Sheldon, 29"));
    }

    [Test]
    public void TestExecuteJsonPathWithSize()
    {
        var parameters = new List<string> { _jsonSource, "$.person.Size()" };
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("2"));
    }

    [Test]
    public void TestExecuteJsonPathWithToString()
    {
        var parameters = new List<string> { _jsonSource, "$.person.ToString()" };
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("{\"name\":\"Sheldon\",\"age\":\"29\"}"));
    }

    [Test]
    public void TestExecuteJsonPathUnknown()
    {
        var parameters = new List<string> { _jsonSource, "$.person.unknown" };
        Assert.Throws<AgenixSystemException>(() => _function.Execute(parameters, Context));
    }
}
