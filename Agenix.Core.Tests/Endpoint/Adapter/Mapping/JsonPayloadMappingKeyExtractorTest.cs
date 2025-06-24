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
using Agenix.Core.Message;
using Agenix.Validation.Json.Endpoint.Adapter.Mapping;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Endpoint.Adapter.Mapping;

/// <summary>
///     Unit tests for the <see cref="JsonPayloadMappingKeyExtractor" /> class.
/// </summary>
public class JsonPayloadMappingKeyExtractorTest
{
    [Test]
    public void TestExtractMappingKey()
    {
        var extractor = new JsonPayloadMappingKeyExtractor();
        extractor.SetJsonPathExpression("$.person.name");

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"person\": {\"name\": \"Penny\"} }")), "Penny");

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"person\": {\"name\": \"Leonard\"} }")), "Leonard");
    }

    [Test]
    public void TestExtractMappingKeyWithoutJsonPathExpressionSet()
    {
        var extractor = new JsonPayloadMappingKeyExtractor();

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"person\": {\"name\": \"Penny\"} }")), "person");

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"animal\": {\"name\": \"Sheldon\"} }")), "animal");
    }

    [Test]
    public void TestRouteMessageWithBadJsonPathExpression()
    {
        var extractor = new JsonPayloadMappingKeyExtractor();

        extractor.SetJsonPathExpression("$.I_DO_NOT_EXIST");

        Assert.Throws<AgenixSystemException>(() => extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"person\": {\"name\": \"Penny\"} }")));
    }
}
