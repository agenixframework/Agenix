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
