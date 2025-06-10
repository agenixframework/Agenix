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
using Agenix.Core.Message;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Xml;

public class XmlFormattingMessageProcessorTest : AbstractNUnitSetUp
{
    private XmlFormattingMessageProcessor _messageProcessor;

    [SetUp]
    public void SetUp()
    {
        _messageProcessor = new XmlFormattingMessageProcessor();
    }

    [Test]
    public void TestProcessMessage()
    {
        // Arrange
        var message = new DefaultMessage("<root>" +
                                         "<element attribute='attribute-value'>" +
                                         "<sub-element>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        // Act
        _messageProcessor.Process(message, Context);

        // Assert
        Assert.That(message.GetPayload<string>(), Contains.Substring("\n"));
    }

    [Test]
    public void TestProcessMessageExplicitType()
    {
        // Arrange
        var message = new DefaultMessage("<root>" +
                                         "<element attribute='attribute-value'>" +
                                         "<sub-element>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");
        message.SetType(MessageType.XML);

        // Act
        _messageProcessor.Process(message, Context);

        // Assert
        Assert.That(message.GetPayload<string>(), Does.Contain("\n"));
    }

    [Test]
    public void TestProcessNonXmlMessage()
    {
        // Arrange
        var message = new DefaultMessage("This is plaintext");
        message.SetType(MessageType.PLAINTEXT);

        // Act
        _messageProcessor.Process(message, Context);

        // Assert
        Assert.That(message.GetPayload<string>(), Is.EqualTo("This is plaintext"));
    }

}
