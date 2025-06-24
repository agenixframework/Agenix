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
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;

namespace Agenix.Validation.Xml.Tests.Integration;

/// <summary>
///     This test shows the usage of inline test variables. In xml data definition you can
///     use the escape sequence ${variable_name} to add variable values to the xml template.
///     The parameter "variable_name" will be the name of a valid test variable or a test function.
/// </summary>
[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class InlineVariablesIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void InlineVariablesTest()
    {
        // Define test variables
        runner.Given(CreateVariables()
            .Variable("text", "Hallo")
            .Variable("text2", "Test Framework")
        );

        // First send/receive cycle with dynamic content
        runner.When(Send("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <xml>
                    <message>${text}</message>
                    <message>${text2}</message>
                    <message>agenix:concat(${text}, ' Test', ' Framework!')</message>
                    <message>agenix:upperCase('klein')</message>
                    <message>Text is: agenix:lowerCase('GROSS')</message>
                    <message>${text} ${text2}</message>
                  </xml>
                  """)
            .Header("operation", "Greetings")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <xml>
                    <message>Hallo</message>
                    <message>Test Framework</message>
                    <message>Hallo Test Framework!</message>
                    <message>KLEIN</message>
                    <message>Text is: gross</message>
                    <message>Hallo Test Framework</message>
                  </xml>
                  """)
            .Header("operation", "Greetings")
        );

        // Second send/receive cycle with resolved values
        runner.When(Send("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <xml>
                    <message>Hallo</message>
                    <message>Test Framework</message>
                    <message>Hallo Test Framework!</message>
                    <message>KLEIN</message>
                    <message>Text is: gross</message>
                    <message>Hallo Test Framework</message>
                  </xml>
                  """)
            .Header("operation", "Greetings")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <xml>
                    <message>${text}</message>
                    <message>${text2}</message>
                    <message>agenix:concat(${text}, ' Test', ' Framework!')</message>
                    <message>agenix:upperCase('klein')</message>
                    <message>Text is: agenix:lowerCase('GROSS')</message>
                    <message>${text} ${text2}</message>
                  </xml>
                  """)
            .Header("operation", "Greetings")
        );
    }
}
