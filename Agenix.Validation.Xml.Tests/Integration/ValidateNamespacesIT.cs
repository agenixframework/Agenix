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
using Agenix.Validation.Xml.Dsl;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class ValidateNamespacesIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void ValidateNamespaces()
    {
        runner.Given(Echo("Test: Success with single namespace validation"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<Message>Hello</Message>" +
                  "</trq:TestRequest>")
        );

        runner.Then(Receive("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<Message>Hello</Message>" +
                  "</trq:TestRequest>")
            .Validate(XmlSupport.Xml()
                .Namespace("trq", "http://agenix.org/schemas/test")
                .SchemaValidation(false))
            .Timeout(TimeSpan.FromSeconds(5).Seconds)
        );

        runner.Given(Echo("Test: Success with multiple namespace validations"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body(
                "<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\" xmlns:msg=\"http://agenix.org/schemas/message\">" +
                "<msg:Message>Hello</msg:Message>" +
                "</trq:TestRequest>")
        );

        runner.Then(Receive("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body(
                "<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\" xmlns:msg=\"http://agenix.org/schemas/message\">" +
                "<msg:Message>Hello</msg:Message>" +
                "</trq:TestRequest>")
            .Validate(XmlSupport.Xml()
                .Namespace("trq", "http://agenix.org/schemas/test")
                .Namespace("msg", "http://agenix.org/schemas/message")
                .SchemaValidation(false))
            .Timeout(TimeSpan.FromSeconds(5).Seconds)
        );

        runner.Given(Echo("Test: Success with multiple nested namespace validations"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<msg:Message xmlns:msg=\"http://agenix.org/schemas/message\">Hello</msg:Message>" +
                  "</trq:TestRequest>")
        );

        runner.Then(Receive("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<msg:Message xmlns:msg=\"http://agenix.org/schemas/message\">Hello</msg:Message>" +
                  "</trq:TestRequest>")
            .Validate(XmlSupport.Xml()
                .Namespace("trq", "http://agenix.org/schemas/test")
                .Namespace("msg", "http://agenix.org/schemas/message")
                .SchemaValidation(false))
            .Timeout(TimeSpan.FromSeconds(5).Seconds)
        );

        runner.Given(Echo("Test: Failure because of missing namespace"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<Message>Hello</Message>" +
                  "</trq:TestRequest>")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("direct:test")
                .Message()
                .Type(MessageType.XML)
                .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                      "<Message>Hello</Message>" +
                      "</trq:TestRequest>")
                .Validate(XmlSupport.Xml()
                    .Namespace("trq", "http://agenix.org/schemas/test")
                    .Namespace("missing", "http://agenix.org/schemas/missing")
                    .SchemaValidation(false))
                .Timeout(TimeSpan.FromSeconds(5).Seconds)
            )
        );

        runner.Given(Echo("Test: Failure because of wrong namespace prefix"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<wrong:TestRequest xmlns:wrong=\"http://agenix.org/schemas/test\">" +
                  "<Message>Hello</Message>" +
                  "</wrong:TestRequest>")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("direct:test")
                .Message()
                .Type(MessageType.XML)
                .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                      "<Message>Hello</Message>" +
                      "</trq:TestRequest>")
                .Validate(XmlSupport.Xml()
                    .Namespace("trq", "http://agenix.org/schemas/test")
                    .SchemaValidation(false))
                .Timeout(TimeSpan.FromSeconds(5).Seconds)
            )
        );

        runner.Given(Echo("Test: Failure because of wrong namespace uri"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/wrong\">" +
                  "<Message>Hello</Message>" +
                  "</trq:TestRequest>")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("direct:test")
                .Message()
                .Type(MessageType.XML)
                .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                      "<Message>Hello</Message>" +
                      "</trq:TestRequest>")
                .Validate(XmlSupport.Xml()
                    .Namespace("trq", "http://agenix.org/schemas/test")
                    .SchemaValidation(false))
                .Timeout(TimeSpan.FromSeconds(5).Seconds)
            )
        );
    }
}
