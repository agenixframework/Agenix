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
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Actions.TraceVariablesAction.Builder;
using static Agenix.Core.Validation.PathExpressionValidationContext.Builder;

namespace Agenix.Validation.Xml.Tests.Integration;

/// <summary>
///     This test shows how to use test variables.
///     Variables can be defined and referenced throughout the test workflow.
///     They can be used for dynamic content generation, message validation,
///     and storing extracted values from received messages.
/// </summary>
[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class UseVariablesIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void UseVariablesTest()
    {
        // Define test variables with random values
        runner.Given(CreateVariables()
            .Variable("correlationId", "agenix:randomNumber(10)")
            .Variable("messageId", "agenix:randomNumber(10)")
        );

        // Send asynchronous hello request: Agenix -> HelloService
        runner.When(Send("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <HelloRequest>
                      <MessageId>${messageId}</MessageId>
                      <CorrelationId>${correlationId}</CorrelationId>
                      <User>Christoph</User>
                      <Text>Hello Agenix</Text>
                  </HelloRequest>
                  """)
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        // Receive message and extract values into new variables
        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Validate(PathExpression().Xpath("//HelloRequest/CorrelationId", "${correlationId}").Build())
            .Extract(MessageSupport.MessageHeaderSupport.FromHeaders()
                .Header("Operation", "operation")
                .Header("CorrelationId", "id")
            )
            .Extract(XpathSupport.Xpath()
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='User']", "user")
                .AsExtractor())
        );

        // Echo the extracted operation variable
        runner.Then(Echo("${operation}"));

        // Trace all variables to see their current values
        runner.Then(TraceVariables()
            .Variable("id")
            .Variable("correlationId")
            .Variable("operation")
            .Variable("messageId")
            .Variable("user")
        );
    }
}
