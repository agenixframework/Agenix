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
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Validation.Xml.Dsl;
using NUnit.Framework;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;

namespace Agenix.Validation.Xml.Tests.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class HamcrestValidationMatcherIT
{
    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint _helloEndpoint;

    [AgenixResource] protected TestContext context;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    [Description("Tests the @AssertThat()@ validator")]
    [Author("asuruceanu")]
    public void HamcrestValidationMatcherTest()
    {
        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<HelloMessage>" +
                  "  <message>Hello foo!</message>" +
                  "</HelloMessage>"));

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Body("<HelloMessage>" +
                  "  <message>@AssertThat(Not(EqualTo(bar)))@</message>" +
                  "</HelloMessage>")
            .Validate(XmlSupport.Xml()
                .Expression("/HelloMessage/message", "@AssertThat(ContainsString(foo!))@")
                .Expression("//message", "@AssertThat(HasSize(1))@")));
    }
}
