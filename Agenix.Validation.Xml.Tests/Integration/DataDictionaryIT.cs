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
using Agenix.Core.Dsl;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Validation.Xml.Dsl;
using Agenix.Validation.Xml.Variable.Dictionary.Xml;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Actions.TraceVariablesAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class DataDictionaryIT
{
    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint _helloEndpoint;

    [AgenixResource] protected TestContext context;

    [AgenixResource] private NodeMappingDataDictionary helloServiceDataDictionary;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void DataDictionaryTest()
    {
        runner.Given(CreateVariables().Variable("user", "Agenix"));
        runner.Given(CreateVariables().Variable("correlationId", "agenix:RandomNumber(10)"));
        runner.Given(CreateVariables().Variable("messageId", "agenix:RandomNumber(10)"));

        runner.When(Send("hello.endpoint")
            .Name("Send asynchronous hello request: Agenix -> HelloService")
            .Message()
            .Type(MessageType.XML)
            .Dictionary(HelloServiceDataDictionary())
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">\n" +
                  "    <MessageId>0</MessageId>\n" +
                  "    <CorrelationId>0</CorrelationId>\n" +
                  "    <User></User>\n" +
                  "    <Text></Text>\n" +
                  "</HelloRequest>")
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Dictionary(HelloServiceDataDictionary())
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">\n" +
                  "    <MessageId>0</MessageId>\n" +
                  "    <CorrelationId>0</CorrelationId>\n" +
                  "    <User></User>\n" +
                  "    <Text></Text>\n" +
                  "</HelloRequest>")
            .Extract(MessageSupport.Message().Headers()
                .Header("Operation", "operation")
                .Header("CorrelationId", "id"))
            .Extract(XmlSupport.Xml()
                .Expression("//:HelloRequest/:User", "user"))
        );

        runner.Then(Echo("${operation}"));
        runner.Then(TraceVariables("id", "correlationId", "operation", "messageId", "user"));
    }

    public NodeMappingDataDictionary HelloServiceDataDictionary()
    {
        var dict = new NodeMappingDataDictionary { IsGlobalScope = false };
        dict.Mappings.Add("HelloRequest.MessageId", "${messageId}");
        dict.Mappings.Add("HelloRequest.CorrelationId", "${correlationId}");
        dict.Mappings.Add("HelloRequest.User", "Agenix");
        dict.Mappings.Add("HelloRequest.Text", "Hello ${user}");
        return dict;
    }
}
