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

using Agenix.Api.Endpoint;
using Agenix.Core.Endpoint.Direct;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Endpoint.Direct;

public class DirectEndpointComponentTest
{
    private TestContext _context;

    [SetUp]
    public void SetupMock()
    {
        _context = TestContextFactory.NewInstance().GetObject();
    }

    [Test]
    public void TestCreateDirectEndpoint()
    {
        var component = new DirectEndpointComponent();

        ClassicAssert.False(_context.ReferenceResolver.IsResolvable("queueName"));
        var endpoint = component.CreateEndpoint("direct:queueName", _context);

        ClassicAssert.IsInstanceOf<DirectEndpoint>(endpoint);

        var directEndpoint = (DirectEndpoint)endpoint;

        ClassicAssert.AreEqual("queueName", directEndpoint.EndpointConfiguration.GetQueueName());
        ClassicAssert.AreEqual(5000L, directEndpoint.EndpointConfiguration.Timeout);
        ClassicAssert.True(_context.ReferenceResolver.IsResolvable("queueName"));
    }

    [Test]
    public void TestCreateSyncDirectEndpoint()
    {
        var component = new DirectEndpointComponent();

        ClassicAssert.False(_context.ReferenceResolver.IsResolvable("queueName"));
        var endpoint = component.CreateEndpoint("direct:sync:queueName", _context);

        ClassicAssert.IsInstanceOf<DirectSyncEndpoint>(endpoint);

        var directSyncEndpoint = (DirectSyncEndpoint)endpoint;

        ClassicAssert.AreEqual("queueName", directSyncEndpoint.EndpointConfiguration.GetQueueName());
        ClassicAssert.True(_context.ReferenceResolver.IsResolvable("queueName"));
    }

    [Test]
    public void TestCreateDirectEndpointWithParameters()
    {
        var component = new DirectEndpointComponent();

        var endpoint = component.CreateEndpoint("direct:queueName?timeout=10000", _context);

        ClassicAssert.IsInstanceOf<DirectEndpoint>(endpoint);

        var directEndpoint = (DirectEndpoint)endpoint;

        ClassicAssert.AreEqual("queueName", directEndpoint.EndpointConfiguration.GetQueueName());
        ClassicAssert.AreEqual(10000L, directEndpoint.EndpointConfiguration.Timeout);
    }

    [Test]
    public void TestLookupAll()
    {
        var validators = IEndpointComponent.Lookup();
        ClassicAssert.AreEqual(1, validators.Count);
        ClassicAssert.IsNotNull(validators["direct"]);
        ClassicAssert.AreEqual(typeof(DirectEndpointComponent), validators["direct"].GetType());
    }

    [Test]
    public void TestLookupByQualifier()
    {
        ClassicAssert.IsTrue(IEndpointComponent.Lookup("direct").IsPresent);
    }
}
