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

using Agenix.Api.Spi;
using Agenix.Http.Client;
using Moq;
using NUnit.Framework.Legacy;
using HttpClient = Agenix.Http.Client.HttpClient;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Http.Tests.Client;

public class HttpEndpointComponentTest
{
    private readonly TestContext _context = new();
    private readonly IReferenceResolver _referenceResolver = new Mock<IReferenceResolver>().Object;

    [SetUp]
    public void SetUp()
    {
        _context.SetReferenceResolver(_referenceResolver);
    }

    [Test]
    public void TestCreateClientEndpoint()
    {
        var component = new HttpEndpointComponent();

        var endpoint = component.CreateEndpoint("http://localhost:8088/test", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("http://localhost:8088/test", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Post, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(5000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }

    [Test]
    public void TestCreateClientEndpointWithParameters()
    {
        var component = new HttpEndpointComponent();
        var endpoint =
            component.CreateEndpoint("http://localhost:8088/test?requestMethod=DELETE&customParam=foo", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("http://localhost:8088/test?customParam=foo",
            ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Delete, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(5000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }

    [Test]
    public void TestCreateClientEndpointWithCustomParameters()
    {
        var component = new HttpEndpointComponent();
        var endpoint = component.CreateEndpoint("http://localhost:8088/test?requestMethod=GET&timeout=10000", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("http://localhost:8088/test", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Get, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(10000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }

    [Test]
    public void TestCreateClientHttpsEndpoint()
    {
        var component = new HttpsEndpointComponent();

        var endpoint = component.CreateEndpoint("https://localhost:8088/test", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("https://localhost:8088/test", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Post, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(5000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }

    [Test]
    public void TestCreateClientHttpsEndpointWithParameters()
    {
        var component = new HttpsEndpointComponent();
        var endpoint =
            component.CreateEndpoint("https://localhost:8088/test?requestMethod=DELETE&customParam=foo", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("https://localhost:8088/test?customParam=foo",
            ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Delete, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(5000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }

    [Test]
    public void TestCreateClientHttpsEndpointWithCustomParameters()
    {
        var component = new HttpsEndpointComponent();
        var endpoint =
            component.CreateEndpoint("https://localhost:8088/test?requestMethod=GET&timeout=10000", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("https://localhost:8088/test", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Get, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(10000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }
}
