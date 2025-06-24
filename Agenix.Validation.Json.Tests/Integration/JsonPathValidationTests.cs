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
using Agenix.Api.Annotations;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Endpoint.Direct.Annotation;
using Agenix.Core.Message;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Validation.Json.Dsl.JsonPathSupport;

namespace Agenix.Validation.Json.Tests.Integration;

[NUnitAgenixSupport]
public class JsonPathExpressionValidationIT
{
    private const string JsonBody = "{\"user\":\"andy\", \"age\": 32}";

    [AgenixEndpoint]
    [DirectEndpointConfig(Queue = "test")]
    private DirectEndpoint _direct;

    [BindToRegistry] private IMessageQueue test = new DefaultMessageQueue("test");

    [AgenixResource] protected ITestCaseRunner runner;

    [AgenixResource] protected TestContext context;


    [Test]
    public void ShouldPerformJsonPathValidation()
    {
        runner.Given(Send(_direct)
            .Message()
            .Type(MessageType.JSON)
            .Body(JsonBody)
        );

        runner.Then(Receive(_direct)
            .Message()
            .Body("{\"user\":\"@Ignore@\", \"age\": \"@Ignore@\"}")
            .Validate(JsonPath()
                .Expression("$.user", "andy")
                .Expression("$.age", 32))
        );
    }

    [Test]
    public void ShouldPerformJsonPathValidationWithMessageProcessing()
    {
        runner.Given(Send(_direct)
            .Message()
            .Type(MessageType.JSON)
            .Body("{\"user\":\"?\", \"age\": 0}")
            .Process(JsonPath().Expression("$.user", "andy"))
            .Process(JsonPath().Expression("$.age", 32))
        );

        runner.Then(Receive(_direct)
            .Message()
            .Type(MessageType.JSON)
            .Body("{\"user\":\"x\", \"age\": \"99\"}")
            .Process(JsonPath().Expression("$.age", 32))
            .Process(JsonPath().Expression("$.user", "andy"))
            .Validate(JsonPath()
                .Expression("$.user", "andy")
                .Expression("$.age", 32))
        );
    }

    [Test]
    public void ShouldFailOnJsonPathValidation()
    {
        runner.Given(Send(_direct)
            .Message()
            .Type(MessageType.JSON)
            .Body(JsonBody)
        );

        Assert.Throws<TestCaseFailedException>(() =>
            runner.Then(Receive(_direct)
                .Message()
                .Body("{\"user\":\"@Ignore@\", \"age\": \"@Ignore@\"}")
                .Validate(JsonPath().Expression("$.user", "wrong"))
            )
        );
    }

    [Test]
    public void ShouldPerformJsonPathValidationWithMultipleExpressions()
    {
        runner.Given(Send(_direct)
            .Message()
            .Type(MessageType.JSON)
            .Body(JsonBody)
        );

        runner.Then(Receive(_direct)
            .Message()
            .Body("{\"user\":\"@Ignore@\", \"age\": \"@Ignore@\"}")
            .Validate(JsonPath().Expression("$.user", "andy"))
            .Validate(JsonPath().Expression("$.age", 32))
        );
    }

    [Test]
    public void ShouldFailOnJsonPathValidationWithMultipleExpressions()
    {
        runner.Given(Send(_direct)
            .Message()
            .Type(MessageType.JSON)
            .Body(JsonBody)
        );

        Assert.Throws<TestCaseFailedException>(() =>
            runner.Then(Receive(_direct)
                .Message()
                .Body("{\"user\":\"@Ignore@\", \"age\": \"@Ignore@\"}")
                .Validate(JsonPath().Expression("$.user", "andy"))
                .Validate(JsonPath().Expression("$.age", 0))
            )
        );
    }
}
