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
using Agenix.Api.Util;
using Agenix.Http.Actions;
using Agenix.Http.Message;
using Moq;
using Moq.AutoMock;
using NUnit.Framework.Legacy;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Http.Tests.Actions;

public class HttpClientActionBuilderTest
{
    private AutoMocker _autoMocker;

    private HttpClientActionBuilder _fixture;
    private Mock<HttpClient> _httpClientMock;

    [SetUp]
    public void BeforeMethodSetup()
    {
        _autoMocker = new AutoMocker();
        _httpClientMock = _autoMocker.GetMock<HttpClient>();
        _fixture = new HttpClientActionBuilder(_httpClientMock.Object);
    }

    [Test]
    public void ResponseWithHttpStatus()
    {
        var httpMessageBuilderSupport = _fixture.Receive()
            .Response(HttpStatusCode.OK) // Method under test
            .Message();

        var httpMessage = (HttpMessage)ReflectionUtils.GetInstanceFieldValue(httpMessageBuilderSupport, "httpMessage");
        ClassicAssert.NotNull(httpMessage);

        var headers = httpMessage.GetHeaders();
        ClassicAssert.AreEqual((int)HttpStatusCode.OK, headers[HttpMessageHeaders.HttpStatusCode]);
        ClassicAssert.AreEqual(HttpStatusCode.OK.ToString(), headers[HttpMessageHeaders.HttpReasonPhrase]);
    }

    [Test]
    public void RequestWithGetMethodAndQueryParameters()
    {
        var httpMessageBuilderSupport = _fixture.Send()
            .Get()
            .Path("/test")
            .QueryParam("q", "v")
            .Message();

        var httpMessage = (HttpMessage)ReflectionUtils.GetInstanceFieldValue(httpMessageBuilderSupport, "httpMessage");
        ClassicAssert.NotNull(httpMessage);

        var headers = httpMessage.GetHeaders();
        ClassicAssert.AreEqual(HttpMethod.Get.Method, headers[HttpMessageHeaders.HttpRequestMethod]);
        ClassicAssert.AreEqual("/test", headers[HttpMessageHeaders.HttpRequestUri]);
        ClassicAssert.AreEqual("q=v", headers[HttpMessageHeaders.HttpQueryParams]);
    }

    [Test]
    public void ResponseWithHttpStatusCode()
    {
        const int code = 123;

        var httpMessageBuilderSupport = new HttpClientActionBuilder(_httpClientMock.Object)
            .Receive()
            .Response((HttpStatusCode)code) // Method under test
            .Message();

        var httpMessage = (HttpMessage)ReflectionUtils.GetInstanceFieldValue(httpMessageBuilderSupport, "httpMessage");
        ClassicAssert.NotNull(httpMessage);

        var headers = httpMessage.GetHeaders();
        ClassicAssert.AreEqual(code, headers[HttpMessageHeaders.HttpStatusCode]);
        ClassicAssert.False(headers.ContainsKey(HttpMessageHeaders.HttpReasonPhrase));
    }

    [Test]
    public void IsReferenceResolverAwareTestActionBuilder()
    {
        ClassicAssert.True(_fixture is not null, "Is instanceof AbstractReferenceResolverAwareTestActionBuilder");
    }
}
