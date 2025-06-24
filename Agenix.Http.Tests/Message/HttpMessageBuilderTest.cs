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
using Agenix.Api.Message;
using Agenix.Http.Message;
using Moq;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Http.Tests.Message;

public class HttpMessageBuilderTest
{
    private HttpMessage _message;

    [SetUp]
    public void BeforeTest()
    {
        _message = new HttpMessage();
        _message.SetHeader(MessageHeaders.Timestamp, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1);
    }

    [Test]
    public void TestDefaultMessageHeader()
    {
        //GIVEN
        var builder = GetBuilder();

        //WHEN
        var builtMessage = builder.Build(new TestContext(), nameof(MessageType.XML));

        //THEN
        ClassicAssert.AreEqual(3, builtMessage.GetHeaders().Count);
        ClassicAssert.NotNull(_message.GetHeader(MessageHeaders.Id));
        ClassicAssert.NotNull(_message.GetHeader(MessageHeaders.Timestamp));
        ClassicAssert.AreNotEqual(_message.GetHeader(MessageHeaders.Id), builtMessage.GetHeader(MessageHeaders.Id));
        ClassicAssert.AreNotEqual(_message.GetHeader(MessageHeaders.Timestamp),
            builtMessage.GetHeader(MessageHeaders.Timestamp));
        ClassicAssert.AreEqual(nameof(MessageType.XML), builtMessage.GetType());
    }

    [Test]
    public void TestHeaderVariableSubstitution()
    {
        // GIVEN
        var builder = GetBuilder();

        var testContext = new TestContext();
        testContext.SetVariable("testHeader", "foo");
        testContext.SetVariable("testValue", "bar");

        _message.SetHeader("${testHeader}", "${testValue}");

        // WHEN
        var builtMessage = builder.Build(testContext, nameof(MessageType.XML));

        // THEN
        ClassicAssert.AreEqual("bar", builtMessage.GetHeader("foo"));
    }

    [Test]
    public void TestTemplateHeadersArePreserved()
    {
        // GIVEN
        var builder = GetBuilder();
        _message.SetHeader("foo", "bar");

        // WHEN
        var builtMessage = builder.Build(new TestContext(), nameof(MessageType.XML));

        // THEN
        ClassicAssert.AreEqual("bar", builtMessage.GetHeader("foo"));
    }

    [Test]
    public void TestCookieEnricherIsCalledForTemplateCookies()
    {
        // GIVEN
        var cookieEnricher = new CookieEnricher();
        var testContextMock = new TestContext();
        var templateCookie = new Cookie { Name = "foo" };
        _message.SetCookies([templateCookie]);


        var enrichedCookie = cookieEnricher.Enrich([templateCookie], testContextMock);
        var builder = new HttpMessageBuilder(_message, cookieEnricher);

        // WHEN
        var builtMessage = (HttpMessage)builder.Build(testContextMock, nameof(MessageType.XML));

        // THEN
        ClassicAssert.AreEqual(1, builtMessage.GetCookies().Count);
        ClassicAssert.AreEqual(enrichedCookie.First(), builtMessage.GetCookies().First());
    }

    /// Creates and returns an instance of HttpMessageBuilder using a predefined HttpMessage and a mock implementation of CookieEnricher.
    /// <returns>An instance of HttpMessageBuilder initialized with the specified HttpMessage and a mock CookieEnricher.</returns>
    private HttpMessageBuilder GetBuilder()
    {
        var cookieEnricherMock = new Mock<CookieEnricher>();
        return new HttpMessageBuilder(_message, cookieEnricherMock.Object);
    }
}
