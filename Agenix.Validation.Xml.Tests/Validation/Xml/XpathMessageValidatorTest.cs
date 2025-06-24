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

using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;
using NHamcrest;
using NUnit.Framework;
using Has = NHamcrest.Has;
using Is = NHamcrest.Is;
using NUnitIIs = NUnit.Framework.Is;


namespace Agenix.Validation.Xml.Tests.Validation.Xml;

public class XpathMessageValidatorTest : AbstractNUnitSetUp
{
    private readonly XpathMessageValidator _validator = new();

    [Test]
    public void TestValidateMessageElementsWithXPathSuccessful()
    {
        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value'>"
                                         + "<sub-element attribute='A'>text-value</sub-element>"
                                         + "</element>"
                                         + "</root>");

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expression("//element/sub-element", "text-value")
            .Build();

        _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext);
    }

    [Test]
    public void TestValidateMessageElementsWithValidationMatcherSuccessful()
    {
        // Arrange
        const string xmlContent = "<root>" +
                                  "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                  "<sub-element attribute='A'>text-value</sub-element>" +
                                  "</element>" +
                                  "</root>";

        var message = new DefaultMessage(xmlContent);

        var validationExpressions = new Dictionary<string, object>
        {
            { "//element/@attributeA", "@StartsWith('attribute-')@" },
            { "//element/@attributeB", "@EndsWith('-value')@" },
            { "//element/sub-element", "@EqualsIgnoreCase('TEXT-VALUE')@" }
        };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validationExpressions)
            .Build();

        // Act & Assert
        _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext);
    }

    [Test]
    public void TestValidateMessageElementsWithValidationMatcherNotSuccessful()
    {
        // Arrange
        const string xmlContent = "<root>" +
                                  "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                  "<sub-element attribute='A'>text-value</sub-element>" +
                                  "</element>" +
                                  "</root>";

        var message = new DefaultMessage(xmlContent);

        var validationExpressions = new Dictionary<string, object>
        {
            { "//element/@attributeA", "@StartsWith('attribute-')@" },
            { "//element/@attributeB", "@EndsWith('-value')@" },
            { "//element/sub-element", "@Contains('FAIL')@" }
        };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validationExpressions)
            .Build();

        // Act & Assert - Expecting ValidationException to be thrown
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));
    }

    [Test]
    public void TestValidateMessageElementsWithXPathNotSuccessful()
    {
        // Arrange
        const string xmlContent = "<root>" +
                                  "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                  "<sub-element attribute='A'>text-value</sub-element>" +
                                  "</element>" +
                                  "</root>";

        var message = new DefaultMessage(xmlContent);

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expression("//element/sub-element", "false-value")
            .Build();

        // Act & Assert - Expecting ValidationException to be thrown
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));
    }

    [Test]
    public void TestValidateMessageElementsWithDotNotationSuccessful()
    {
        // Arrange
        var xmlContent = "<root>" +
                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                         "<sub-element attribute='A'>text-value</sub-element>" +
                         "</element>" +
                         "</root>";

        var message = new DefaultMessage(xmlContent);

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expression("root.element.sub-element", "text-value")
            .Build();

        // Act & Assert - This should not throw any exception
        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));
    }

    [Test]
    public void TestValidateMessageElementsWithDotNotationValidationMatcherSuccessful()
    {
        // Arrange
        const string xmlContent = "<root>" +
                                  "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                  "<sub-element attribute='A'>text-value</sub-element>" +
                                  "</element>" +
                                  "</root>";

        var message = new DefaultMessage(xmlContent);

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expression("root.element.sub-element", "@Contains('ext-val')@")
            .Build();

        // Act & Assert - This should not throw any exception
        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));
    }

    [Test]
    public void TestValidateMessageElementsWithDotNotationValidationMatcherNotSuccessful()
    {
        // Arrange
        var xmlContent = "<root>" +
                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                         "<sub-element attribute='A'>text-value</sub-element>" +
                         "</element>" +
                         "</root>";

        var message = new DefaultMessage(xmlContent);

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expression("root.element.sub-element", "@Contains(false-value)@")
            .Build();

        // Act & Assert - Expecting ValidationException to be thrown
        Assert.That(() => _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void TestValidateMessageElementsWithDotNotationNotSuccessful()
    {
        // Arrange
        const string xmlContent = "<root>" +
                                  "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                  "<sub-element attribute='A'>text-value</sub-element>" +
                                  "</element>" +
                                  "</root>";

        var message = new DefaultMessage(xmlContent);

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expression("root.element.sub-element", "false-value")
            .Build();

        // Act & Assert - Expecting ValidationException to be thrown
        Assert.That(() => _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void TestValidateMessageElementsWithMixedNotationsSuccessful()
    {
        // Arrange
        const string xmlContent = "<root>" +
                                  "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                  "<sub-element attribute='A'>text-value</sub-element>" +
                                  "</element>" +
                                  "</root>";

        var message = new DefaultMessage(xmlContent);

        // Mix of xpath and dot-notation
        var validationExpressions = new Dictionary<string, object>
        {
            { "//element/sub-element", "text-value" }, { "root.element.sub-element", "text-value" }
        };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validationExpressions)
            .Build();

        // Act & Assert - This should not throw any exception
        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));
    }

    [Test]
    public void TestValidateMessageElementsWithNodeListResult()
    {
        // Arrange
        var xmlContent = "<root>" +
                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                         "<sub-element attribute='A'>text-value</sub-element>" +
                         "<sub-element attribute='B'>other-value</sub-element>" +
                         "</element>" +
                         "</root>";

        var message = new DefaultMessage(xmlContent);

        // Test 1: Comma-separated values validation
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expression("node-set://element/sub-element", "text-value,other-value")
            .Build();

        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));

        // Test 2: Hamcrest matcher - size greater than 1 and not empty
        validationContext = new XpathMessageValidationContext.Builder()
            .Expression("node-set://element/sub-element", Matches.AllOf(Is.OfLength(2), Is.Not(Is.Null())))
            .Build();

        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));

        // Test 3: Array format with spaces
        validationContext = new XpathMessageValidationContext.Builder()
            .Expression("node-set://element/sub-element", "[text-value, other-value]")
            .Build();

        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));

        // Test 4: Array format without spaces
        validationContext = new XpathMessageValidationContext.Builder()
            .Expression("node-set://element/sub-element", "[text-value,other-value]")
            .Build();

        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));

        // Test 5: Attribute values array
        validationContext = new XpathMessageValidationContext.Builder()
            .Expression("node-set://@attribute", "[A, B]")
            .Build();

        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));

        // Test 6: Size validation
        validationContext = new XpathMessageValidationContext.Builder()
            .Expression("node-set://@attribute", Is.OfLength(2))
            .Build();

        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));

        // Test 7: Contains validation
        validationContext = new XpathMessageValidationContext.Builder()
            .Expression("node-set://@attribute", Matches.AllOf(Has.Item(Is.EqualTo("A")), Has.Item(Is.EqualTo("B"))
            ))
            .Build();

        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));
    }

    [Test]
    public void TestValidateMessageElementsWithNodeListResultNoMatch()
    {
        // Arrange
        const string xmlContent = "<root>" +
                                  "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                  "<sub-element attribute='A'>text-value</sub-element>" +
                                  "<sub-element attribute='B'>other-value</sub-element>" +
                                  "</element>" +
                                  "</root>";
        var message = new DefaultMessage(xmlContent);
        var expressions = new Dictionary<string, object>
        {
            ["node-set://element/other-element"] = "",
            ["boolean://element/other-element"] = "False",
            ["boolean://element/sub-element"] = "True"
        };
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(expressions)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));
    }

    [Test]
    public void TestValidateMessageElementsWithNodeListCount()
    {
        // Arrange
        const string xmlContent = "<root>" +
                                  "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                  "<sub-element attribute='A'>text-value</sub-element>" +
                                  "<sub-element attribute='B'>text-value</sub-element>" +
                                  "</element>" +
                                  "</root>";

        var message = new DefaultMessage(xmlContent);

        var expressions = new Dictionary<string, object>
        {
            ["number:count(//element/sub-element[.='text-value'])"] = "2",
            ["integer:count(//element/sub-element[.='text-value'])"] = "2",
            ["number:count(//element/sub-element)"] = Is.GreaterThan(1.0),
            ["integer:count(//element/sub-element)"] = Is.GreaterThan(1)
        };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(expressions)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() =>
            _validator.ValidateMessage(message, new DefaultMessage(), Context, validationContext));
    }

    [Test]
    public void ShouldFindProperValidationContext()
    {
        // Arrange
        var validationContexts = new List<IValidationContext>
        {
            new HeaderValidationContext(), new XmlMessageValidationContext()
        };

        // Act & Assert - should return null when no XpathMessageValidationContext is present
        Assert.That(_validator.FindValidationContext(validationContexts), NUnitIIs.Null);

        // Add XpathMessageValidationContext
        validationContexts.Add(new XpathMessageValidationContext());

        // Act & Assert - should return non-null when XpathMessageValidationContext is present
        Assert.That(_validator.FindValidationContext(validationContexts), NUnitIIs.Not.Null);
    }
}
