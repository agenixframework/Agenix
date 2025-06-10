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
