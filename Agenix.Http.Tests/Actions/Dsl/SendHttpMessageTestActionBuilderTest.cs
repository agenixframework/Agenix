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

using System.Net;
using System.Net.Mime;
using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Http.Actions;
using Agenix.Http.Message;
using Moq;
using NUnit.Framework.Legacy;
using HttpClient = Agenix.Http.Client.HttpClient;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Http.Tests.Actions.Dsl;

public class SendHttpMessageTestActionBuilderTest : AbstractNUnitSetUp
{
    private readonly HttpClient _httpClient = Mock.Of<HttpClient>();
    private readonly IProducer _messageProducer = Mock.Of<IProducer>();


    [Test]
    public void TestFork()
    {
        Mock.Get(_httpClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_httpClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        // Create synchronization primitives for fork testing
        var firstMessageSent = new ManualResetEventSlim(false);
        var secondMessageSent = new ManualResetEventSlim(false);

        // Setup mocks to signal when messages are sent
        Mock.Get(_messageProducer)
            .Setup(m => m.Send(
                It.Is<IMessage>(msg => msg.GetPayload<string>() == "Foo"),
                It.IsAny<TestContext>()))
            .Callback(() => firstMessageSent.Set());

        Mock.Get(_messageProducer)
            .Setup(m => m.Send(
                It.Is<IMessage>(msg => msg.GetPayload<string>() == "Bar"),
                It.IsAny<TestContext>()))
            .Callback(() => secondMessageSent.Set());

        var builder = new DefaultTestCaseRunner(Context);

        // First action (non-forked)
        builder.Run(HttpActionBuilder.Http().Client(_httpClient)
            .Send()
            .Get()
            .Message(new DefaultMessage("Foo").SetHeader("operation", "foo"))
            .Type(MessageType.PLAINTEXT)
            .Header("additional", "additionalValue"));

        // Second action (forked)
        builder.Run(HttpActionBuilder.Http().Client(_httpClient)
            .Send()
            .Post()
            .Message(new DefaultMessage("Bar").SetHeader("operation", "bar"))
            .Type(MessageType.PLAINTEXT)
            .Fork(true));

        // Wait for both messages to be sent (with timeout)
        Assert.That(firstMessageSent.Wait(TimeSpan.FromSeconds(5)), Is.True,
            "First message should be sent within timeout");
        Assert.That(secondMessageSent.Wait(TimeSpan.FromSeconds(5)), Is.True,
            "Second message should be sent within timeout");

        // Verify the calls occurred
        Mock.Get(_messageProducer).Verify(m => m.Send(
            It.Is<IMessage>(msg => msg.GetPayload<string>() == "Foo"),
            It.IsAny<TestContext>()), Times.Once);

        Mock.Get(_messageProducer).Verify(m => m.Send(
            It.Is<IMessage>(msg => msg.GetPayload<string>() == "Bar"),
            It.IsAny<TestContext>()), Times.Once);

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(2, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(SendMessageAction), test.GetActions()[0].GetType());
        ClassicAssert.AreEqual(typeof(SendMessageAction), test.GetActions()[1].GetType());

        var action = (SendMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("http:send-request", action.Name);
        ClassicAssert.AreEqual(_httpClient, action.Endpoint);

        var messageBuilder = (HttpMessageBuilder)action.MessageBuilder;

        ClassicAssert.AreEqual("Foo", messageBuilder.GetMessage().GetPayload<string>());
        ClassicAssert.AreEqual(4, messageBuilder.BuildMessageHeaders(Context).Count);
        ClassicAssert.AreEqual(MessageType.PLAINTEXT.ToString(),
            messageBuilder.BuildMessageHeaders(Context)[MessageHeaders.MessageType]);
        ClassicAssert.AreEqual(HttpMethod.Get.Method,
            messageBuilder.BuildMessageHeaders(Context)[HttpMessageHeaders.HttpRequestMethod]);
        ClassicAssert.AreEqual("foo", messageBuilder.BuildMessageHeaders(Context)["operation"]);
        ClassicAssert.AreEqual("additionalValue", messageBuilder.BuildMessageHeaders(Context)["additional"]);

        ClassicAssert.IsFalse(action.ForkMode);

        action = (SendMessageAction)test.GetActions()[1];
        ClassicAssert.AreEqual("Bar", ((HttpMessageBuilder)action.MessageBuilder).GetMessage().GetPayload<string>());
        ClassicAssert.IsTrue(action.ForkMode);

        // Clean up
        firstMessageSent.Dispose();
        secondMessageSent.Dispose();
    }

    [Test]
    public void TestMessageObjectOverride()
    {
        Mock.Get(_httpClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_httpClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);
        Mock.Get(_messageProducer).Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ClassicAssert.AreEqual("Foo", message.GetPayload<string>());
            });

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(HttpActionBuilder.Http().Client(_httpClient)
            .Send()
            .Get()
            .Message(new HttpMessage("Foo")
                .Cookie(new Cookie("Bar", "987654"))
                .SetHeader("operation", "foo"))
            .Cookie(new Cookie("Foo", "123456"))
            .ContentType(MediaTypeNames.Application.Json)
            .Type(MessageType.PLAINTEXT)
            .Header("additional", "additionalValue"));

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        var action = (SendMessageAction)test.GetActions()[0];

        var messageBuilder = (HttpMessageBuilder)action.MessageBuilder;

        ClassicAssert.AreEqual("Foo", messageBuilder.GetMessage().GetPayload<string>());
        ClassicAssert.AreEqual(7, messageBuilder.BuildMessageHeaders(Context).Count);
        ClassicAssert.AreEqual("foo", messageBuilder.BuildMessageHeaders(Context)["operation"]);
        ClassicAssert.AreEqual("additionalValue", messageBuilder.BuildMessageHeaders(Context)["additional"]);
        ClassicAssert.AreEqual(MediaTypeNames.Application.Json,
            messageBuilder.BuildMessageHeaders(Context)["Content-Type"]);
        ClassicAssert.AreEqual("Bar=987654",
            messageBuilder.BuildMessageHeaders(Context)[HttpMessageHeaders.HttpCookiePrefix + "Bar"]);
        ClassicAssert.AreEqual("Foo=123456",
            messageBuilder.BuildMessageHeaders(Context)[HttpMessageHeaders.HttpCookiePrefix + "Foo"]);
    }

    [Test]
    public void TestHttpMethod()
    {
        Mock.Get(_httpClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_httpClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        Mock.Get(_messageProducer).Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
                    message.GetPayload<string>());
                ClassicAssert.AreEqual(HttpMethod.Get.Method, message.GetHeader(HttpMessageHeaders.HttpRequestMethod));
            });

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(HttpActionBuilder.Http().Client(_httpClient)
            .Send()
            .Get()
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        var action = (SendMessageAction)test.GetActions()[0];

        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((HttpMessageBuilder)action.MessageBuilder).GetMessage().GetPayload<string>());
        ClassicAssert.AreEqual(1, ((HttpMessageBuilder)action.MessageBuilder).BuildMessageHeaders(Context).Count);
    }

    [Test]
    public void TestHttpRequestUriAndPath()
    {
        Mock.Get(_httpClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_httpClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        Mock.Get(_messageProducer).Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
                    message.GetPayload<string>());
                ClassicAssert.AreEqual("http://localhost:8080/",
                    message.GetHeader(IEndpointUriResolver.EndpointUriHeaderName));
                ClassicAssert.AreEqual("/test", message.GetHeader(IEndpointUriResolver.RequestPathHeaderName));
            });

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(HttpActionBuilder.Http().Client(_httpClient)
            .Send()
            .Get("/test")
            .Uri("http://localhost:8080/")
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(SendMessageAction)));

        var action = (SendMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("http:send-request"));
        Assert.That(action.Endpoint, Is.EqualTo(_httpClient));
        Assert.That(action.MessageBuilder.GetType(), Is.EqualTo(typeof(HttpMessageBuilder)));

        var messageBuilder = (HttpMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.GetMessage().GetPayload<string>(),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(4));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[HttpMessageHeaders.HttpRequestMethod],
            Is.EqualTo("GET"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[HttpMessageHeaders.HttpRequestUri],
            Is.EqualTo("http://localhost:8080/"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[IEndpointUriResolver.EndpointUriHeaderName],
            Is.EqualTo("http://localhost:8080/"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[IEndpointUriResolver.RequestPathHeaderName],
            Is.EqualTo("/test"));
    }

    [Test]
    public void TestHttpRequestUriAndQueryParams()
    {
        Mock.Get(_httpClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_httpClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        Mock.Get(_messageProducer).Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
                    message.GetPayload<string>());
                ClassicAssert.AreEqual("http://localhost:8080/",
                    message.GetHeader(IEndpointUriResolver.EndpointUriHeaderName));
                ClassicAssert.AreEqual("param1=value1,param2=value2",
                    message.GetHeader(IEndpointUriResolver.QueryParamHeaderName));
            });

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(HttpActionBuilder.Http().Client(_httpClient)
            .Send()
            .Get()
            .Uri("http://localhost:8080/")
            .QueryParam("param1", "value1")
            .QueryParam("param2", "value2")
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(SendMessageAction)));

        var action = (SendMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("http:send-request"));
        Assert.That(action.Endpoint, Is.EqualTo(_httpClient));
        Assert.That(action.MessageBuilder.GetType(), Is.EqualTo(typeof(HttpMessageBuilder)));

        var messageBuilder = (HttpMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.GetMessage().GetPayload<string>(),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(5));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[HttpMessageHeaders.HttpRequestMethod],
            Is.EqualTo("GET"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[HttpMessageHeaders.HttpRequestUri],
            Is.EqualTo("http://localhost:8080/"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[HttpMessageHeaders.HttpQueryParams],
            Is.EqualTo("param1=value1,param2=value2"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[IEndpointUriResolver.EndpointUriHeaderName],
            Is.EqualTo("http://localhost:8080/"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[IEndpointUriResolver.QueryParamHeaderName],
            Is.EqualTo("param1=value1,param2=value2"));
    }
}
