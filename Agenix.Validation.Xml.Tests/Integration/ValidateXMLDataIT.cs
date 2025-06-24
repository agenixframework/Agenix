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
using Agenix.Api.Message;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Validation.Xml.Dsl;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class ValidateXMLDataIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void ValidateXMLData()
    {
        runner.Given(CreateVariables()
            .Variable("correlationId", "agenix:randomNumber(10)")
            .Variable("messageId", "agenix:randomNumber(10)")
            .Variable("user", "Christoph")
        );

        // Test 1: Basic XML validation with ignore element
        runner.When(Send("direct:hello")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">" +
                  "<MessageId>${messageId}</MessageId>" +
                  "<CorrelationId>${correlationId}</CorrelationId>" +
                  "<User>${user}</User>" +
                  "<Text>Hello ${user}</Text>" +
                  "</HelloRequest>")
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(Receive("direct:hello")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">" +
                  "<MessageId>${messageId}</MessageId>" +
                  "<CorrelationId>${correlationId}</CorrelationId>" +
                  "<User>${user}</User>" +
                  "<Text>xxx</Text>" +
                  "</HelloRequest>")
            .Validate(XmlSupport.Xml()
                .Ignore("HelloRequest.Text"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        // Test 2: XML validation with namespace context and XPath ignore
        runner.When(Send("direct:hello")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">" +
                  "<MessageId>${messageId}</MessageId>" +
                  "<CorrelationId>${correlationId}</CorrelationId>" +
                  "<User>${user}</User>" +
                  "<Text>Hello ${user}</Text>" +
                  "</HelloRequest>")
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(Receive("direct:hello")
            .Message()
            .Type(MessageType.XML)
            .Body("<ns0:HelloRequest xmlns:ns0=\"http://agenix.org/schemas/samples/HelloService.xsd\">" +
                  "<ns0:MessageId>${messageId}</ns0:MessageId>" +
                  "<ns0:CorrelationId>${correlationId}</ns0:CorrelationId>" +
                  "<ns0:User>${user}</ns0:User>" +
                  "<ns0:Text>xxx</ns0:Text>" +
                  "</ns0:HelloRequest>")
            .Validate(XmlSupport.Xml()
                .NamespaceContext("ns", "http://agenix.org/schemas/samples/HelloService.xsd")
                .Ignore("//ns:HelloRequest/ns:Text"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );
    }
}
