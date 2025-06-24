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

using System.Collections.Concurrent;
using System.Collections.Generic;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Endpoint;

public class DefaultEndpointFactoryTest
{
    private Mock<IReferenceResolver> _referenceResolver;

    [SetUp]
    public void SetUp()
    {
        _referenceResolver = new Mock<IReferenceResolver>();
    }

    [Test]
    public void TestResolveDirectEndpoint()
    {
        // Arrange
        _referenceResolver.Setup(r => r.Resolve<IEndpoint>("myEndpoint")).Returns(new Mock<IEndpoint>().Object);
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);
        var factory = new DefaultEndpointFactory();

        // Act
        var endpoint = factory.Create("myEndpoint", context);

        // Assert
        ClassicAssert.IsNotNull(endpoint);
    }

    [Test]
    public void TestResolveCustomEndpoint()
    {
        // Arrange
        var components = new ConcurrentDictionary<string, IEndpointComponent>();
        components.TryAdd("custom", new DirectEndpointComponent());

        _referenceResolver.Setup(r => r.ResolveAll<IEndpointComponent>()).Returns(components);
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);

        var factory = new DefaultEndpointFactory();

        // Act
        var endpoint = factory.Create("custom:custom.queue", context);

        // Assert
        ClassicAssert.IsInstanceOf<DirectEndpoint>(endpoint);
        ClassicAssert.AreEqual("custom.queue", ((DirectEndpoint)endpoint).EndpointConfiguration.GetQueueName());
    }

    [Test]
    public void TestOverwriteEndpointComponent()
    {
        // Arrange
        var components = new ConcurrentDictionary<string, IEndpointComponent>();
        components.TryAdd("jms", new DirectEndpointComponent());


        _referenceResolver.Setup(r => r.ResolveAll<IEndpointComponent>()).Returns(components);
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);

        var factory = new DefaultEndpointFactory();

        // Act
        var endpoint = factory.Create("jms:custom.queue", context);

        // Assert
        ClassicAssert.IsInstanceOf<DirectEndpoint>(endpoint);
        ClassicAssert.AreEqual("custom.queue", ((DirectEndpoint)endpoint).EndpointConfiguration.GetQueueName());
    }

    [Test]
    public void TestResolveUnknownEndpointComponent()
    {
        // Arrange
        _referenceResolver.Setup(r => r.ResolveAll<IEndpointComponent>())
            .Returns(new ConcurrentDictionary<string, IEndpointComponent>());
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);

        var factory = new DefaultEndpointFactory();

        // Act & Assert
        var ex = Assert.Throws<AgenixSystemException>(() => factory.Create("unknown:unknown", context));

        Assert.That(ex, Is.Not.Null);
        ClassicAssert.IsTrue(ex.Message.StartsWith("Unable to create endpoint component"));
    }

    [Test]
    public void TestResolveInvalidEndpointUri()
    {
        // Arrange
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);
        var factory = new DefaultEndpointFactory();

        // Act & Assert
        var ex = Assert.Throws<AgenixSystemException>(() => factory.Create("jms:", context));
        Assert.That(ex, Is.Not.Null);
        ClassicAssert.IsTrue(ex.Message.StartsWith("Invalid endpoint uri"));
    }
}
