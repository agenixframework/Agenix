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
///     This test demonstrates XPath segment variable extraction functionality.
///     It shows how to extract values from XML stored in variables using XPath expressions
///     and send the extracted data with a different payload structure.
/// </summary>
[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class XpathSegmentVariableExtractorIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    [Description("Extract value from a xml stored as variable and send it with a different payload.")]
    public void XpathSegmentVariableExtractorTest()
    {
        // Define variable containing XML data
        runner.Given(CreateVariables()
            .Variable("xmlVar", "<Person><Name>Peter</Name></Person>")
        );

        // Send a message using XPath extraction from a variable
        runner.When(Send("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("<OtherPerson><Name>${xmlVar.xpath(//Person/Name)}</Name></OtherPerson>")
        );

        // Receive and validate the transformed message
        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("<OtherPerson><Name>Peter</Name></OtherPerson>")
        );
    }
}
