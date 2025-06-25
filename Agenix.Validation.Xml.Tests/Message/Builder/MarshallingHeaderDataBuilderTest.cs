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
using Agenix.Api.Exceptions;
using Agenix.Api.Spi;
using Agenix.Api.Xml;
using Agenix.Core.Xml;
using Agenix.Validation.Xml.Message.Builder;
using Moq;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Message.Builder;

public class MarshallingHeaderDataBuilderTest : AbstractNUnitSetUp
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
    public void ShouldBuildHeaderData()
    {
        // Arrange
        var marshallerMap = new ConcurrentDictionary<string, IMarshaller> { ["marshaller"] = _marshaller };

        _referenceResolver.Setup(r => r.ResolveAll<IMarshaller>()).Returns(marshallerMap);
        _referenceResolver.Setup(r => r.Resolve<IMarshaller>()).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingHeaderDataBuilder(_request);

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldBuildHeaderDataWithMapper()
    {
        // Arrange
        var builder = new MarshallingHeaderDataBuilder(_request, _marshaller);

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldBuildHeaderDataWithMapperName()
    {
        // Arrange
        _referenceResolver.Setup(r => r.IsResolvable("marshaller")).Returns(true);
        _referenceResolver.Setup(r => r.Resolve<IMarshaller>("marshaller")).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingHeaderDataBuilder(_request, "marshaller");

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldBuildHeaderDataWithVariableSupport()
    {
        // Arrange
        Context.SetVariable("message", "Hello Agenix!");
        var builder = new MarshallingHeaderDataBuilder(new TestRequest("${message}"), _marshaller);

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldThrowExceptionWhenMarshallerNotFound()
    {
        // Arrange
        _referenceResolver.Setup(r => r.IsResolvable("nonExistentMarshaller")).Returns(false);
        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingHeaderDataBuilder(_request, "nonExistentMarshaller");

        // Act & Assert
        Assert.That(() => builder.BuildHeaderData(Context),
            Throws.TypeOf<AgenixSystemException>()
                .With.Message.EqualTo("Unable to find proper object marshaller for name 'nonExistentMarshaller'"));
    }

    [Test]
    public void ShouldThrowExceptionWhenMultipleMarshallersFound()
    {
        // Arrange
        var marshallerMap = new ConcurrentDictionary<string, IMarshaller>
        {
            ["marshaller1"] = _marshaller,
            ["marshaller2"] = new Xml2Marshaller(typeof(TestRequest))
        };

        _referenceResolver.Setup(r => r.ResolveAll<IMarshaller>()).Returns(marshallerMap);
        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingHeaderDataBuilder(_request);

        // Act & Assert
        Assert.That(() => builder.BuildHeaderData(Context),
            Throws.TypeOf<AgenixSystemException>()
                .With.Message.Contains("Unable to auto detect object marshaller")
                .And.Message.Contains("found 2 matching marshaller instances"));
    }

    [Test]
    public void ShouldReturnSuperResultWhenPayloadIsNull()
    {
        // Arrange
        var builder = new MarshallingHeaderDataBuilder(null);

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ShouldReturnSuperResultWhenPayloadIsString()
    {
        // Arrange
        var builder = new MarshallingHeaderDataBuilder("simple string");

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("simple string"));
    }
}
