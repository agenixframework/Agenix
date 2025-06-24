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

using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Functions;

public class EscapeXmlFunctionTest : AbstractNUnitSetUp
{
    private const string EscapedXml = "&lt;foo&gt;&lt;bar&gt;Yes, I like W0rld!&lt;/bar&gt;&lt;/foo&gt;";

    private const string XmlFragment = "<foo><bar>Yes, I like W0rld!</bar></foo>";
    private readonly EscapeXmlFunction _function = new();

    [Test]
    public void TestEscapeXmlFunction()
    {
        ClassicAssert.AreEqual(_function.Execute([XmlFragment], Context), EscapedXml);
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.Throws<InvalidFunctionUsageException>(() =>
            _function.Execute(new List<string>(), Context)
        );
    }
}
