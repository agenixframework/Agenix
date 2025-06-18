#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion


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
        var factories = new Dictionary<string, IMessageSelector.IMessageSelectorFactory>
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
