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
using Agenix.Api.Messaging;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests;

public class XPathTest : AbstractNUnitSetUp
{
    private readonly Mock<IConsumer> _consumer = new();
    private readonly Mock<IEndpoint> _endpoint = new();
    private readonly Mock<IEndpointConfiguration> _endpointConfiguration = new();

    [Test]
    public void TestUsingXPath()
    {
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns='http://test' xmlns:ns1='http://agenix'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "<ns1:ns-element>namespace</ns1:ns-element>" +
                                         "<search-element>search-for</search-element>" +
                                         "</ns1:root>");


        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            // Use local-name() to ignore namespaces
            { "//*[local-name()='element']/*[local-name()='sub-elementA']", "text-value" },
            { "//*[local-name()='element']/*[local-name()='sub-elementA'][@attribute='A']", "text-value" },
            { "//*[local-name()='element']/*[local-name()='sub-elementB']", "text-value" },
            { "//*[local-name()='element']/*[local-name()='sub-elementB']/@attribute", "B" },
            { "//ns1:ns-element", "namespace" },
            { "//*[.='search-for']", "search-for" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);

        // Optional: Add verification that the action was called correctly
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestUsingXPathWithDefaultNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root xmlns='http://test'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "<ns-element>namespace</ns-element>" +
            "<search-element>search-for</search-element>" +
            "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            // Using local-name() for namespace-agnostic validation
            { "//*[local-name()='element']/*[local-name()='sub-elementA']", "text-value" },
            { "//*[local-name()='element']/*[local-name()='sub-elementA'][@attribute='A']", "text-value" },
            { "//*[local-name()='element']/*[local-name()='sub-elementB']", "text-value" },
            { "//*[local-name()='element']/*[local-name()='sub-elementB']/@attribute", "B" },
            { "//*[local-name()='ns-element']", "namespace" },
            { "//*[.='search-for']", "search-for" }
        };


        // Act
        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert
        receiveAction.Execute(Context);

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);
    }

    [Test]
    public void TestUsingXPathWithExplicitNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root xmlns='http://test' xmlns:ns1='http://agenix'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "<ns1:ns-element>namespace</ns1:ns-element>" +
            "<search-element>search-for</search-element>" +
            "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            { "//def:element/def:sub-elementA", "text-value" }, { "//ns1:ns-element", "namespace" }
        };

        // Act
        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .NamespaceContext("def", "http://test") // Map default namespace to prefix
            .NamespaceContext("ns1", "http://agenix")
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert
        receiveAction.Execute(Context);

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);
    }

    [Test]
    public void TestValidateMessageElementsUsingXPathWithResultTypes()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<ns1:root xmlns='http://test' xmlns:ns1='http://agenix'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "<ns1:ns-element>namespace</ns1:ns-element>" +
            "<search-element>search-for</search-element>" +
            "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            // Node result type validations
            { "node://def:element/def:sub-elementA", "text-value" },
            { "number:count(/ns1:root/def:element/*)", "3" },
            { "string:local-name(/ns1:root)", "root" },
            { "boolean://def:element", "True" },
            { "node://ns1:ns-element", "namespace" },
            { "node://*[.='search-for']", "search-for" },

            // String result type validations
            { "string:concat(/ns1:root/ns1:ns-element, ' is the value')", "namespace is the value" },
            { "string:local-name(/*)", "root" },
            { "string:namespace-uri(/*)", "http://agenix" },

            // Boolean result type validations
            { "boolean:contains(/ns1:root/def:search-element, 'search')", "True" },
            { "boolean:/ns1:root/def:element", "True" },
            { "boolean:/ns1:root/def:element-does-not-exist", "False" }
        };

        // Act
        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .NamespaceContext("def", "http://test") // Map default namespace
            .NamespaceContext("ns1", "http://agenix") // Map explicit namespace
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert
        receiveAction.Execute(Context);

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);
    }

    [Test]
        public void TestExtractMessageValuesUsingXPathWithExplicitNamespaceContext()
        {
            // Arrange
            _endpoint.Reset();
            _consumer.Reset();
            _endpointConfiguration.Reset();

            _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
            _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
            _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

            // Complex XML with multiple namespaces
            var message = new DefaultMessage(
                "<ns1:root xmlns='http://test' xmlns:ns1='http://agenix' xmlns:order='http://test/order'>" +
                    "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                        "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                        "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                        "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                    "</element>" +
                    "<ns1:ns-element>namespace</ns1:ns-element>" +
                    "<search-element>search-for</search-element>" +
                    "<order:product xmlns:order='http://test/order'>" +
                        "<order:name>Widget</order:name>" +
                        "<order:price currency='USD'>19.99</order:price>" +
                    "</order:product>" +
                "</ns1:root>");

            _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
                     .Returns(message);

            // Map XPath expressions with explicit namespace prefixes to variable names
            var extractMessageElements = new Dictionary<string, object>
            {
                // Node extractions using explicit namespace prefixes
                { "node://def:element/def:sub-elementA", "defaultNsElementA" },
                { "node://def:element/def:sub-elementA/@attribute", "defaultNsElementAttribute" },
                { "node://def:element/def:sub-elementB", "defaultNsElementB" },
                { "node://def:element/def:sub-elementB/@attribute", "defaultNsElementBAttribute" },
                { "node://ns1:ns-element", "namespacedElement" },
                { "node://def:search-element", "searchElement" },

                // Product information with order namespace
                { "node://ord:product/ord:name", "productName" },
                { "node://ord:product/ord:price", "productPrice" },
                { "node://ord:product/ord:price/@currency", "productCurrency" },

                // Number result type validations
                { "number:count(/ns1:root/def:element/*)", "childElementCount" },
                { "number:count(//ord:product)", "productCount" },

                // String result type validations
                { "string:concat(/ns1:root/ns1:ns-element, ' processed')", "concatenatedValue" },
                { "string:local-name(/ns1:root)", "rootLocalName" },
                { "string:namespace-uri(/ns1:root)", "rootNamespaceUri" },
                { "string:concat(//ord:name, ' costs ', //ord:price, ' ', //ord:price/@currency)", "productSummary" },

                // Boolean result type validations
                { "boolean:contains(/ns1:root/def:search-element, 'search')", "containsSearchText" },
                { "boolean://ns1:root/def:element", "elementExists" },
                { "boolean://ord:product", "productExists" },
                { "boolean://ns1:root/def:element-does-not-exist", "nonExistentElementCheck" }
            };

            // Create a variable extractor with explicit namespace context
            var variableExtractor = new XpathPayloadVariableExtractor.Builder()
                .Expressions(extractMessageElements)
                .Namespace("def", "http://test")           // Map default namespace to 'def' prefix
                .Namespace("ns1", "http://agenix")         // Map explicit namespace
                .Namespace("ord", "http://test/order")     // Map order namespace to 'ord' prefix
                .Build();

            // Act
            var controlMessageBuilder = new DefaultMessageBuilder();
            var validationContext = new XmlMessageValidationContext.Builder()
                .SchemaValidation(false)
                .Build();

            var receiveAction = new ReceiveMessageAction.Builder()
                .Endpoint(_endpoint.Object)
                .Message(controlMessageBuilder)
                .Validate(validationContext)
                .Process(variableExtractor)
                .Build();

            receiveAction.Execute(Context);

            // Assert - Verify all extracted variables from default namespace elements
            Assert.That(Context.GetVariable("defaultNsElementA"), Is.Not.Null);
            Assert.That(Context.GetVariable("defaultNsElementA"), Is.EqualTo("text-value"));

            Assert.That(Context.GetVariable("defaultNsElementAttribute"), Is.Not.Null);
            Assert.That(Context.GetVariable("defaultNsElementAttribute"), Is.EqualTo("A"));

            Assert.That(Context.GetVariable("defaultNsElementB"), Is.Not.Null);
            Assert.That(Context.GetVariable("defaultNsElementB"), Is.EqualTo("text-value"));

            Assert.That(Context.GetVariable("defaultNsElementBAttribute"), Is.Not.Null);
            Assert.That(Context.GetVariable("defaultNsElementBAttribute"), Is.EqualTo("B"));

            // Verify namespaced elements
            Assert.That(Context.GetVariable("namespacedElement"), Is.Not.Null);
            Assert.That(Context.GetVariable("namespacedElement"), Is.EqualTo("namespace"));

            Assert.That(Context.GetVariable("searchElement"), Is.Not.Null);
            Assert.That(Context.GetVariable("searchElement"), Is.EqualTo("search-for"));

            // Verify product information with order namespace
            Assert.That(Context.GetVariable("productName"), Is.Not.Null);
            Assert.That(Context.GetVariable("productName"), Is.EqualTo("Widget"));

            Assert.That(Context.GetVariable("productPrice"), Is.Not.Null);
            Assert.That(Context.GetVariable("productPrice"), Is.EqualTo("19.99"));

            Assert.That(Context.GetVariable("productCurrency"), Is.Not.Null);
            Assert.That(Context.GetVariable("productCurrency"), Is.EqualTo("USD"));

            // Verify numeric calculations
            Assert.That(Context.GetVariable("childElementCount"), Is.Not.Null);
            Assert.That(Context.GetVariable("childElementCount"), Is.EqualTo("3"));

            Assert.That(Context.GetVariable("productCount"), Is.Not.Null);
            Assert.That(Context.GetVariable("productCount"), Is.EqualTo("1"));

            // Verify string operations
            Assert.That(Context.GetVariable("concatenatedValue"), Is.Not.Null);
            Assert.That(Context.GetVariable("concatenatedValue"), Is.EqualTo("namespace processed"));

            Assert.That(Context.GetVariable("rootLocalName"), Is.Not.Null);
            Assert.That(Context.GetVariable("rootLocalName"), Is.EqualTo("root"));

            Assert.That(Context.GetVariable("rootNamespaceUri"), Is.Not.Null);
            Assert.That(Context.GetVariable("rootNamespaceUri"), Is.EqualTo("http://agenix"));

            Assert.That(Context.GetVariable("productSummary"), Is.Not.Null);
            Assert.That(Context.GetVariable("productSummary"), Is.EqualTo("Widget costs 19.99 USD"));

            // Verify boolean operations
            Assert.That(Context.GetVariable("containsSearchText"), Is.Not.Null);
            Assert.That(Context.GetVariable("containsSearchText"), Is.EqualTo("True"));

            Assert.That(Context.GetVariable("elementExists"), Is.Not.Null);
            Assert.That(Context.GetVariable("elementExists"), Is.EqualTo("True"));

            Assert.That(Context.GetVariable("productExists"), Is.Not.Null);
            Assert.That(Context.GetVariable("productExists"), Is.EqualTo("True"));

            Assert.That(Context.GetVariable("nonExistentElementCheck"), Is.Not.Null);
            Assert.That(Context.GetVariable("nonExistentElementCheck"), Is.EqualTo("False"));
        }

        [Test]
        public void TestExtractMessageValuesWithDynamicNamespaceMapping()
        {
            // Arrange
            _endpoint.Reset();
            _consumer.Reset();
            _endpointConfiguration.Reset();

            _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
            _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
            _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

            // Set up test variables for dynamic namespace mapping
            Context.SetVariable("defaultNamespace", "http://test");
            Context.SetVariable("agenixNamespace", "http://agenix");

            var message = new DefaultMessage(
                "<ns1:root xmlns='http://test' xmlns:ns1='http://agenix'>" +
                    "<element version='1.0'>" +
                        "<sub-elementA>dynamic-value</sub-elementA>" +
                    "</element>" +
                    "<ns1:metadata>dynamic-metadata</ns1:metadata>" +
                "</ns1:root>");

            _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
                     .Returns(message);

            var extractMessageElements = new Dictionary<string, object>
            {
                { "node://def:element/def:sub-elementA", "dynamicElement" },
                { "node://def:element/@version", "elementVersion" },
                { "node://cit:metadata", "dynamicMetadata" },
                { "string:concat('Extracted: ', //def:element/def:sub-elementA)", "dynamicConcat" }
            };

            // Build namespace mappings using variables
            var namespaceMap = new Dictionary<string, string>
            {
                { "def", Context.GetVariable("defaultNamespace") as string },
                { "cit", Context.GetVariable("agenixNamespace") as string }
            };

            var variableExtractor = new XpathPayloadVariableExtractor.Builder()
                .Expressions(extractMessageElements)
                .Namespaces(namespaceMap)
                .Build();

            // Act
            var controlMessageBuilder = new DefaultMessageBuilder();
            var validationContext = new XmlMessageValidationContext.Builder()
                .SchemaValidation(false)
                .Build();

            var receiveAction = new ReceiveMessageAction.Builder()
                .Endpoint(_endpoint.Object)
                .Message(controlMessageBuilder)
                .Validate(validationContext)
                .Process(variableExtractor)
                .Build();

            receiveAction.Execute(Context);

            // Assert
            Assert.That(Context.GetVariable("dynamicElement"), Is.Not.Null);
            Assert.That(Context.GetVariable("dynamicElement"), Is.EqualTo("dynamic-value"));

            Assert.That(Context.GetVariable("elementVersion"), Is.Not.Null);
            Assert.That(Context.GetVariable("elementVersion"), Is.EqualTo("1.0"));

            Assert.That(Context.GetVariable("dynamicMetadata"), Is.Not.Null);
            Assert.That(Context.GetVariable("dynamicMetadata"), Is.EqualTo("dynamic-metadata"));

            Assert.That(Context.GetVariable("dynamicConcat"), Is.Not.Null);
            Assert.That(Context.GetVariable("dynamicConcat"), Is.EqualTo("Extracted: dynamic-value"));
        }

        [Test]
        public void TestExtractMessageValuesWithMixedNamespaceApproaches()
        {
            // Arrange
            _endpoint.Reset();
            _consumer.Reset();
            _endpointConfiguration.Reset();

            _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
            _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
            _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

            var message = new DefaultMessage(
                "<ns1:root xmlns='http://test' xmlns:ns1='http://agenix'>" +
                    "<element>" +
                        "<sub-elementA>value-a</sub-elementA>" +
                    "</element>" +
                    "<ns1:ns-element>namespace-value</ns1:ns-element>" +
                "</ns1:root>");

            _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
                     .Returns(message);

            // Mix of explicit namespace prefixes and dot notation
            var extractMessageElements = new Dictionary<string, object>
            {
                // Using explicit namespace prefix
                { "node://def:element/def:sub-elementA", "explicitNsValue" },

                // Using dot notation (namespace-agnostic)
                { "//*[local-name()='element']/*[local-name()='sub-elementA']", "dotNotationValue" },

                // Using declared namespace prefix
                { "node://ns1:ns-element", "declaredNsValue" },

                // Mixed approaches in functions
                { "string:concat(//def:element/def:sub-elementA, ' + ', //ns1:ns-element)", "mixedConcat" }
            };

            var variableExtractor = new XpathPayloadVariableExtractor.Builder()
                .Expressions(extractMessageElements)
                .Namespace("def", "http://test")     // Map default namespace
                .Namespace("ns1", "http://agenix")   // Explicit mapping (though already declared)
                .Build();

            // Act
            var controlMessageBuilder = new DefaultMessageBuilder();
            var validationContext = new XmlMessageValidationContext.Builder()
                .SchemaValidation(false)
                .Build();

            var receiveAction = new ReceiveMessageAction.Builder()
                .Endpoint(_endpoint.Object)
                .Message(controlMessageBuilder)
                .Validate(validationContext)
                .Process(variableExtractor)
                .Build();

            receiveAction.Execute(Context);

            // Assert
            Assert.That(Context.GetVariable("explicitNsValue"), Is.Not.Null);
            Assert.That(Context.GetVariable("explicitNsValue"), Is.EqualTo("value-a"));

            Assert.That(Context.GetVariable("dotNotationValue"), Is.Not.Null);
            Assert.That(Context.GetVariable("dotNotationValue"), Is.EqualTo("value-a"));

            Assert.That(Context.GetVariable("declaredNsValue"), Is.Not.Null);
            Assert.That(Context.GetVariable("declaredNsValue"), Is.EqualTo("namespace-value"));

            Assert.That(Context.GetVariable("mixedConcat"), Is.Not.Null);
            Assert.That(Context.GetVariable("mixedConcat"), Is.EqualTo("value-a + namespace-value"));
        }

}
