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

using Agenix.Api.Message;
using Agenix.Api.Validation.Context;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Validation.Xml;
using Agenix.Http.Actions;
using Agenix.Http.Client;
using Agenix.Http.Message;
using Moq;
using NUnit.Framework.Legacy;
using HttpClient = Agenix.Http.Client.HttpClient;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Http.Tests.Actions.Dsl;

public class ReceiveHttpMessageTestActionBuilderTest : AbstractNUnitSetUp
{
    private readonly Mock<HttpEndpointConfiguration> _configuration = new();
    private readonly Mock<HttpClient> _httpClient = new();

    [Test]
    public void TestHttpRequestProperties()
    {
        Mock.Get(_httpClient.Object).Reset();
        Mock.Get(_configuration.Object).Reset();

        Mock.Get(_configuration.Object).Setup(x => x.Timeout).Returns(100L);
        _httpClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _httpClient.Setup(m => m.CreateConsumer()).Returns(_httpClient.Object);
        Mock.Get(_httpClient.Object).Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(
            new HttpMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .Method(HttpMethod.Get)
                .Path("/test/foo")
                .QueryParam("noValue")
                .QueryParam("param1", "value1")
                .QueryParam("param2", "value2"));

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(HttpActionBuilder.Http().Client(_httpClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Type(MessageType.XML)
        );

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(test.GetActionCount(), 1);
        ClassicAssert.AreEqual(test.GetActions()[0].GetType(), typeof(ReceiveMessageAction));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual(action.Name, "http:receive-response");

        ClassicAssert.AreEqual(action.ValidationContexts.Count, 2L);
        ClassicAssert.AreEqual(action.ValidationContexts[0].GetType(), typeof(HeaderValidationContext));
        ClassicAssert.AreEqual(action.ValidationContexts[1].GetType(), typeof(XmlMessageValidationContext));

        var messageBuilder = (HttpMessageBuilder)action.MessageBuilder;
        ClassicAssert.AreEqual(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            "<TestRequest><Message>Hello World!</Message></TestRequest>");
    }
}
