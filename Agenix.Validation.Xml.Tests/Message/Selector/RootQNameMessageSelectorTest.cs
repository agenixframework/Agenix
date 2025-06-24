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
using Agenix.Validation.Xml.Message.Selector;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Message.Selector;

public class RootQNameMessageSelectorTest : AbstractNUnitSetUp
{
    [Test]
    public void TestQNameSelector()
    {
        var messageSelector = new RootQNameMessageSelector(RootQNameMessageSelector.SelectorId, "Foo", Context);

        Assert.That(messageSelector.Accept(new DefaultMessage("<Foo><text>foobar</text></Foo>")), Is.True);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<Foo xmlns=\"http://agenix.org/schema\"><text>foobar</text></Foo>")), Is.True);
        Assert.That(messageSelector.Accept(new DefaultMessage("<Bar><text>foobar</text></Bar>")), Is.False);

        messageSelector = new RootQNameMessageSelector(RootQNameMessageSelector.SelectorId, "{}Foo", Context);

        Assert.That(messageSelector.Accept(new DefaultMessage("<Foo><text>foobar</text></Foo>")), Is.True);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<Foo xmlns=\"http://agenix.org/schema\"><text>foobar</text></Foo>")), Is.True);
        Assert.That(messageSelector.Accept(new DefaultMessage("<Bar><text>foobar</text></Bar>")), Is.False);
    }

    [Test]
    public void TestQNameSelectorWithMessageObjectPayload()
    {
        var messageSelector = new RootQNameMessageSelector(RootQNameMessageSelector.SelectorId, "Foo", Context);

        Assert.That(messageSelector.Accept(new DefaultMessage(new DefaultMessage("<Foo><text>foobar</text></Foo>"))),
            Is.True);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage(
                    new DefaultMessage("<Foo xmlns=\"http://agenix.org/schema\"><text>foobar</text></Foo>"))), Is.True);
        Assert.That(messageSelector.Accept(new DefaultMessage(new DefaultMessage("<Bar><text>foobar</text></Bar>"))),
            Is.False);
    }

    [Test]
    public void TestQNameWithNamespaceSelector()
    {
        var messageSelector = new RootQNameMessageSelector(RootQNameMessageSelector.SelectorId,
            "{http://agenix.org/schema}Foo", Context);

        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<Foo xmlns=\"http://agenix.org/schema\"><text>foobar</text></Foo>")), Is.True);
        Assert.That(messageSelector.Accept(new DefaultMessage("<Foo><text>foobar</text></Foo>")), Is.False);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<Foo xmlns=\"http://agenix.org/schema/foo\"><text>foobar</text></Foo>")), Is.False);
        Assert.That(
            messageSelector.Accept(
                new DefaultMessage("<Bar xmlns=\"http://agenix.org/schema\"><text>foobar</text></Bar>")), Is.False);
    }

    [Test]
    public void TestNonXmlPayload()
    {
        var messageSelector = new RootQNameMessageSelector(RootQNameMessageSelector.SelectorId,
            "{http://agenix.org/schema}Foo", Context);

        Assert.That(messageSelector.Accept(new DefaultMessage("PLAINTEXT")), Is.False);
    }

    [Test]
    public void TestInvalidQName()
    {
        var ex = Assert.Throws<AgenixSystemException>(() =>
        {
            var rootQNameMessageSelector = new RootQNameMessageSelector(RootQNameMessageSelector.SelectorId,
                "{http://agenix.org/schemaFoo",
                Context);
        });

        Assert.That(ex.Message, Does.StartWith("Invalid root QName"));
    }
}
