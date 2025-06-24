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
using Agenix.Core.Message;
using Agenix.Validation.Json.Validation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonPathMessageProcessorTest : AbstractNUnitSetUp
{
    [Test]
    public void TestConstructWithJsonPath()
    {
        var message = new DefaultMessage("{ \"TestMessage\": { \"Text\": \"Hello World!\" }}");

        var jsonPathExpressions = new Dictionary<string, object> { { "$.TestMessage.Text", "Hello!" } };

        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        processor.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Text\":\"Hello!\"}}"));
    }

    [Test]
    public void TestConstructWithJsonPathMultipleValues()
    {
        var message = new DefaultMessage("{ \"TestMessage\": { \"Text\": \"Hello World!\", \"Id\": 1234567}}");

        var jsonPathExpressions = new Dictionary<string, object>
        {
            { "$.TestMessage.Text", "Hello!" }, { "$.TestMessage.Id", "9999999" }
        };

        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        processor.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Text\":\"Hello!\",\"Id\":9999999}}"));
    }

    [Test]
    public void TestConstructWithJsonPathWithArrays()
    {
        var message =
            new DefaultMessage(
                "{ \"TestMessage\": [{ \"Text\": \"Hello World!\" }, { \"Text\": \"Another Hello World!\" }]}");

        var jsonPathExpressions = new Dictionary<string, object> { { "$..Text", "Hello!" } };

        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        processor.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>(),
            Is.EqualTo("{\"TestMessage\":[{\"Text\":\"Hello!\"},{\"Text\":\"Hello!\"}]}"));
    }

    [Test]
    public void TestConstructWithJsonPathNoResult()
    {
        var message = new DefaultMessage("{ \"TestMessage\": { \"Text\": \"Hello World!\" }}");

        var jsonPathExpressions = new Dictionary<string, object> { { "$.TestMessage.Unknown", "Hello!" } };

        var processor = new JsonPathMessageProcessor.Builder()
            .IgnoreNotFound(true)
            .Expressions(jsonPathExpressions)
            .Build();

        processor.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Text\":\"Hello World!\"}}"));
    }

    [Test]
    public void TestConstructFailOnUnknownJsonPath()
    {
        var message = new DefaultMessage("{ \"TestMessage\": { \"Text\": \"Hello World!\" }}");

        var jsonPathExpressions = new Dictionary<string, object> { { "$.TestMessage.Unknown", "Hello!" } };

        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        Assert.Throws<UnknownElementException>(() => processor.ProcessMessage(message, Context));
    }
}
