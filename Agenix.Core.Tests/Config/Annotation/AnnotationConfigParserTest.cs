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

using System;
using Agenix.Api.Config.Annotation;
using Agenix.Api.Endpoint;
using Agenix.Core.Endpoint.Direct.Annotation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Config.Annotation;

/// <summary>
///     Unit tests for the AnnotationConfigParser implementations.
/// </summary>
public class AnnotationConfigParserTest
{
    [Test]
    public void TestLookup()
    {
        var parsers = IAnnotationConfigParser<Attribute, IEndpoint>.Lookup();
        ClassicAssert.AreEqual(parsers.Count, 2L);
        ClassicAssert.IsTrue(parsers.ContainsKey("direct.sync"));
        ClassicAssert.IsTrue(parsers.ContainsKey("direct.async"));
    }

    [Test]
    public void ShouldLookupParser()
    {
        ClassicAssert.IsTrue(IAnnotationConfigParser<Attribute, IEndpoint>.Lookup("direct.sync").IsPresent);
        ClassicAssert.AreEqual(IAnnotationConfigParser<Attribute, IEndpoint>.Lookup("direct.sync").Value.GetType(),
            typeof(DirectSyncEndpointConfigParser));
        ClassicAssert.IsTrue(IAnnotationConfigParser<Attribute, IEndpoint>.Lookup("direct.async").IsPresent);
        ClassicAssert.AreEqual(IAnnotationConfigParser<Attribute, IEndpoint>.Lookup("direct.async").Value.GetType(),
            typeof(DirectEndpointConfigParser));
    }
}
