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

using System.Reflection;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using Agenix.Validation.Xml.Validation.Matcher.Core;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Validation.Matcher;

public class XmlValidationMatcherTests
{
    private Mock<IMessageValidator<IValidationContext>> _mockMessageValidator;
    private Mock<MessageValidatorRegistry> _mockMessageValidatorRegistry;
    private Mock<IReferenceResolver> _mockReferenceResolver;
    private Mock<TestContext> _mockTestContext;
    private XmlValidationMatcher _xmlValidationMatcher;

    [SetUp]
    public void SetUp()
    {
        _mockTestContext = new Mock<TestContext>();
        _mockMessageValidatorRegistry = new Mock<MessageValidatorRegistry>();
        _mockReferenceResolver = new Mock<IReferenceResolver>();
        _mockMessageValidator = new Mock<IMessageValidator<IValidationContext>>();

        _mockTestContext.Setup(x => x.MessageValidatorRegistry).Returns(_mockMessageValidatorRegistry.Object);
        _mockTestContext.Setup(x => x.ReferenceResolver).Returns(_mockReferenceResolver.Object);

        _xmlValidationMatcher = new XmlValidationMatcher();
    }

    [Test]
    public void Validate_WithValidParameters_CallsValidateMessage()
    {
        // Arrange
        const string fieldName = "testField";
        const string value = "<root><element>test</element></root>";
        var controlParameters = new List<string> { "<root><element>expected</element></root>" };

        _mockMessageValidatorRegistry
            .Setup(x => x.FindMessageValidator(XmlValidationMatcher.DefaultXmlMessageValidator))
            .Returns(Optional<IMessageValidator<IValidationContext>>.Of(_mockMessageValidator.Object));

        // Act
        _xmlValidationMatcher.Validate(fieldName, value, controlParameters, _mockTestContext.Object);

        // Assert
        _mockMessageValidator.Verify(x => x.ValidateMessage(
                It.IsAny<DefaultMessage>(),
                It.IsAny<DefaultMessage>(),
                _mockTestContext.Object,
                It.IsAny<List<IValidationContext>>())
            , Times.Once);
    }

    [Test]
    public void Validate_WithCDataValue_RemovesCDataBeforeValidation()
    {
        // Arrange
        var fieldName = "testField";
        var value = "<![CDATA[<root><element>test</element></root>]]>";
        var controlParameters = new List<string> { "<root><element>expected</element></root>" };
        var expectedCleanedValue = "<root><element>test</element></root>";

        _mockMessageValidatorRegistry
            .Setup(x => x.FindMessageValidator(XmlValidationMatcher.DefaultXmlMessageValidator))
            .Returns(Optional<IMessageValidator<IValidationContext>>.Of(_mockMessageValidator.Object));

        // Act
        _xmlValidationMatcher.Validate(fieldName, value, controlParameters, _mockTestContext.Object);

        // Assert
        _mockMessageValidator.Verify(x => x.ValidateMessage(
            It.Is<DefaultMessage>(m => m.Payload.ToString() == expectedCleanedValue),
            It.IsAny<DefaultMessage>(),
            _mockTestContext.Object,
            It.IsAny<List<IValidationContext>>()), Times.Once);
    }

    [Test]
    public void GetMessageValidator_WhenValidatorCached_ReturnsCachedValidator()
    {
        // Arrange
        _mockMessageValidatorRegistry
            .Setup(x => x.FindMessageValidator(XmlValidationMatcher.DefaultXmlMessageValidator))
            .Returns(Optional<IMessageValidator<IValidationContext>>.Of(_mockMessageValidator.Object));

        // First call to cache the validator
        _xmlValidationMatcher.Validate("field", "value", new List<string> { "control" }, _mockTestContext.Object);

        // Act - Second call should use cached validator
        _xmlValidationMatcher.Validate("field2", "value2", new List<string> { "control2" }, _mockTestContext.Object);

        // Assert - Registry should only be called once
        _mockMessageValidatorRegistry.Verify(x => x.FindMessageValidator(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void GetMessageValidator_WhenFoundInRegistry_ReturnsValidator()
    {
        // Arrange
        _mockMessageValidatorRegistry
            .Setup(x => x.FindMessageValidator(XmlValidationMatcher.DefaultXmlMessageValidator))
            .Returns(Optional<IMessageValidator<IValidationContext>>.Of(_mockMessageValidator.Object));

        // Act
        _xmlValidationMatcher.Validate("field", "value", new List<string> { "control" }, _mockTestContext.Object);

        // Assert
        _mockMessageValidator.Verify(x => x.ValidateMessage(
            It.IsAny<DefaultMessage>(),
            It.IsAny<DefaultMessage>(),
            _mockTestContext.Object,
            It.IsAny<List<IValidationContext>>()), Times.Once);
    }

    [Test]
    public void GetMessageValidator_WhenNotInRegistryButInResolver_ReturnsValidator()
    {
        // Arrange
        _mockMessageValidatorRegistry
            .Setup(x => x.FindMessageValidator(XmlValidationMatcher.DefaultXmlMessageValidator))
            .Returns(Optional<IMessageValidator<IValidationContext>>.Empty);

        _mockReferenceResolver
            .Setup(x =>
                x.Resolve<IMessageValidator<IValidationContext>>(XmlValidationMatcher.DefaultXmlMessageValidator))
            .Returns(_mockMessageValidator.Object);

        // Act
        _xmlValidationMatcher.Validate("field", "value", new List<string> { "control" }, _mockTestContext.Object);

        // Assert
        _mockMessageValidator.Verify(x => x.ValidateMessage(
            It.IsAny<DefaultMessage>(),
            It.IsAny<DefaultMessage>(),
            _mockTestContext.Object,
            It.IsAny<List<IValidationContext>>()), Times.Once);
    }


    [TestCase("<![CDATA[<root>test</root>]]>", "<root>test</root>")]
    [TestCase("  <![CDATA[<root>test</root>]]>  ", "<root>test</root>")]
    [TestCase("<root>test</root>", "<root>test</root>")]
    [TestCase("  <root>test</root>  ", "<root>test</root>")]
    [TestCase("<![CDATA[]]>", "")]
    [TestCase("<![CDATA[simple text]]>", "simple text")]
    public void RemoveCDataElements_WithVariousInputs_ReturnsExpectedResult(string input, string expected)
    {
        // Act - Using reflection to access private method
        var method = typeof(XmlValidationMatcher).GetMethod("RemoveCDataElements",
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (string)method.Invoke(null, new object[] { input });

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void RemoveCDataElements_WithNullInput_ThrowsException()
    {
        // Act & Assert
        var method = typeof(XmlValidationMatcher).GetMethod("RemoveCDataElements",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(null, new object[] { null }));
    }

    [Test]
    public void RemoveCDataElements_WithOnlyCDataStart_ReturnsOriginalTrimmed()
    {
        // Arrange
        var input = "<![CDATA[incomplete";
        var expected = "<![CDATA[incomplete";

        // Act
        var method = typeof(XmlValidationMatcher).GetMethod("RemoveCDataElements",
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (string)method.Invoke(null, [input]);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void RemoveCDataElements_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        const string input = "";
        const string expected = "";

        // Act
        var method = typeof(XmlValidationMatcher).GetMethod("RemoveCDataElements",
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (string)method.Invoke(null, [input]);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void RemoveCDataElements_WithWhitespaceOnly_ReturnsEmptyString()
    {
        // Arrange
        const string input = "   ";
        const string expected = "";

        // Act
        var method = typeof(XmlValidationMatcher).GetMethod("RemoveCDataElements",
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (string)method.Invoke(null, [input]);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Constants_HaveCorrectValues()
    {
        // Assert
        Assert.That(XmlValidationMatcher.DefaultXmlMessageValidator, Is.EqualTo("defaultXmlMessageValidator"));
    }

    [Test]
    public void Validate_WithEmptyControlParameters_ThrowsException()
    {
        // Arrange
        const string fieldName = "testField";
        const string value = "<root>test</root>";
        var controlParameters = new List<string>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _xmlValidationMatcher.Validate(fieldName, value, controlParameters, _mockTestContext.Object));
    }

    [Test]
    public void Validate_WithNullControlParameters_ThrowsException()
    {
        // Arrange
        const string fieldName = "testField";
        const string value = "<root>test</root>";
        List<string> controlParameters = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            _xmlValidationMatcher.Validate(fieldName, value, controlParameters, _mockTestContext.Object));
    }

    [Test]
    public void Validate_WithNullContext_ThrowsException()
    {
        // Arrange
        const string fieldName = "testField";
        const string value = "<root>test</root>";
        var controlParameters = new List<string> { "<root>expected</root>" };

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            _xmlValidationMatcher.Validate(fieldName, value, controlParameters, null));
    }
}
