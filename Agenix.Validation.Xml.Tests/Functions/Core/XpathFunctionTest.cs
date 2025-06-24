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
public class XpathFunctionTest : AbstractNUnitSetUp
{
    private readonly XpathFunction _function = new();
    private const string XmlSource = "<person><name>Sheldon</name><age>29</age></person>";

    [Test]
    public void TestExecuteXpath()
    {
        var parameters = new List<string> { XmlSource, "/person/name" };

        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("Sheldon"));
    }

    [Test]
    public void TestExecuteXpathWithNamespaces()
    {
        var parameters = new List<string>();
        const string xmlSourceNamespace =
            "<person xmlns=\"http://agenix.sample.org/person\"><name>Sheldon</name><age>29</age></person>";
        parameters.Add(xmlSourceNamespace);
        parameters.Add("/p:person/p:name");

        Context.NamespaceContextBuilder.NamespaceMappings["p"] = "http://agenix.sample.org/person";

        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("Sheldon"));
    }

    [Test]
    public void TestExecuteXpathUnknown()
    {
        var parameters = new List<string> { XmlSource, "/person/unknown" };

        Assert.That(() => _function.Execute(parameters, Context),
            Throws.TypeOf<AgenixSystemException>());
    }

    [Test]
    public void ShouldLookupFunction()
    {
        var functionLookup = IFunction.Lookup();

        Assert.That(functionLookup.ContainsKey("xpath"), Is.True);
        Assert.That(functionLookup["xpath"].GetType(), Is.EqualTo(typeof(XpathFunction)));

        var defaultLibrary = new DefaultFunctionLibrary();
        Assert.That(defaultLibrary.GetFunction("xpath").GetType(), Is.EqualTo(typeof(XpathFunction)));
    }
}
