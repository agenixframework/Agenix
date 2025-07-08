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

using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;
using Agenix.Selenium.Endpoint;
using OpenQA.Selenium;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Finds an element in a DOM tree on current page and validates its properties and settings.
///     Test action fails in case no element is found or the validation expectations are not met.
/// </summary>
public class FindElementAction : AbstractSeleniumAction
{
    /// <summary>
    ///     Default constructor
    /// </summary>
    public FindElementAction(Builder builder) : this("find", builder)
    {
        Attributes = builder.Attributes;
        Styles = builder.Styles;
        IsDisplayed = builder.Displayed;
        IsEnabled = builder.Enabled;
        Text = builder.Text;
        TagName = builder.TagName;
    }

    /// <summary>
    ///     Constructor with name
    /// </summary>
    protected FindElementAction(string name, Builder builder) : base(name, builder)
    {
        By = builder.By;
        Property = builder.Property;
        PropertyValue = builder.PropertyValue;
    }

    // Properties
    /// <summary>
    ///     Element selector property
    /// </summary>
    public string Property { get; }

    public string PropertyValue { get; }

    /// <summary>
    ///     Optional validation expectations on element
    /// </summary>
    public string TagName { get; }

    public Dictionary<string, string> Attributes { get; }

    public Dictionary<string, string> Styles { get; }

    public bool IsDisplayed { get; } = true;

    public bool IsEnabled { get; } = true;

    public string Text { get; }

    /// <summary>
    ///     Optional By used in fluent API
    /// </summary>
    public By? By { get; }

    /// <summary>
    /// Executes the find element action by locating the specified element on the page and processing it.
    /// </summary>
    /// <param name="browser">The <see cref="SeleniumBrowser"/> instance used to interact with the web browser.</param>
    /// <param name="context">The <see cref="TestContext"/> providing context-specific information for the test execution.</param>
    /// <exception cref="AgenixSystemException">
    /// Thrown when the specified element cannot be found on the page.
    /// </exception>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        var findBy = CreateBy(context);
        var element = browser.WebDriver.FindElement(findBy);

        if (element == null)
        {
            throw new AgenixSystemException($"Failed to find element '{findBy}' on page");
        }

        Validate(element, browser, context);
        Execute(element, browser, context);
    }

    /// <summary>
    ///     Subclasses may override this method to add element actions
    /// </summary>
    protected virtual void Execute(IWebElement element, SeleniumBrowser browser, TestContext context)
    {
        if (!string.IsNullOrWhiteSpace(element.TagName))
        {
            context.GetVariables()[element.TagName] = element;
        }
    }

    /// <summary>
    ///     Validates found a web element with expected content
    /// </summary>
    protected virtual void Validate(IWebElement element, SeleniumBrowser browser, TestContext context)
    {
        ValidateElementProperty("tag-name", TagName, element.TagName, context);
        ValidateElementProperty("text", Text, element.Text, context);

        if (IsDisplayed != element.Displayed)
        {
            throw new ValidationException($"Selenium web element validation failed, " +
                                          $"property 'displayed' expected '{IsDisplayed}', but was '{element.Displayed}'");
        }

        if (IsEnabled != element.Enabled)
        {
            throw new ValidationException($"Selenium web element validation failed, " +
                                          $"property 'enabled' expected '{IsEnabled}', but was '{element.Enabled}'");
        }

        if (Attributes != null)
        {
            foreach (var attributeEntry in Attributes)
            {
                ValidateElementProperty($"attribute '{attributeEntry.Key}'",
                    attributeEntry.Value, element.GetAttribute(attributeEntry.Key), context);
            }
        }

        if (Styles != null)
        {
            foreach (var styleEntry in Styles)
            {
                ValidateElementProperty($"css style '{styleEntry.Key}'",
                    styleEntry.Value, element.GetCssValue(styleEntry.Key), context);
            }
        }
    }

    /// <summary>
    ///     Validates web element property value with validation matcher support
    /// </summary>
    private static void ValidateElementProperty(string propertyName, string controlValue, string resultValue,
        TestContext context)
    {
        if (!string.IsNullOrWhiteSpace(controlValue))
        {
            var control = context.ReplaceDynamicContentInString(controlValue);

            if (ValidationMatcherUtils.IsValidationMatcherExpression(control))
            {
                ValidationMatcherUtils.ResolveValidationMatcher("payload", resultValue, control, context);
            }
            else
            {
                if (!control.Equals(resultValue))
                {
                    throw new ValidationException($"Selenium web element validation failed, " +
                                                  $"{propertyName} expected '{control}', but was '{resultValue}'");
                }
            }
        }
    }

    /// <summary>
    ///     Create By selector from type information
    /// </summary>
    protected By CreateBy(TestContext context)
    {
        if (By != null)
        {
            return By;
        }

        var value = ReplaceDynamicContent(PropertyValue, context);

        return Property switch
        {
            "id" => By.Id(value),
            "class-name" => By.ClassName(value),
            "link-text" => By.LinkText(value),
            "css-selector" => By.CssSelector(value),
            "name" => By.Name(value),
            "tag-name" => By.TagName(value),
            "xpath" => By.XPath(value),
            _ => throw new AgenixSystemException($"Unknown selector type: {Property}")
        };
    }

    /// <summary>
    ///     Helper method to replace dynamic content (placeholder implementation)
    /// </summary>
    private static string ReplaceDynamicContent(string value, TestContext context)
    {
        return string.IsNullOrEmpty(value) ? value :
            context.ReplaceDynamicContentInString(str: value);
    }

    /// <summary>
    ///     Action builder
    /// </summary>
    public class Builder : ElementActionBuilder<FindElementAction, Builder>
    {
        public Dictionary<string, string> Attributes { get; } = new();
        public Dictionary<string, string> Styles { get; } = new();
        public bool Displayed { get; private set; } = true;
        public bool Enabled { get; private set; } = true;
        public string Text { get; private set; }
        public string TagName { get; private set; }

        /// <summary>
        ///     Add text validation
        /// </summary>
        public Builder SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        ///     Add tag name validation
        /// </summary>
        public Builder SetTagName(string tagName)
        {
            TagName = tagName;
            return this;
        }

        /// <summary>
        ///     Add attribute validation
        /// </summary>
        public Builder SetAttribute(string name, string value)
        {
            Attributes[name] = value;
            return this;
        }

        /// <summary>
        ///     Add CSS style validation
        /// </summary>
        public Builder SetStyle(string name, string value)
        {
            Styles[name] = value;
            return this;
        }

        /// <summary>
        ///     Add enabled validation
        /// </summary>
        public Builder SetEnabled(bool enabled)
        {
            Enabled = enabled;
            return this;
        }

        /// <summary>
        ///     Add displayed validation
        /// </summary>
        public Builder SetDisplayed(bool displayed)
        {
            Displayed = displayed;
            return this;
        }

        /// <summary>
        /// Constructs and returns a new instance of the FindElementAction class
        /// using the configured properties in the builder.
        /// </summary>
        /// <returns>Returns a newly created FindElementAction instance.</returns>
        public override FindElementAction Build()
        {
            return new FindElementAction(this);
        }
    }

    /// <summary>
    ///     Abstract element-based action builder
    /// </summary>
    public abstract class ElementActionBuilder<TAction, TBuilder> : AbstractSeleniumAction.Builder<TAction, TBuilder>
        where TAction : ISeleniumAction
        where TBuilder : ElementActionBuilder<TAction, TBuilder>
    {
        public By By { get; protected set; }
        public string Property { get; protected set; }
        public string PropertyValue { get; protected set; }

        /// <summary>
        ///     Represents an element that can be used as part of various actions or validations in the DOM tree.
        /// </summary>
        public TBuilder Element(By by)
        {
            By = by;
            return self;
        }

        /// <summary>
        ///     Represents an element that can be used as part of various actions or validations in the DOM tree.
        /// </summary>
        /// <param name="property">The name of the property to validate or interact with.</param>
        /// <param name="propertyValue">The expected value of the property to validate or set.</param>
        /// <returns>An instance of the builder to allow method chaining.</returns>
        public TBuilder Element(string property, string propertyValue)
        {
            Property = property;
            PropertyValue = propertyValue;
            return self;
        }

        /// <summary>
        /// Builds and returns an instance of the specific action.
        /// </summary>
        /// <returns>The constructed instance of the action.</returns>
        public abstract override TAction Build();
    }
}
