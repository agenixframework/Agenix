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
using Agenix.Core.Endpoint.Adapter.Mapping;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Endpoint.Adapter.Mapping;

/// <summary>
///     Unit tests for the HeaderMappingKeyExtractor class.
/// </summary>
public class HeaderMappingKeyExtractorTest
{
    [Test]
    public void TestExtractMappingKey()
    {
        var extractor = new HeaderMappingKeyExtractor();
        extractor.SetHeaderName("Foo");

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage("Foo")
            .SetHeader("Foo", "foo")
            .SetHeader("Bar", "bar")), "foo");
    }

    [Test]
    public void TestExtractMappingKeyWithoutHeaderNameSet()
    {
        var extractor = new HeaderMappingKeyExtractor();

        Assert.Throws<AgenixSystemException>(() => extractor.ExtractMappingKey(new DefaultMessage("Foo")
            .SetHeader("Foo", "foo")
            .SetHeader("Bar", "bar")));
    }

    [Test]
    public void TestExtractMappingKeyWithUnknownHeaderName()
    {
        var extractor = new HeaderMappingKeyExtractor("UNKNOWN");

        Assert.Throws<AgenixSystemException>(() => extractor.ExtractMappingKey(new DefaultMessage("Foo")
            .SetHeader("Foo", "foo")
            .SetHeader("Bar", "bar")));
    }
}
