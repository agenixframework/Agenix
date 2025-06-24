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
using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Validation.Xml.Functions.Core;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Functions.Core;

[TestFixture]
public class CreateCDataSectionFunctionTest : AbstractNUnitSetUp
{
    private readonly CreateCDataSectionFunction _function = new();

    [Test]
    public void TestFunction()
    {
        const string xmlFragment = "<foo><bar>I like Agenix!</bar></foo>";
        const string resultXml = "<![CDATA[<foo><bar>I like Agenix!</bar></foo>]]>";

        Assert.That(_function.Execute([xmlFragment], Context), Is.EqualTo(resultXml));
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.That(() => _function.Execute([], Context),
            Throws.TypeOf<InvalidFunctionUsageException>());
    }

    [Test]
    public void ShouldLookupFunction()
    {
        var functionLookup = IFunction.Lookup();

        Assert.That(functionLookup.ContainsKey("cdataSection"), Is.True);
        Assert.That(functionLookup["cdataSection"].GetType(), Is.EqualTo(typeof(CreateCDataSectionFunction)));

        var defaultLibrary = new DefaultFunctionLibrary();
        Assert.That(defaultLibrary.GetFunction("cdataSection").GetType(),
            Is.EqualTo(typeof(CreateCDataSectionFunction)));
    }
}
