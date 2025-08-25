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
using Agenix.Api.Spi;
using Agenix.Validation.Json.Message.Builder;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Message.Builder;

public class JsonSerializerHeaderDataBuilderTest : AbstractNUnitSetUp
{
    private readonly JsonSerializer _mapper = new();
    private readonly TestRequest _request = new("Hello Agenix!");
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Mock<IReferenceResolver> _mockReferenceResolver;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [SetUp]
    public void SetUp()
    {
        _mockReferenceResolver = new Mock<IReferenceResolver>();
        Context.SetReferenceResolver(_mockReferenceResolver.Object);
    }

    [Test]
    public async Task ShouldBuildHeaderData()
    {
        // Arrange
        _mockReferenceResolver.Setup(resolver => resolver.ResolveAll<JsonSerializer>())
            .Returns(new ConcurrentDictionary<string, JsonSerializer>(
                new Dictionary<string, JsonSerializer> { { "mapper", _mapper } }
            ));

        _mockReferenceResolver.Setup(resolver => resolver.Resolve<JsonSerializer>())
            .Returns(_mapper);

        var builder = new JsonSerializerHeaderDataBuilder(_request);

        // Act
        var result = await builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("{\"Message\":\"Hello Agenix!\"}"));
    }

    [Test]
    public async Task ShouldBuildHeaderDataWithMapper()
    {
        var builder = new JsonSerializerHeaderDataBuilder(_request, _mapper);

        Assert.That(await builder.BuildHeaderData(Context), Is.EqualTo("{\"Message\":\"Hello Agenix!\"}"));
    }

    [Test]
    public async Task ShouldBuildHeaderDataWithMapperName()
    {
        // Arrange
        _mockReferenceResolver.Setup(resolver => resolver.IsResolvable("mapper")).Returns(true);
        _mockReferenceResolver.Setup(resolver => resolver.Resolve<JsonSerializer>("mapper")).Returns(_mapper);

        Context.SetReferenceResolver(_mockReferenceResolver.Object);

        var builder = new JsonSerializerHeaderDataBuilder(_request, "mapper");

        // Act
        var result = await builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("{\"Message\":\"Hello Agenix!\"}"));
    }

    [Test]
    public async Task ShouldBuildHeaderDataWithVariableSupport()
    {
        Context.SetVariable("message", "Hello Agenix!");
        var builder = new JsonSerializerHeaderDataBuilder(new TestRequest("${message}"), _mapper);

        Assert.That(await builder.BuildHeaderData(Context), Is.EqualTo("{\"Message\":\"Hello Agenix!\"}"));
    }
}
