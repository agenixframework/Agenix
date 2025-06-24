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

using Agenix.Core.Message;
using Agenix.Validation.Json.Validation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonPathVariableExtractorTest : AbstractNUnitSetUp
{
    private readonly string _jsonPayload =
        "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"numbers\": [10,20,30,40], \"id\":\"x123456789x\"}";

    [Test]
    public void TestExtractVariables()
    {
        var variableExtractor = new JsonPathVariableExtractor.Builder()
            .Expression("$.KeySet()", "keySet")
            .Expression("$['index']", "index")
            .Expression("$.numbers", "numbers")
            .Expression("$.numbers.Size()", "numbersSize")
            .Expression("$.person", "person")
            .Expression("$.person.name", "personName")
            .Expression("$.ToString()", "toString")
            .Expression("$.Values()", "values")
            .Expression("$.Size()", "size")
            .Expression("$.*", "all")
            .Expression("$", "root")
            .Build();

        variableExtractor.ExtractVariables(new DefaultMessage(_jsonPayload), Context);

        ClassicAssert.IsNotNull(Context.GetVariable("keySet"));
        Assert.That(Context.GetVariable("keySet"), Is.EqualTo("text, person, index, numbers, id"));


        ClassicAssert.IsNotNull(Context.GetVariable("index"));
        Assert.That(Context.GetVariable("index"), Is.EqualTo("5"));


        ClassicAssert.IsNotNull(Context.GetVariable("numbers"));
        Assert.That(Context.GetVariable("numbers"), Is.EqualTo("[10,20,30,40]"));


        ClassicAssert.IsNotNull(Context.GetVariable("numbersSize"));
        Assert.That(Context.GetVariable("numbersSize"), Is.EqualTo("4"));

        ClassicAssert.IsNotNull(Context.GetVariable("person"));
        Assert.That(Context.GetVariable("person"), Is.EqualTo("{\"name\":\"John\",\"surname\":\"Doe\"}"));

        ClassicAssert.IsNotNull(Context.GetVariable("personName"));
        Assert.That(Context.GetVariable("personName"), Is.EqualTo("John"));

        ClassicAssert.IsNotNull(Context.GetVariable("toString"));
        Assert.That(
            Context.GetVariable("toString"),
            Is.EqualTo(
                "{\"text\":\"Hello World!\",\"person\":{\"name\":\"John\",\"surname\":\"Doe\"},\"index\":5,\"numbers\":[10,20,30,40],\"id\":\"x123456789x\"}"));

        ClassicAssert.IsNotNull(Context.GetVariable("values"));
        Assert.That(Context.GetVariable("values"),
            Is.EqualTo("Hello World!, {\"name\":\"John\",\"surname\":\"Doe\"}, 5, [10,20,30,40], x123456789x"));

        ClassicAssert.IsNotNull(Context.GetVariable("size"));
        Assert.That(Context.GetVariable("size"), Is.EqualTo("5"));

        ClassicAssert.IsNotNull(Context.GetVariable("all"));
        Assert.That(Context.GetVariable("all"),
            Is.EqualTo("Hello World!, {\"name\":\"John\",\"surname\":\"Doe\"}, 5, [10,20,30,40], x123456789x"));

        ClassicAssert.IsNotNull(Context.GetVariable("root"));
        Assert.That(
            Context.GetVariable("root"),
            Is.EqualTo(
                "{\"text\":\"Hello World!\",\"person\":{\"name\":\"John\",\"surname\":\"Doe\"},\"index\":5,\"numbers\":[10,20,30,40],\"id\":\"x123456789x\"}"));
    }
}
