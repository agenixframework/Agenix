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

using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Validation.Xml.Functions;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Functions;

public class XmlFunctionsTest : AbstractNUnitSetUp
{
    [Test]
    public void TestCreateCDataSection()
    {
        Assert.That(XmlFunctions.CreateCDataSection("<Test><Message>Some Text</Message></Test>", Context),
            Is.EqualTo("<![CDATA[<Test><Message>Some Text</Message></Test>]]>"));
    }

    [Test]
    public void TestEscapeXml()
    {
        Assert.That(XmlFunctions.EscapeXml("<Test><Message>Some Text</Message></Test>", Context),
            Is.EqualTo("&lt;Test&gt;&lt;Message&gt;Some Text&lt;/Message&gt;&lt;/Test&gt;"));
    }

    [Test]
    public void TestXpath()
    {
        Assert.That(XmlFunctions.XPath("<Test><Message>Some Text</Message></Test>", "/Test/Message", Context),
            Is.EqualTo("Some Text"));
    }

    [Test]
    public void TestFunctionUtils()
    {
        Context.FunctionRegistry = new DefaultFunctionRegistry();

        Assert.That(
            FunctionUtils.ResolveFunction("agenix:EscapeXml('<Message>Hello Yes, I like Agenix!</Message>')", Context),
            Is.EqualTo("&lt;Message&gt;Hello Yes, I like Agenix!&lt;/Message&gt;"));

        Assert.That(
            FunctionUtils.ResolveFunction("agenix:escapeXml('<Message>Hello Yes , I like Agenix!</Message>')", Context),
            Is.EqualTo("&lt;Message&gt;Hello Yes , I like Agenix!&lt;/Message&gt;"));

        Assert.That(
            FunctionUtils.ResolveFunction(
                "agenix:escapeXml('<Message>Hello Yes,I like Agenix, and this is great!</Message>')", Context),
            Is.EqualTo("&lt;Message&gt;Hello Yes,I like Agenix, and this is great!&lt;/Message&gt;"));

        Assert.That(FunctionUtils.ResolveFunction("agenix:cdataSection('<Message>Hello Agenix!</Message>')", Context),
            Is.EqualTo("<![CDATA[<Message>Hello Agenix!</Message>]]>"));

        Assert.That(
            FunctionUtils.ResolveFunction("agenix:Xpath('<Message>Hello Agenix!</Message>', '/Message')", Context),
            Is.EqualTo("Hello Agenix!"));
    }
}
