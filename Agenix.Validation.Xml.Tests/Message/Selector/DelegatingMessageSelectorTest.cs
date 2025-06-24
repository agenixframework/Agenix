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

using System.Collections.Concurrent;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Xml.Namespace;
using Agenix.Core.Message;
using Agenix.Core.Message.Selector;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Message.Selector;

public class DelegatingMessageSelectorTest : AbstractNUnitSetUp
{
    private readonly Mock<IReferenceResolver> _resolver = new();

    [Test]
    public void TestRootQNameDelegation()
    {
        var messageSelector = new DelegatingMessageSelector("foo = 'bar' AND root-qname = 'FooTest'", Context);

        var acceptMessage = new DefaultMessage("<FooTest><text>foobar</text></FooTest>")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("<BarTest><text>foobar</text></BarTest>")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);

        messageSelector = new DelegatingMessageSelector("root-qname = 'FooTest'", Context);

        acceptMessage = new DefaultMessage("<FooTest><text>foobar</text></FooTest>")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        declineMessage = new DefaultMessage("<BarTest><text>foobar</text></BarTest>")
            .SetHeader("operation", "foo");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);
    }

    [Test]
    public void TestHeaderMatchingSelector()
    {
        var messageSelector = new DelegatingMessageSelector("operation = 'foo'", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foobar");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);
    }

    [Test]
    public void TestHeaderMatchingSelectorAndOperation()
    {
        var messageSelector = new DelegatingMessageSelector("foo = 'bar' AND operation = 'foo'", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);
    }

    [Test]
    public void TestPayloadMatchingDelegation()
    {
        var messageSelector = new DelegatingMessageSelector("foo = 'bar' AND payload = 'FooTest'", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("BarTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);

        messageSelector = new DelegatingMessageSelector("payload = 'FooTest'", Context);

        acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        declineMessage = new DefaultMessage("BarTest")
            .SetHeader("operation", "foo");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);
    }

    [Test]
    public void TestPayloadAndHeaderMatchingDelegation()
    {
        var messageSelector = new DelegatingMessageSelector("header:payload = 'foo' AND payload = 'foo'", Context);

        Assert.That(messageSelector.Accept(new DefaultMessage("foo")
            .SetHeader("payload", "foo")), Is.True);

        Assert.That(messageSelector.Accept(new DefaultMessage("foo")
            .SetHeader("payload", "bar")), Is.False);

        Assert.That(messageSelector.Accept(new DefaultMessage("bar")
            .SetHeader("payload", "foo")), Is.False);
    }

    [Test]
    public void TestRootQNameDelegationWithNamespace()
    {
        var messageSelector =
            new DelegatingMessageSelector("root-qname = '{http://agenix.org/fooschema}FooTest'", Context);

        var acceptMessage =
            new DefaultMessage("<FooTest xmlns=\"http://agenix.org/fooschema\"><text>foo</text></FooTest>")
                .SetHeader("operation", "foo");

        var declineMessage =
            new DefaultMessage("<FooTest xmlns=\"http://agenix.org/barschema\"><text>bar</text></FooTest>")
                .SetHeader("operation", "foo");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);
    }

    [Test]
    public void TestXPathEvaluationDelegation()
    {
        var messageSelector =
            new DelegatingMessageSelector("foo = 'bar' AND root-qname = 'FooTest' AND xpath://FooTest/text = 'foobar'",
                Context);

        var acceptMessage = new DefaultMessage("<FooTest><text>foobar</text></FooTest>")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("<FooTest><text>barfoo</text></FooTest>")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);

        messageSelector = new DelegatingMessageSelector("xpath://FooTest/text = 'foobar'", Context);

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);
    }


    [Test]
    public void TestCustomMessageSelectorDelegation()
    {
        var factories = new ConcurrentDictionary<string, IMessageSelector.IMessageSelectorFactory>
        {
            ["customSelectorFactory"] = new CustomMessageSelectorFactory()
        };

        _resolver.Setup(r => r.ResolveAll<IMessageSelector.IMessageSelectorFactory>())
            .Returns(factories);

        Context.SetReferenceResolver(_resolver.Object);
        var messageSelector = new DelegatingMessageSelector("x:foo = 'bar'", Context);

        var acceptMessage = new DefaultMessage("FooBar")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooBar")
            .SetHeader("foo", "bars")
            .SetHeader("operation", "foo");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);
    }

    [Test]
    public void TestXPathEvaluationDelegationWithNamespaceBuilder()
    {
        var nsContextBuilder = new NamespaceContextBuilder();
        nsContextBuilder.NamespaceMappings["foo"] = "http://agenix.org/foo";

        Context.NamespaceContextBuilder = nsContextBuilder;

        _resolver.Reset();

        _resolver.Setup(r => r.Resolve<NamespaceContextBuilder>())
            .Returns(nsContextBuilder);

        var messageSelector =
            new DelegatingMessageSelector(
                "foo = 'bar' AND root-qname = 'FooTest' AND xpath://foo:FooTest/foo:text = 'foobar'", Context);

        var acceptMessage =
            new DefaultMessage("<FooTest xmlns=\"http://agenix.org/foo\"><text>foobar</text></FooTest>")
                .SetHeader("foo", "bar")
                .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("<FooTest><text>barfoo</text></FooTest>")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);

        messageSelector = new DelegatingMessageSelector("xpath://foo:FooTest/foo:text = 'foobar'", Context);

        Assert.That(messageSelector.Accept(acceptMessage), Is.True);
        Assert.That(messageSelector.Accept(declineMessage), Is.False);
    }


    private class CustomMessageSelectorFactory : IMessageSelector.IMessageSelectorFactory
    {
        public bool Supports(string key)
        {
            return key.StartsWith("x:");
        }

        public IMessageSelector Create(string key, string value, TestContext context)
        {
            return new CustomMessageSelector(value);
        }
    }

    private class CustomMessageSelector : IMessageSelector
    {
        private readonly string _expectedValue;

        public CustomMessageSelector(string expectedValue)
        {
            _expectedValue = expectedValue;
        }

        public bool Accept(IMessage message)
        {
            return message.GetHeaders().TryGetValue("foo", out var headerValue) &&
                   headerValue?.ToString() == _expectedValue;
        }
    }
}
