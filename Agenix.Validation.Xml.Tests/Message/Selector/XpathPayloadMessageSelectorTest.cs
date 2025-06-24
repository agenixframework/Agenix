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

using Agenix.Core.Message;
using Agenix.Validation.Xml.Message.Selector;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Message.Selector;

public class XpathPayloadMessageSelectorTest : AbstractNUnitSetUp
{
    [Test]
    public void TestXPathEvaluation()
    {
        var messageSelector = new XpathPayloadMessageSelector("xpath://Foo/text", "foobar", Context);

        Assert.That(messageSelector.Accept(new DefaultMessage("<Foo><text>foobar</text></Foo>")), Is.True);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<Foo xmlns=\"http://agenix.org/schema\"><text>foobar</text></Foo>")), Is.False);
        Assert.That(messageSelector.Accept(new DefaultMessage("<Bar><text>foobar</text></Bar>")), Is.False);
        Assert.That(messageSelector.Accept(new DefaultMessage("This is plain text!")), Is.False);

        messageSelector = new XpathPayloadMessageSelector("xpath://ns:Foo/ns:text", "foobar", Context);

        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<ns:Foo xmlns:ns=\"http://agenix.org/schema\"><ns:text>foobar</ns:text></ns:Foo>")),
            Is.True);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage(
                    "<ns1:Foo xmlns:ns1=\"http://agenix.org/schema\"><ns1:text>foobar</ns1:text></ns1:Foo>")),
            Is.False);
        Assert.That(messageSelector.Accept(new DefaultMessage("<Bar><text>foobar</text></Bar>")), Is.False);

        messageSelector =
            new XpathPayloadMessageSelector("xpath://{http://agenix.org/schema}Foo/{http://agenix.org/schema}text",
                "foobar", Context);

        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<ns:Foo xmlns:ns=\"http://agenix.org/schema\"><ns:text>foobar</ns:text></ns:Foo>")),
            Is.True);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage(
                    "<ns1:Foo xmlns:ns1=\"http://agenix.org/schema\"><ns1:text>foobar</ns1:text></ns1:Foo>")), Is.True);

        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<Foo xmlns=\"http://agenix.org/unknown\"><text>foobar</text></Foo>")), Is.False);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage(
                    "<ns:Foo xmlns:ns=\"http://agenix.org/unknown\"><ns:text>foobar</ns:text></ns:Foo>")), Is.False);

        messageSelector =
            new XpathPayloadMessageSelector("xpath://{http://agenix.org/schema}Foo/{http://agenix.org/schema2}text",
                "foobar", Context);
        Assert.That(
            messageSelector.Accept(new DefaultMessage(
                "<ns1:Foo xmlns:ns1=\"http://agenix.org/schema\" xmlns:ns2=\"http://agenix.org/schema2\"><ns2:text>foobar</ns2:text></ns1:Foo>")),
            Is.True);

        messageSelector =
            new XpathPayloadMessageSelector("xpath://ns:Foos/ns:Foo[ns:key='KEY-X']/ns:value", "foo", Context);
        Assert.That(
            messageSelector.Accept(new DefaultMessage(
                "<ns:Foos xmlns:ns=\"http://agenix.org/schema\"><ns:Foo><ns:key>KEY-X</ns:key><ns:value>foo</ns:value></ns:Foo><ns:Foo><ns:key>KEY-Y</ns:key><ns:value>bar</ns:value></ns:Foo></ns:Foos>")),
            Is.True);
        Assert.That(
            messageSelector.Accept(new DefaultMessage(
                "<ns:Foos xmlns:ns=\"http://agenix.org/schema\"><ns:Foo><ns:key>KEY-Z</ns:key><ns:value>foo</ns:value></ns:Foo><ns:Foo><ns:key>KEY-Y</ns:key><ns:value>bar</ns:value></ns:Foo></ns:Foos>")),
            Is.False);
    }

    [Test]
    public void TestXPathEvaluationValidationMatcher()
    {
        var messageSelector = new XpathPayloadMessageSelector("xpath://Foo/text", "@StartsWith(foo)@", Context);

        Assert.That(messageSelector.Accept(new DefaultMessage("<Foo><text>foobar</text></Foo>")), Is.True);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<Foo xmlns=\"http://agenix.org/schema\"><text>foobar</text></Foo>")), Is.False);
        Assert.That(messageSelector.Accept(new DefaultMessage("<Bar><text>foobar</text></Bar>")), Is.False);
        Assert.That(messageSelector.Accept(new DefaultMessage("This is plain text!")), Is.False);
    }

    [Test]
    public void TestXPathEvaluationWithMessageObjectPayload()
    {
        var messageSelector = new XpathPayloadMessageSelector("xpath://Foo/text", "foobar", Context);

        Assert.That(messageSelector.Accept(new DefaultMessage(new DefaultMessage("<Foo><text>foobar</text></Foo>"))),
            Is.True);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage(
                    new DefaultMessage("<Foo xmlns=\"http://agenix.org/schema\"><text>foobar</text></Foo>"))),
            Is.False);
        Assert.That(messageSelector.Accept(new DefaultMessage(new DefaultMessage("<Bar><text>foobar</text></Bar>"))),
            Is.False);
        Assert.That(messageSelector.Accept(new DefaultMessage(new DefaultMessage("This is plain text!"))), Is.False);
    }
}
