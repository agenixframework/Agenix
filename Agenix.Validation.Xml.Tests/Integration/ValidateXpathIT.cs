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
using Agenix.Api.Xml.Namespace;
using Agenix.Core.Actions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Validation.Xml.Dsl;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class ValidateXpathIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void ValidateXpath()
    {
        runner.Given(CreateVariables()
            .Variable("correlationId", "agenix:randomNumber(10)")
            .Variable("messageId", "agenix:randomNumber(10)")
            .Variable("user", "Agenix")
        );

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
            .Validate(XmlSupport.Xml().XPath()
                .Expression("//ns0:HelloRequest/ns0:MessageId", "${messageId}")
                .Expression("//ns0:HelloRequest/ns0:CorrelationId", "${correlationId}")
                .Expression("//ns0:HelloRequest/ns0:Text", "Hello ${user}")
                .NamespaceContext("ns0", "http://agenix.org/schemas/samples/HelloService.xsd"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

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
            .Validate(XmlSupport.Xml().XPath()
                .Expression("//ns1:HelloRequest/ns1:MessageId", "${messageId}")
                .Expression("//ns1:HelloRequest/ns1:CorrelationId", "${correlationId}")
                .Expression("//ns1:HelloRequest/ns1:Text", "Hello ${user}")
                .NamespaceContext("ns1", "http://agenix.org/schemas/samples/HelloService.xsd"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

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
            .Validate(XpathSupport.Xpath()
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='MessageId']", "${messageId}")
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='CorrelationId']", "${correlationId}")
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='Text']", "Hello ${user}")
                .AsValidationContext())
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Given(Echo("Now using xpath validation elements"));

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
            .Validate(XmlSupport.Xml()
                .XPath()
                .Expression("//ns0:HelloRequest/ns0:MessageId", "${messageId}")
                .Expression("//ns0:HelloRequest/ns0:CorrelationId", "${correlationId}")
                .Expression("//ns0:HelloRequest/ns0:Text", "Hello ${user}")
                .NamespaceContext("ns0", "http://agenix.org/schemas/samples/HelloService.xsd"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

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
            .Validate(XmlSupport.Xml()
                .XPath()
                .Expression("//ns1:HelloRequest/ns1:MessageId", "${messageId}")
                .Expression("//ns1:HelloRequest/ns1:CorrelationId", "${correlationId}")
                .Expression("//ns1:HelloRequest/ns1:Text", "Hello ${user}")
                .NamespaceContext("ns1", "http://agenix.org/schemas/samples/HelloService.xsd"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

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
            .Validate(XpathSupport.Xpath()
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='MessageId']", "${messageId}")
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='CorrelationId']", "${correlationId}")
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='Text']", "Hello ${user}")
                .AsValidationContext())
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Given(Echo("Test: Default namespace mapping"));

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
            .Validate(XmlSupport.Xml()
                .XPath()
                .Expression("//test:HelloRequest/def:MessageId", "${messageId}")
                .Expression("//test:HelloRequest/def:CorrelationId", "${correlationId}")
                .Expression("//test:HelloRequest/def:Text", "Hello ${user}")
                .NamespaceContext(TestNamespaceContextBuilder().NamespaceMappings))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
            .Extract(XpathSupport.Xpath()
                .Expression("//def:HelloRequest/def:Text", "extractedText"))
        );

        runner.Then(DefaultTestActionBuilder.Action(ctx =>
        {
            Assert.That(ctx.GetVariable("extractedText"),
                Is.EqualTo(ctx.ReplaceDynamicContentInString("Hello ${user}")));
        }));
    }

    private NamespaceContextBuilder TestNamespaceContextBuilder()
    {
        var builder = new NamespaceContextBuilder();
        builder.NamespaceMappings.Add("test", "http://agenix.org/schemas/samples/HelloService.xsd");
        return builder;
    }

    [Test]
    public void ShouldFailOnMultipleXpathExpressionValidation()
    {
        runner.Given(CreateVariables()
            .Variable("correlationId", "agenix:randomNumber(10)")
            .Variable("messageId", "agenix:randomNumber(10)")
            .Variable("user", "Agenix")
        );

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

        Assert.Throws<TestCaseFailedException>(() =>
        {
            runner.Then(Receive("direct:hello")
                .Message()
                .Type(MessageType.XML)
                .Validate(XpathSupport.Xpath()
                    .Expression("//*[local-name()='HelloRequest']/*[local-name()='MessageId']", "${messageId}")
                    .AsValidationContext())
                .Validate(XpathSupport.Xpath()
                    .Expression("//*[local-name()='HelloRequest']/*[local-name()='CorrelationId']", "${correlationId}")
                    .AsValidationContext())
                .Validate(XpathSupport.Xpath()
                    .Expression("//*[local-name()='HelloRequest']/*[local-name()='Text']", "should fail")
                    .AsValidationContext())
                .Header("Operation", "sayHello")
                .Header("CorrelationId", "${correlationId}")
            );
        });
    }
}
