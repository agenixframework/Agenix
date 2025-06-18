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
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation.Json;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Schema;
using Agenix.Validation.Xml.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml.Schema;
using Moq;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Validation.Xml;

public class DomXmlMessageValidatorTests : AbstractNUnitSetUp
{
    private readonly XsdSchemaRepository schemaRepository = new();

    private readonly SimpleXsdSchema testSchema = new();
    private readonly SimpleXsdSchema testSchema1 = new();
    private readonly SimpleXsdSchema testSchema2 = new();
    private readonly SimpleXsdSchema testSchema3 = new();
    private readonly XsdSchemaRepository testSchemaRepository1 = new();
    private readonly XsdSchemaRepository testSchemaRepository2 = new();
    private readonly DomXmlMessageValidator validator = new();

    [Test]
    public void ConstructorWithoutArguments()
    {
        var xmlSchemaValidation = ReflectionUtils.GetInstanceFieldValue(validator, "_schemaValidation");

        Assert.That(xmlSchemaValidation, Is.InstanceOf<XmlSchemaValidation>());
        Assert.That(xmlSchemaValidation, Is.Not.Null);
    }

    [Test]
    public void AllArgsConstructor()
    {
        var xmlSchemaValidationMock = Mock.Of<XmlSchemaValidation>();
        var domXmlMessageValidator = new DomXmlMessageValidator(xmlSchemaValidationMock);

        var xmlSchemaValidation = ReflectionUtils.GetInstanceFieldValue(domXmlMessageValidator, "_schemaValidation");

        Assert.That(xmlSchemaValidation, Is.InstanceOf<XmlSchemaValidation>());
        Assert.That(xmlSchemaValidation, Is.Not.Null);
        Assert.That(xmlSchemaValidation, Is.EqualTo(xmlSchemaValidationMock));
    }

    [Test]
    public void ValidateXmlSchema()
    {
        var message = new DefaultMessage("<message xmlns='http://agenix.org/test'>" +
                                         "<correlationId>Kx1R123456789</correlationId>" +
                                         "<bookingId>Bx1G987654321</bookingId>" +
                                         "<test>Hello TestFramework</test>" +
                                         "</message>");

        var validator = new DomXmlMessageValidator();
        var schemaResource = FileUtils.GetFileResource(
            "assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Validation/test.xsd");
        var schema = new SimpleXsdSchema(schemaResource);
        schema.AfterPropertiesSet();

        Context.ReferenceResolver.Bind("schema", schema);
        var xmlMessageValidation = new XmlMessageValidationContext.Builder().Schema("schema").Build();

        validator.ValidateXmlSchema(message, Context, xmlMessageValidation);
    }

    [Test]
    public void TestExpectDefaultNamespace()
    {
        var message = new DefaultMessage("<root xmlns='http://agenix.org/test'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        var expectedNamespaces = new Dictionary<string, string> { { "", "http://agenix.org/test" } };

        var validator = new DomXmlMessageValidator();
        validator.ValidateNamespaces(expectedNamespaces, message);
    }

    [Test]
    public void TestExpectNamespace()
    {
        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix.org/ns1'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        var expectedNamespaces = new Dictionary<string, string> { { "ns1", "http://agenix.org/ns1" } };

        var validator = new DomXmlMessageValidator();
        validator.ValidateNamespaces(expectedNamespaces, message);
    }

    [Test]
    public void TestExpectMixedNamespaces()
    {
        var message = new DefaultMessage("<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix.org/default" }, { "ns1", "http://agenix.org/ns1" }
        };

        var validator = new DomXmlMessageValidator();
        validator.ValidateNamespaces(expectedNamespaces, message);
    }

    [Test]
    public void TestExpectMultipleNamespaces()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix.org/default" },
            { "ns1", "http://agenix.org/ns1" },
            { "ns2", "http://agenix.org/ns2" }
        };

        var validator = new DomXmlMessageValidator();
        validator.ValidateNamespaces(expectedNamespaces, message);
    }

    [Test]
    public void TestExpectDefaultNamespaceError()
    {
        var message = new DefaultMessage("<root xmlns='http://agenix.org'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        var expectedNamespaces = new Dictionary<string, string> { { "", "http://agenix.org/wrong" } };

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateNamespaces(expectedNamespaces, message));
    }

    [Test]
    public void TestExpectNamespaceError()
    {
        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix.org/ns1'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        var expectedNamespaces = new Dictionary<string, string> { { "ns1", "http://agenix.org/ns1/wrong" } };

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateNamespaces(expectedNamespaces, message));
    }

    [Test]
    public void TestExpectMixedNamespacesError()
    {
        var message = new DefaultMessage("<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix.org/default/wrong" }, { "ns1", "http://agenix.org/ns1" }
        };

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateNamespaces(expectedNamespaces, message));
    }

    [Test]
    public void TestExpectMultipleNamespacesError()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix.org/default" },
            { "ns1", "http://agenix.org/ns1/wrong" },
            { "ns2", "http://agenix.org/ns2" }
        };

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateNamespaces(expectedNamespaces, message));
    }

    [Test]
    public void TestExpectWrongNamespacePrefix()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix.org/default" },
            { "nswrong", "http://agenix.org/ns1" },
            { "ns2", "http://agenix.org/ns2" }
        };

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateNamespaces(expectedNamespaces, message));
    }

    [Test]
    public void TestExpectDefaultNamespaceButNamespace()
    {
        var message = new DefaultMessage(
            "<ns0:root xmlns:ns0='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2'>" +
            "<ns0:element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<ns0:sub-element attribute='A'>text-value</ns0:sub-element>" +
            "</ns0:element>" +
            "</ns0:root>");

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix.org/default" },
            { "ns1", "http://agenix.org/ns1" },
            { "ns2", "http://agenix.org/ns2" }
        };

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateNamespaces(expectedNamespaces, message));
    }

    [Test]
    public void TestExpectNamespaceButDefaultNamespace()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "ns0", "http://agenix.org/default" },
            { "ns1", "http://agenix.org/ns1" },
            { "ns2", "http://agenix.org/ns2" }
        };

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateNamespaces(expectedNamespaces, message));
    }

    [Test]
    public void TestExpectAdditionalNamespace()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix.org/default" },
            { "ns1", "http://agenix.org/ns1" },
            { "ns2", "http://agenix.org/ns2" },
            { "ns4", "http://agenix.org/ns4" }
        };

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateNamespaces(expectedNamespaces, message));
    }

    [Test]
    public void TestExpectNamespaceButNamespaceMissing()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:ns4='http://agenix.org/ns4'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix.org/default" },
            { "ns1", "http://agenix.org/ns1" },
            { "ns2", "http://agenix.org/ns2" }
        };

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateNamespaces(expectedNamespaces, message));
    }

    [Test]
    public void TestValidateMessagePayloadSuccess()
    {
        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        var controlMessage = new DefaultMessage("<root>" +
                                                "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                "<sub-element attribute='A'>text-value</sub-element>" +
                                                "</element>" +
                                                "</root>");

        var validationContext = new XmlMessageValidationContext();

        var validator = new DomXmlMessageValidator();
        validator.ValidateMessage(message, controlMessage, Context, validationContext);
    }

    [Test]
    public void TestValidateMessagePayloadWithIgnoresSuccess()
    {
        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element1 attribute='A'>THIS_IS_IGNORED_BY_XPATH</sub-element1>" +
                                         "<sub-element2 attribute='A'>THIS IS IGNORED BY IGNORE-EXPR</sub-element2>" +
                                         "<sub-element3 attribute='A'>a text</sub-element3>" +
                                         "</element>" +
                                         "</root>");

        var controlMessage = new DefaultMessage("<root>" +
                                                "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                "<sub-element1 attribute='A'>text-value</sub-element1>" +
                                                "<sub-element2 attribute='A'>@Ignore@</sub-element2>" +
                                                "<sub-element3 attribute='A'>a text</sub-element3>" +
                                                "</element>" +
                                                "</root>");

        var ignoreExpressions = new HashSet<string> { "//root/element/sub-element1" };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreExpressions)
            .Build();

        var validator = new DomXmlMessageValidator();
        validator.ValidateMessage(message, controlMessage, Context, validationContext);
    }

    [Test]
    public void TestValidateMessagePayloadWithValidationMatchersFailsBecauseOfAttribute()
    {
        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='text-attribute'>text-element</sub-element>" +
                                         "</element>" +
                                         "</root>");

        var controlMessage = new DefaultMessage("<root>" +
                                                "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                "<sub-element attribute='@StartsWith(FAIL)@'>@startsWith(text)@</sub-element>" +
                                                "</element>" +
                                                "</root>");

        var validationContext = new XmlMessageValidationContext();
        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateMessage(message, controlMessage, Context, validationContext));
    }

    [Test]
    public void TestValidateMessagePayloadWithValidationMatcherOnElementFails()
    {
        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='text-attribute'>text-element</sub-element>" +
                                         "</element>" +
                                         "</root>");

        var controlMessage = new DefaultMessage("<root>" +
                                                "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                "<sub-element attribute='text-attribute'>@StartsWith(FAIL)@</sub-element>" +
                                                "</element>" +
                                                "</root>");

        var validationContext = new XmlMessageValidationContext();
        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateMessage(message, controlMessage, Context, validationContext));
    }

    [Test]
    public void TestValidateMessagePayloadWithValidationMatcherOnAttributeFails()
    {
        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='text-attribute'>text-element</sub-element>" +
                                         "</element>" +
                                         "</root>");

        var controlMessage = new DefaultMessage("<root>" +
                                                "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                "<sub-element attribute='@StartsWith(FAIL)@'>text-element</sub-element>" +
                                                "</element>" +
                                                "</root>");

        var validationContext = new XmlMessageValidationContext();
        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateMessage(message, controlMessage, Context, validationContext));
    }

    [Test]
    public void TestNamespaceQualifiedAttributeValue()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var controlMessage = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var validator = new DomXmlMessageValidator();
        validator.ValidateMessage(message, controlMessage, Context, validationContext);
    }

    [Test]
    public void TestNamespaceQualifiedAttributeValueParentDeclaration()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xmlns:ns1='http://agenix.org/ns1' xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var controlMessage = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var validator = new DomXmlMessageValidator();
        validator.ValidateMessage(message, controlMessage, Context, validationContext);
    }

    [Test]
    public void TestNamespaceQualifiedAttributeValueParentDeclarationInSource()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xmlns:ns1='http://agenix.org/ns1' xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var controlMessage = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xmlns:ns1='http://agenix.org/ns1' xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var validator = new DomXmlMessageValidator();
        validator.ValidateMessage(message, controlMessage, Context, validationContext);
    }

    [Test]
    public void TestNamespaceQualifiedAttributeValueDifferentPrefix()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var controlMessage = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:cit='http://agenix.org/ns1' xmlns:cit2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='cit:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='cit2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var validator = new DomXmlMessageValidator();
        validator.ValidateMessage(message, controlMessage, Context, validationContext);
    }

    [Test]
    public void TestNamespaceQualifiedLikeAttributeValues()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element credentials='username:password' attributeB='attribute-value'>" +
            "<sub-element>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var controlMessage = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element credentials='username:password' attributeB='attribute-value'>" +
            "<sub-element>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var validator = new DomXmlMessageValidator();
        validator.ValidateMessage(message, controlMessage, Context, validationContext);
    }

    [Test]
    public void TestCommentBeforeRootElement()
    {
        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                         "<!-- some comment -->" +
                                         "<root>" +
                                         "<element>test</element>" +
                                         "</root>");

        var controlMessage = new DefaultMessage("<root>" +
                                                "<element>test</element>" +
                                                "</root>");

        var validationContext = new XmlMessageValidationContext();

        var validator = new DomXmlMessageValidator();
        validator.ValidateMessage(message, controlMessage, Context, validationContext);
    }

    [Test]
    public void TestComment()
    {
        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                         "<root>" +
                                         "<!-- some comment -->" +
                                         "<element>test</element>" +
                                         "</root>");

        var controlMessage = new DefaultMessage("<root>" +
                                                "<element>test</element>" +
                                                "</root>");

        var validationContext = new XmlMessageValidationContext();

        var validator = new DomXmlMessageValidator();
        validator.ValidateMessage(message, controlMessage, Context, validationContext);
    }

    [Test]
    public void TestNamespaceQualifiedAttributeValueFails()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var controlMessage = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='ns1:wrong-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateMessage(message, controlMessage, Context, validationContext));
    }

    [Test]
    public void TestNamespaceQualifiedAttributeValueUriMismatch()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var controlMessage = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:cit='http://agenix.org/cit' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='cit:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateMessage(message, controlMessage, Context, validationContext));
    }

    [Test]
    public void TestNamespaceQualifiedAttributeMissingPrefix()
    {
        var message = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/ns1' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var controlMessage = new DefaultMessage(
            "<root xmlns='http://agenix.org/default' xmlns:ns1='http://agenix.org/cit' xmlns:ns2='http://agenix.org/ns2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<element xsi:type='ns1:attribute-value' attributeB='attribute-value'>" +
            "<sub-element xsi:type='ns2:AType'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var validator = new DomXmlMessageValidator();

        Assert.Throws<ValidationException>(() =>
            validator.ValidateMessage(message, controlMessage, Context, validationContext));
    }

    [Test]
    public void ShouldFindProperValidationContext()
    {
        var validationContexts = new List<IValidationContext>();
        validationContexts.Add(new HeaderValidationContext());
        validationContexts.Add(new XmlMessageValidationContext());

        Assert.That(validator.FindValidationContext(validationContexts), Is.Not.Null);

        validationContexts.Clear();
        validationContexts.Add(new HeaderValidationContext());
        validationContexts.Add(new DefaultMessageValidationContext());

        Assert.That(validator.FindValidationContext(validationContexts), Is.Not.Null);

        validationContexts.Clear();
        validationContexts.Add(new JsonMessageValidationContext());

        Assert.That(validator.FindValidationContext(validationContexts), Is.Null);
    }

    [Test]
    public void TestLookup()
    {
        var validators = ISchemaValidator<ISchemaValidationContext>.Lookup();
        Assert.That(validators.Count, Is.EqualTo(1));
        Assert.That(validators["defaultXmlSchemaValidator"], Is.Not.Null);
        Assert.That(validators["defaultXmlSchemaValidator"].GetType(), Is.EqualTo(typeof(XmlSchemaValidation)));
    }

    [Test]
    public void TestTestLookup()
    {
        Assert.That(ISchemaValidator<ISchemaValidationContext>.Lookup("xml").IsPresent, Is.True);
    }
}
