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
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;

namespace Agenix.Validation.Xml.Tests.Integration;

/// <summary>
///     Test sends messages to a message channel and receives these messages performing validation.
/// </summary>
[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class MessageValidationIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void MessageValidationTest()
    {
        // Test validation success
        runner.Given(Echo("Test validation success"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        // Test validation success - auto select message type
        runner.Given(Echo("Test validation success - auto select message type"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        // Test header validation success
        runner.Given(Echo("Test header validation success"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Header("Operation", "sayHello")
        );

        runner.When(Send("hello.endpoint")
            .Message()
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Header("Operation", "sayHello")
        );

        // Test header validation success - auto select message type
        runner.Given(Echo("Test header validation success - auto select message type"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Header("Operation", "sayHello")
        );

        // Test validation errors
        runner.Given(Echo("Test validation errors"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Type(MessageType.XML)
                .Body("<Text>Goodbye Agenix</Text>")
                .Header("Operation", "sayHello")
            )
        );

        // Test validation errors - auto select message type
        runner.Given(Echo("Test validation errors - auto select message type"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Body("<Text>Goodbye Agenix</Text>")
                .Header("Operation", "sayHello")
            )
        );

        // Test header validation error
        runner.Given(Echo("Test header validation error"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Type(MessageType.XML)
                .Header("Operation", "sayGoodbye")
            )
        );

        runner.When(Send("hello.endpoint")
            .Message()
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Type(MessageType.XML)
                .Header("Operation", "sayGoodbye")
            )
        );

        // Test header validation error - auto select a message type
        runner.Given(Echo("Test header validation error - auto select message type"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Header("Operation", "sayGoodbye")
            )
        );
    }
}
