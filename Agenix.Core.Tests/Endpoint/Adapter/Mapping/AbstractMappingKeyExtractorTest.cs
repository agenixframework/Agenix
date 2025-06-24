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

using Agenix.Api.Message;
using Agenix.Core.Endpoint.Adapter.Mapping;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Endpoint.Adapter.Mapping;

/// <summary>
///     Unit tests for the AbstractMappingKeyExtractor class.
/// </summary>
public class AbstractMappingKeyExtractorTest
{
    [Test]
    public void TestMappingKeyPrefixSuffix()
    {
        var mappingKeyExtractor = new CustomMappingKeyExtractor();

        mappingKeyExtractor.SetMappingKeyPrefix("pre_");
        ClassicAssert.AreEqual("pre_key", mappingKeyExtractor.ExtractMappingKey(new DefaultMessage("")));

        mappingKeyExtractor.SetMappingKeySuffix("_end");
        ClassicAssert.AreEqual("pre_key_end", mappingKeyExtractor.ExtractMappingKey(new DefaultMessage("")));

        mappingKeyExtractor.SetMappingKeyPrefix("");
        ClassicAssert.AreEqual("key_end", mappingKeyExtractor.ExtractMappingKey(new DefaultMessage("")));
    }

    private class CustomMappingKeyExtractor : AbstractMappingKeyExtractor
    {
        protected override string GetMappingKey(IMessage request)
        {
            return "key";
        }
    }
}
