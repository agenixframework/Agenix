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

using Agenix.Api.Exceptions;
using Agenix.Api.Spi;
using Agenix.Api.Xml;
using Agenix.Core.Xml;
using Agenix.Validation.Xml.Message.Builder;
using Moq;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Message.Builder;

public class MarshallingPayloadBuilderTest : AbstractNUnitSetUp
{
    private readonly Xml2Marshaller _marshaller = new(typeof(TestRequest));
    private readonly Mock<IReferenceResolver> _referenceResolver = new();
    private readonly TestRequest _request = new("Hello Agenix!");

    [SetUp]
    public void SetUp()
    {
        _marshaller.SetProperty("omitxmldeclaration", "true");
        _marshaller.SetProperty("stripnamespaces", "true");
        _marshaller.SetProperty("removeemptylines", "true");
    }

    [Test]
    public void ShouldBuildPayload()
    {
        // Arrange
        var marshallerMap = new Dictionary<string, IMarshaller> { ["marshaller"] = _marshaller };

        _referenceResolver.Setup(r => r.ResolveAll<IMarshaller>()).Returns(marshallerMap);
        _referenceResolver.Setup(r => r.Resolve<IMarshaller>()).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingPayloadBuilder(_request);

        // Act
        var result = builder.BuildPayload(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldBuildPayloadWithMapper()
    {
        // Arrange
        var builder = new MarshallingPayloadBuilder(_request, _marshaller);

        // Act
        var result = builder.BuildPayload(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldBuildPayloadWithMapperName()
    {
        // Arrange
        _referenceResolver.Setup(r => r.IsResolvable("marshaller")).Returns(true);
        _referenceResolver.Setup(r => r.Resolve<IMarshaller>("marshaller")).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingPayloadBuilder(_request, "marshaller");

        // Act
        var result = builder.BuildPayload(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldBuildPayloadWithVariableSupport()
    {
        // Arrange
        Context.SetVariable("message", "Hello Agenix!");
        var builder = new MarshallingPayloadBuilder(new TestRequest("${message}"), _marshaller);

        // Act
        var result = builder.BuildPayload(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldThrowExceptionWhenMarshallerNotFound()
    {
        // Arrange
        _referenceResolver.Setup(r => r.IsResolvable("nonExistentMarshaller")).Returns(false);
        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingPayloadBuilder(_request, "nonExistentMarshaller");

        // Act & Assert
        Assert.That(() => builder.BuildPayload(Context),
            Throws.TypeOf<AgenixSystemException>()
                .With.Message.EqualTo("Unable to find proper object marshaller for name 'nonExistentMarshaller'"));
    }

    [Test]
    public void ShouldThrowExceptionWhenMultipleMarshallersFound()
    {
        // Arrange
        var marshallerMap = new Dictionary<string, IMarshaller>
        {
            ["marshaller1"] = _marshaller,
            ["marshaller2"] = new Xml2Marshaller(typeof(TestRequest))
        };

        _referenceResolver.Setup(r => r.ResolveAll<IMarshaller>()).Returns(marshallerMap);
        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingPayloadBuilder(_request);

        // Act & Assert
        Assert.That(() => builder.BuildPayload(Context),
            Throws.TypeOf<AgenixSystemException>()
                .With.Message.Contains("Unable to auto detect object marshaller")
                .And.Message.Contains("found 2 matching marshaller instances"));
    }

    [Test]
    public void ShouldReturnSuperResultWhenPayloadIsNull()
    {
        // Arrange
        var builder = new MarshallingPayloadBuilder(null);

        // Act
        var result = builder.BuildPayload(Context);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ShouldReturnSuperResultWhenPayloadIsString()
    {
        // Arrange
        var builder = new MarshallingPayloadBuilder("simple string");

        // Act
        var result = builder.BuildPayload(Context);

        // Assert
        Assert.That(result, Is.EqualTo("simple string"));
    }

    [Test]
    public void ShouldBuildPayloadWithComplexValidation()
    {
        // Arrange
        var builder = new MarshallingPayloadBuilder(_request, _marshaller);

        // Act
        var result = builder.BuildPayload(Context);

        // Assert - Multiple constraints with Assert.That
        Assert.That(result, Is.Not.Null
            .And.Not.Empty
            .And.TypeOf<string>()
            .And.StartsWith("<TestRequest>")
            .And.EndsWith("</TestRequest>")
            .And.Contains("Hello Agenix!"));
    }

    [Test]
    public void ShouldVerifyMarshallerInteractions()
    {
        _referenceResolver.Reset();
        // Arrange
        var marshallerMap = new Dictionary<string, IMarshaller> { ["marshaller"] = _marshaller };

        _referenceResolver.Setup(r => r.ResolveAll<IMarshaller>()).Returns(marshallerMap);
        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingPayloadBuilder(_request);

        // Act
        var result = builder.BuildPayload(Context);

        // Assert - Verify mock interactions with Assert.That
        _referenceResolver.Verify(r => r.ResolveAll<IMarshaller>(), Times.Once);

        Assert.That(result, Is.Not.Null);
        Assert.That(_referenceResolver.Invocations.Count, Is.EqualTo(1));
    }
}
