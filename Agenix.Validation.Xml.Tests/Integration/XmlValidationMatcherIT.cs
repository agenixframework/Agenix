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
///     Test demonstrates XML validation using validation matchers within CDATA sections.
/// </summary>
[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class XmlValidationMatcherIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void XmlValidationMatcherTest()
    {
        runner.Given(CreateVariables()
            .Variable("greetingText", "Hello Agenix")
        );

        const string sendData = """
                                <data>
                                  <greeting>Hello Agenix</greeting>
                                  <timestamp>2012-07-01T00:00:00</timestamp>
                                </data>
                                """;

        const string expectedData = """
                                    <data>
                                      <greeting>${greetingText}</greeting>
                                      <timestamp>@Ignore@</timestamp>
                                    </data>
                                    """;

        runner.When(Send("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body($"<testRequestMessage><text>agenix:CdataSection('{sendData}')</text></testRequestMessage>")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body(
                $"<testRequestMessage><text>agenix:CdataSection('@matchesXml('{expectedData}')@')</text></testRequestMessage>")
        );
    }
}
