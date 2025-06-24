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

using System.Text.RegularExpressions;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Validation.Xml.Validation.Xml;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Validation.Xml;

public class XpathMessageProcessorTest : AbstractNUnitSetUp
{
    private readonly IMessage _message =
        new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text>Hello World!</Text></TestMessage>");

    private readonly IMessage _messageNamespace = new DefaultMessage(
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ns0:TestMessage xmlns:ns0=\"http://agenix.org/test\">" +
        "<ns0:Text>Hello World!</ns0:Text>" +
        "</ns0:TestMessage>");

    [Test]
    public void TestConstructWithXPath()
    {
        // Arrange
        var xPathExpressions = new Dictionary<string, object> { ["/TestMessage/Text"] = "Hello!" };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(xPathExpressions)
            .Build();

        // Act
        processor.ProcessMessage(_message, Context);

        // Assert
        var normalizedPayload = Regex.Replace(_message.GetPayload<string>(), @"\s", "");
        Assert.That(normalizedPayload, Does.EndWith("<TestMessage><Text>Hello!</Text></TestMessage>"));
    }


    [Test]
    public void TestConstructWithXPathAndDefaultNamespace()
    {
        // Arrange
        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage xmlns=\"http://agenix.org/test\">" +
            "<Text>Hello World!</Text>" +
            "</TestMessage>");

        var xPathExpressions = new Dictionary<string, object>
        {
            // Fixed: Use local-name() function to target elements in the default namespace
            ["/*[local-name()='TestMessage']/*[local-name()='Text']"] = "Hello!"
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(xPathExpressions)
            .Build();

        // Act
        processor.ProcessMessage(message, Context);

        // Assert
        var normalizedPayload = Regex.Replace(message.GetPayload<string>(), @"\s", "");
        Assert.That(normalizedPayload, Does.Contain("<Text>Hello!</Text>"));
    }

    [Test]
    public void TestConstructWithXPathAndNamespace()
    {
        // Arrange
        var xPathExpressions = new Dictionary<string, object> { ["/ns0:TestMessage/ns0:Text"] = "Hello!" };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(xPathExpressions)
            .Build();

        // Act
        processor.ProcessMessage(_messageNamespace, Context);

        // Assert
        var normalizedPayload = Regex.Replace(_messageNamespace.GetPayload<string>(), @"\s", "");
        Assert.That(normalizedPayload, Does.Contain("<ns0:Text>Hello!</ns0:Text>"));
    }

    [Test]
    public void TestConstructWithXPathAndGlobalNamespace()
    {
        // Arrange
        Context.NamespaceContextBuilder.NamespaceMappings["global"] = "http://agenix.org/test";

        var xPathExpressions = new Dictionary<string, object> { ["/global:TestMessage/global:Text"] = "Hello!" };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(xPathExpressions)
            .Build();

        // Act
        processor.ProcessMessage(_messageNamespace, Context);

        // Assert
        var normalizedPayload = Regex.Replace(_messageNamespace.GetPayload<string>(), @"\s", "");
        Assert.That(normalizedPayload, Does.Contain("<ns0:Text>Hello!</ns0:Text>"));
    }

    [Test]
    public void TestConstructWithXPathAndNestedNamespace()
    {
        // Arrange
        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ns0:TestMessage xmlns:ns0=\"http://agenix.org/test\">" +
            "<ns1:Text xmlns:ns1=\"http://agenix.org/test/text\">Hello World!</ns1:Text>" +
            "</ns0:TestMessage>");

        var xPathExpressions = new Dictionary<string, object> { ["/ns0:TestMessage/ns1:Text"] = "Hello!" };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(xPathExpressions)
            .Build();

        // Act
        processor.ProcessMessage(message, Context);

        // Assert
        var normalizedPayload = Regex.Replace(message.GetPayload<string>(), @"\s", "");
        Assert.That(normalizedPayload,
            Does.Contain("<ns1:Textxmlns:ns1=\"http://agenix.org/test/text\">Hello!</ns1:Text>"));
    }

    [Test]
    public void TestConstructWithInvalidXPath()
    {
        // Arrange
        var xPathExpressions = new Dictionary<string, object> { [".Invalid/Unknown"] = "Hello!" };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(xPathExpressions)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<AgenixSystemException>(() =>
            processor.ProcessMessage(_message, Context));

        Assert.That(ex.Message, Does.Match("Cannot evaluate XPath expression.*"));
    }

    [Test]
    public void TestConstructWithXPathNoResult()
    {
        // Arrange
        var xPathExpressions = new Dictionary<string, object> { ["/TestMessage/Unknown"] = "Hello!" };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(xPathExpressions)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<AgenixSystemException>(() =>
            processor.ProcessMessage(_message, Context));

        Assert.That(ex.Message, Does.Match("No result for XPath expression.*"));
    }

    [Test]
    public void TestConstructWithXPathAndInvalidGlobalNamespace()
    {
        // Arrange
        var xPathExpressions = new Dictionary<string, object> { ["/global:TestMessage/global:Text"] = "Hello!" };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(xPathExpressions)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<AgenixSystemException>(() =>
            processor.ProcessMessage(_messageNamespace, Context));

        Assert.That(ex.Message, Does.Match("Cannot evaluate XPath expression.*"));
    }

    [Test]
    public void TestAddTextToEmptyElement()
    {
        // Arrange
        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<TestMessage>" +
            "<Text></Text>" +
            "</TestMessage>");

        var xPathExpression = new Dictionary<string, object> { ["//TestMessage/Text"] = "foobar" };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(xPathExpression)
            .Build();

        // Act
        processor.ProcessMessage(message, Context);

        // Assert
        var normalizedPayload = Regex.Replace(message.GetPayload<string>(), @"\s", "");
        Assert.That(normalizedPayload, Does.Contain("<Text>foobar</Text>"));
    }
}
