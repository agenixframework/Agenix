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

using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Messaging;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;


namespace Agenix.Validation.Xml.Tests;

public class ValidationTest : AbstractNUnitSetUp
{
    private Mock<IConsumer> _consumer;
    private Mock<IEndpoint> _endpoint;
    private Mock<IEndpointConfiguration> _endpointConfiguration;

    [SetUp]
    public void SetUp()
    {
        _endpoint = new Mock<IEndpoint>();
        _consumer = new Mock<IConsumer>();
        _endpointConfiguration = new Mock<IEndpointConfiguration>();
    }

    [Test]
    public void TestValidateXmlTree()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - Validation should pass for matching XML structure
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithAttributes()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<order id='12345' status='pending'>" +
                                         "<customer type='premium' level='gold'>" +
                                         "<name first='John' last='Doe'>John Doe</name>" +
                                         "<email verified='true'>john.doe@example.com</email>" +
                                         "<phone country='+1' area='555'>555-0123</phone>" +
                                         "</customer>" +
                                         "<items count='3' total='299.97'>" +
                                         "<item id='item1' price='99.99'>Product A</item>" +
                                         "<item id='item2' price='149.99'>Product B</item>" +
                                         "<item id='item3' price='49.99'>Product C</item>" +
                                         "</items>" +
                                         "</order>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<order id='12345' status='pending'>" +
                                                                          "<customer type='premium' level='gold'>" +
                                                                          "<name first='John' last='Doe'>John Doe</name>" +
                                                                          "<email verified='true'>john.doe@example.com</email>" +
                                                                          "<phone country='+1' area='555'>555-0123</phone>" +
                                                                          "</customer>" +
                                                                          "<items count='3' total='299.97'>" +
                                                                          "<item id='item1' price='99.99'>Product A</item>" +
                                                                          "<item id='item2' price='149.99'>Product B</item>" +
                                                                          "<item id='item3' price='49.99'>Product C</item>" +
                                                                          "</items>" +
                                                                          "</order>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML with complex attributes should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithNamespaces()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version='1.0' encoding='UTF-8'?>" +
                                         "<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' " +
                                         "xmlns:cit='http://agenix.org/test'>" +
                                         "<soap:Header>" +
                                         "<cit:MessageId>MSG-12345</cit:MessageId>" +
                                         "<cit:Timestamp>2024-06-06T10:30:00Z</cit:Timestamp>" +
                                         "</soap:Header>" +
                                         "<soap:Body>" +
                                         "<cit:TestRequest>" +
                                         "<cit:RequestData type='xml'>" +
                                         "<cit:Element value='test'>Content</cit:Element>" +
                                         "</cit:RequestData>" +
                                         "</cit:TestRequest>" +
                                         "</soap:Body>" +
                                         "</soap:Envelope>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<?xml version='1.0' encoding='UTF-8'?>" +
                                                                          "<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' " +
                                                                          "xmlns:cit='http://agenix.org/test'>" +
                                                                          "<soap:Header>" +
                                                                          "<cit:MessageId>MSG-12345</cit:MessageId>" +
                                                                          "<cit:Timestamp>2024-06-06T10:30:00Z</cit:Timestamp>" +
                                                                          "</soap:Header>" +
                                                                          "<soap:Body>" +
                                                                          "<cit:TestRequest>" +
                                                                          "<cit:RequestData type='xml'>" +
                                                                          "<cit:Element value='test'>Content</cit:Element>" +
                                                                          "</cit:RequestData>" +
                                                                          "</cit:TestRequest>" +
                                                                          "</soap:Body>" +
                                                                          "</soap:Envelope>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML with namespaces should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithMixedContent()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<document>" +
                                         "<section id='intro'>" +
                                         "This is <emphasis>mixed content</emphasis> with both text and elements." +
                                         "<note type='warning'>Pay attention to spacing!</note>" +
                                         "More text after the note element." +
                                         "</section>" +
                                         "<section id='content'>" +
                                         "<paragraph>First paragraph with <code>inline code</code>.</paragraph>" +
                                         "<paragraph>Second paragraph with <link href='#ref'>reference</link>.</paragraph>" +
                                         "</section>" +
                                         "</document>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<document>" +
                                                                          "<section id='intro'>" +
                                                                          "This is <emphasis>mixed content</emphasis> with both text and elements." +
                                                                          "<note type='warning'>Pay attention to spacing!</note>" +
                                                                          "More text after the note element." +
                                                                          "</section>" +
                                                                          "<section id='content'>" +
                                                                          "<paragraph>First paragraph with <code>inline code</code>.</paragraph>" +
                                                                          "<paragraph>Second paragraph with <link href='#ref'>reference</link>.</paragraph>" +
                                                                          "</section>" +
                                                                          "</document>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML with mixed content should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithCData()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<response>" +
                                         "<status>success</status>" +
                                         "<data>" +
                                         "<![CDATA[" +
                                         "<script type='text/javascript'>" +
                                         "function test() { return 'Hello & Goodbye'; }" +
                                         "</script>" +
                                         "]]>" +
                                         "</data>" +
                                         "<htmlContent>" +
                                         "<![CDATA[" +
                                         "<div class='content'>" +
                                         "<p>This is HTML content with <b>bold</b> text.</p>" +
                                         "<a href='http://example.com?param=value&other=123'>Link</a>" +
                                         "</div>" +
                                         "]]>" +
                                         "</htmlContent>" +
                                         "</response>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<response>" +
                                                                          "<status>success</status>" +
                                                                          "<data>" +
                                                                          "<![CDATA[" +
                                                                          "<script type='text/javascript'>" +
                                                                          "function test() { return 'Hello & Goodbye'; }" +
                                                                          "</script>" +
                                                                          "]]>" +
                                                                          "</data>" +
                                                                          "<htmlContent>" +
                                                                          "<![CDATA[" +
                                                                          "<div class='content'>" +
                                                                          "<p>This is HTML content with <b>bold</b> text.</p>" +
                                                                          "<a href='http://example.com?param=value&other=123'>Link</a>" +
                                                                          "</div>" +
                                                                          "]]>" +
                                                                          "</htmlContent>" +
                                                                          "</response>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML with CDATA sections should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithEmptyElements()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<configuration>" +
                                         "<database>" +
                                         "<host>localhost</host>" +
                                         "<port>5432</port>" +
                                         "<username>admin</username>" +
                                         "<password></password>" +
                                         "<ssl-enabled/>" +
                                         "<timeout/>" +
                                         "<pool-size>10</pool-size>" +
                                         "</database>" +
                                         "<features>" +
                                         "<feature name='logging' enabled='true'/>" +
                                         "<feature name='monitoring' enabled='false'/>" +
                                         "<feature name='caching'/>" +
                                         "</features>" +
                                         "</configuration>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<configuration>" +
                                                                          "<database>" +
                                                                          "<host>localhost</host>" +
                                                                          "<port>5432</port>" +
                                                                          "<username>admin</username>" +
                                                                          "<password></password>" +
                                                                          "<ssl-enabled/>" +
                                                                          "<timeout/>" +
                                                                          "<pool-size>10</pool-size>" +
                                                                          "</database>" +
                                                                          "<features>" +
                                                                          "<feature name='logging' enabled='true'/>" +
                                                                          "<feature name='monitoring' enabled='false'/>" +
                                                                          "<feature name='caching'/>" +
                                                                          "</features>" +
                                                                          "</configuration>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML with empty elements should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithSpecialCharacters()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version='1.0' encoding='UTF-8'?>" +
                                         "<message>" +
                                         "<content>" +
                                         "<text>Special characters: &lt; &gt; &amp; &quot; &apos;</text>" +
                                         "<unicode>Unicode: ñ é ü ß 中文 العربية русский</unicode>" +
                                         "<symbols>Symbols: © ® ™ € $ £ ¥</symbols>" +
                                         "<math>Math: α β γ ∑ ∆ ∞ ≈ ≠ ≤ ≥</math>" +
                                         "</content>" +
                                         "<attributes>" +
                                         "<element value='&quot;quoted&quot;' symbol='&amp;' comparison='a&lt;b&gt;c'/>" +
                                         "<unicode-attr name='café' city='São Paulo' country='España'/>" +
                                         "</attributes>" +
                                         "</message>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<?xml version='1.0' encoding='UTF-8'?>" +
                                                                          "<message>" +
                                                                          "<content>" +
                                                                          "<text>Special characters: &lt; &gt; &amp; &quot; &apos;</text>" +
                                                                          "<unicode>Unicode: ñ é ü ß 中文 العربية русский</unicode>" +
                                                                          "<symbols>Symbols: © ® ™ € $ £ ¥</symbols>" +
                                                                          "<math>Math: α β γ ∑ ∆ ∞ ≈ ≠ ≤ ≥</math>" +
                                                                          "</content>" +
                                                                          "<attributes>" +
                                                                          "<element value='&quot;quoted&quot;' symbol='&amp;' comparison='a&lt;b&gt;c'/>" +
                                                                          "<unicode-attr name='café' city='São Paulo' country='España'/>" +
                                                                          "</attributes>" +
                                                                          "</message>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML with special characters should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithNestedStructure()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<company>" +
                                         "<departments>" +
                                         "<department id='engineering'>" +
                                         "<name>Engineering</name>" +
                                         "<teams>" +
                                         "<team id='backend'>" +
                                         "<name>Backend Development</name>" +
                                         "<members>" +
                                         "<member id='1' role='lead'>" +
                                         "<name>Alice Johnson</name>" +
                                         "<email>alice@company.com</email>" +
                                         "<skills>" +
                                         "<skill level='expert'>Java</skill>" +
                                         "<skill level='advanced'>Python</skill>" +
                                         "<skill level='intermediate'>Go</skill>" +
                                         "</skills>" +
                                         "</member>" +
                                         "<member id='2' role='developer'>" +
                                         "<name>Bob Smith</name>" +
                                         "<email>bob@company.com</email>" +
                                         "<skills>" +
                                         "<skill level='advanced'>Java</skill>" +
                                         "<skill level='expert'>Kotlin</skill>" +
                                         "</skills>" +
                                         "</member>" +
                                         "</members>" +
                                         "</team>" +
                                         "<team id='frontend'>" +
                                         "<name>Frontend Development</name>" +
                                         "<members>" +
                                         "<member id='3' role='lead'>" +
                                         "<name>Carol Davis</name>" +
                                         "<email>carol@company.com</email>" +
                                         "<skills>" +
                                         "<skill level='expert'>React</skill>" +
                                         "<skill level='advanced'>TypeScript</skill>" +
                                         "</skills>" +
                                         "</member>" +
                                         "</members>" +
                                         "</team>" +
                                         "</teams>" +
                                         "</department>" +
                                         "</departments>" +
                                         "</company>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<company>" +
                                                                          "<departments>" +
                                                                          "<department id='engineering'>" +
                                                                          "<name>Engineering</name>" +
                                                                          "<teams>" +
                                                                          "<team id='backend'>" +
                                                                          "<name>Backend Development</name>" +
                                                                          "<members>" +
                                                                          "<member id='1' role='lead'>" +
                                                                          "<name>Alice Johnson</name>" +
                                                                          "<email>alice@company.com</email>" +
                                                                          "<skills>" +
                                                                          "<skill level='expert'>Java</skill>" +
                                                                          "<skill level='advanced'>Python</skill>" +
                                                                          "<skill level='intermediate'>Go</skill>" +
                                                                          "</skills>" +
                                                                          "</member>" +
                                                                          "<member id='2' role='developer'>" +
                                                                          "<name>Bob Smith</name>" +
                                                                          "<email>bob@company.com</email>" +
                                                                          "<skills>" +
                                                                          "<skill level='advanced'>Java</skill>" +
                                                                          "<skill level='expert'>Kotlin</skill>" +
                                                                          "</skills>" +
                                                                          "</member>" +
                                                                          "</members>" +
                                                                          "</team>" +
                                                                          "<team id='frontend'>" +
                                                                          "<name>Frontend Development</name>" +
                                                                          "<members>" +
                                                                          "<member id='3' role='lead'>" +
                                                                          "<name>Carol Davis</name>" +
                                                                          "<email>carol@company.com</email>" +
                                                                          "<skills>" +
                                                                          "<skill level='expert'>React</skill>" +
                                                                          "<skill level='advanced'>TypeScript</skill>" +
                                                                          "</skills>" +
                                                                          "</member>" +
                                                                          "</members>" +
                                                                          "</team>" +
                                                                          "</teams>" +
                                                                          "</department>" +
                                                                          "</departments>" +
                                                                          "</company>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - Deeply nested XML structure should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithComments()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version='1.0' encoding='UTF-8'?>" +
                                         "<!-- Main configuration file -->" +
                                         "<config version='1.0'>" +
                                         "<!-- Database settings -->" +
                                         "<database>" +
                                         "<host>localhost</host>" +
                                         "<!-- Default port for PostgreSQL -->" +
                                         "<port>5432</port>" +
                                         "<credentials>" +
                                         "<username>admin</username>" +
                                         "<!-- Password should be encrypted in production -->" +
                                         "<password>secret</password>" +
                                         "</credentials>" +
                                         "</database>" +
                                         "<!-- Application settings -->" +
                                         "<application>" +
                                         "<name>Test Application</name>" +
                                         "<!-- Feature flags -->" +
                                         "<features>" +
                                         "<logging enabled='true'/><!-- Enable detailed logging -->" +
                                         "<monitoring enabled='false'/><!-- Disable in development -->" +
                                         "</features>" +
                                         "</application>" +
                                         "<!-- End of configuration -->" +
                                         "</config>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<?xml version='1.0' encoding='UTF-8'?>" +
                                                                          "<!-- Main configuration file -->" +
                                                                          "<config version='1.0'>" +
                                                                          "<!-- Database settings -->" +
                                                                          "<database>" +
                                                                          "<host>localhost</host>" +
                                                                          "<!-- Default port for PostgreSQL -->" +
                                                                          "<port>5432</port>" +
                                                                          "<credentials>" +
                                                                          "<username>admin</username>" +
                                                                          "<!-- Password should be encrypted in production -->" +
                                                                          "<password>secret</password>" +
                                                                          "</credentials>" +
                                                                          "</database>" +
                                                                          "<!-- Application settings -->" +
                                                                          "<application>" +
                                                                          "<name>Test Application</name>" +
                                                                          "<!-- Feature flags -->" +
                                                                          "<features>" +
                                                                          "<logging enabled='true'/><!-- Enable detailed logging -->" +
                                                                          "<monitoring enabled='false'/><!-- Disable in development -->" +
                                                                          "</features>" +
                                                                          "</application>" +
                                                                          "<!-- End of configuration -->" +
                                                                          "</config>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML with comments should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithProcessingInstructions()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version='1.0' encoding='UTF-8'?>" +
                                         "<?xml-stylesheet type='text/xsl' href='transform.xsl'?>" +
                                         "<?processing-instruction data='value'?>" +
                                         "<document>" +
                                         "<?page-break?>" +
                                         "<section>" +
                                         "<title>Introduction</title>" +
                                         "<content>Document content here.</content>" +
                                         "<?page-break?>" +
                                         "</section>" +
                                         "<section>" +
                                         "<title>Conclusion</title>" +
                                         "<content>Final thoughts.</content>" +
                                         "</section>" +
                                         "<?end-of-document?>" +
                                         "</document>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<?xml version='1.0' encoding='UTF-8'?>" +
                                                                          "<?xml-stylesheet type='text/xsl' href='transform.xsl'?>" +
                                                                          "<?processing-instruction data='value'?>" +
                                                                          "<document>" +
                                                                          "<?page-break?>" +
                                                                          "<section>" +
                                                                          "<title>Introduction</title>" +
                                                                          "<content>Document content here.</content>" +
                                                                          "<?page-break?>" +
                                                                          "</section>" +
                                                                          "<section>" +
                                                                          "<title>Conclusion</title>" +
                                                                          "<content>Final thoughts.</content>" +
                                                                          "</section>" +
                                                                          "<?end-of-document?>" +
                                                                          "</document>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML with processing instructions should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWithVariableContent()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        // Set up test variables
        Context.GetVariables()["orderId"] = "ORD-12345";
        Context.GetVariables()["customerName"] = "John Doe";
        Context.GetVariables()["totalAmount"] = "299.99";
        Context.GetVariables()["currency"] = "USD";
        Context.GetVariables()["orderDate"] = "2024-06-06";

        var message = new DefaultMessage("<order>" +
                                         "<id>ORD-12345</id>" +
                                         "<customer>John Doe</customer>" +
                                         "<total currency='USD'>299.99</total>" +
                                         "<date>2024-06-06</date>" +
                                         "<status>confirmed</status>" +
                                         "</order>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<order>" +
                                                                          "<id>${orderId}</id>" +
                                                                          "<customer>${customerName}</customer>" +
                                                                          "<total currency='${currency}'>${totalAmount}</total>" +
                                                                          "<date>${orderDate}</date>" +
                                                                          "<status>confirmed</status>" +
                                                                          "</order>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML with variable substitution should validate correctly
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeDifferentAttributeOrder()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeB='attribute-value' attributeA='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);

        // Assert - XML validation should succeed despite different attribute order
        Assert.That(receiveAction, Is.Not.Null);
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeMissingElement()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Act & Assert - Expecting ValidationException due to missing sub-elementC
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message
                .Contains("Number of child elements not equal for element 'element', expected '2' but was '3'"));

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeAdditionalElement()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "<sub-elementD attribute='D'>text-value</sub-elementD>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Act & Assert - Expecting ValidationException due to additional sub-elementD in control message
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message
                .Contains("Number of child elements not equal for element 'element', expected '4' but was '3'"));

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeMissingAttribute()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Act & Assert - Expecting ValidationException due to missing attribute='A' in control message
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("attribute"));

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeAdditionalAttribute()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A' attribute-additional='additional'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Act & Assert - Expecting ValidationException due to additional attribute-additional='additional' in control message
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message
                .Contains("Number of attributes not equal for element 'sub-elementA', expected '2' but was '1'"));

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWrongAttribute()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute-wrong='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Act & Assert - Expecting ValidationException due to wrong attribute name 'attribute-wrong' instead of 'attribute'
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("attribute"));

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWrongElement()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-element-wrong attribute='A'>text-value</sub-element-wrong>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Act & Assert - Expecting ValidationException due to wrong element name 'sub-element-wrong' instead of 'sub-elementA'
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("sub-element"));

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWrongNodeValue()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>wrong-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Act & Assert - Expecting ValidationException due to wrong node value 'wrong-value' instead of 'text-value'
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("text"));

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateXmlTreeWrongAttributeValue()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='wrong-value'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Act & Assert - Expecting ValidationException due to wrong attribute value 'wrong-value' instead of 'A'
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("attribute"));

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }
}
