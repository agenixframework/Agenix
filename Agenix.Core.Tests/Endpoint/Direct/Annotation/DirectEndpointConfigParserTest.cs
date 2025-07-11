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

using Agenix.Api.Annotations;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Endpoint.Direct.Annotation;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Endpoint.Direct.Annotation;

public class DirectEndpointConfigParserTest
{
    private TestContext _context;
    [AgenixEndpoint]
    [DirectEndpointConfig(QueueName = "testQueue")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectEndpoint _directEndpoint1;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixEndpoint]
    [DirectEndpointConfig(Timeout = 10000L,
        Queue = "myQueue")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectEndpoint _directEndpoint2;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixEndpoint]
    [DirectEndpointConfig]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectEndpoint _directEndpoint3;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixEndpoint]
    [DirectEndpointConfig(QueueName = "testQueue")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectEndpoint _directEndpoint4;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    private Mock<IMessageQueue> _myQueue;

    private Mock<IReferenceResolver> _referenceResolver;

    [OneTimeSetUp]
    public void Setup()
    {
        _referenceResolver = new Mock<IReferenceResolver>();
        _myQueue = new Mock<IMessageQueue>();
        _context = new TestContext();

        _referenceResolver
            .Setup(r => r.Resolve<IMessageQueue>("myQueue"))
            .Returns(_myQueue.Object);
    }

    [SetUp]
    public void SetMocks()
    {
        _context.EndpointFactory = new DefaultEndpointFactory();
        _context.SetReferenceResolver(_referenceResolver.Object);
    }

    [Test]
    public void TestDirectEndpointParser()
    {
        AgenixEndpointAnnotations.InjectEndpoints(this, _context);

        // Null checks to ensure endpoints are properly injected
        ClassicAssert.IsNotNull(_directEndpoint1, "directSyncEndpoint1 is null");
        ClassicAssert.IsNotNull(_directEndpoint2, "directSyncEndpoint2 is null");
        ClassicAssert.IsNotNull(_directEndpoint3, "directSyncEndpoint3 is null");
        ClassicAssert.IsNotNull(_directEndpoint4, "directSyncEndpoint4 is null");

        // 1st message receiver
        ClassicAssert.AreEqual("testQueue", _directEndpoint1.EndpointConfiguration.GetQueueName());
        ClassicAssert.IsNull(_directEndpoint1.EndpointConfiguration.GetQueue());
        ClassicAssert.AreEqual(5000L, _directEndpoint1.EndpointConfiguration.Timeout);

        // 2nd message receiver
        ClassicAssert.IsNull(_directEndpoint2.EndpointConfiguration.GetQueueName());
        ClassicAssert.IsNotNull(_directEndpoint2.EndpointConfiguration.GetQueue());
        ClassicAssert.AreEqual(10000L, _directEndpoint2.EndpointConfiguration.Timeout);

        // 3rd message receiver
        ClassicAssert.IsNull(_directEndpoint3.EndpointConfiguration.GetQueueName());
        ClassicAssert.IsNull(_directEndpoint3.EndpointConfiguration.GetQueue());
    }
}
