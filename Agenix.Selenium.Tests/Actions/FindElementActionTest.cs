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
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

public class FindElementActionTest : AbstractNUnitSetUp
{
    private readonly SeleniumBrowser _seleniumBrowser = new();
    private readonly Mock<IWebDriver> _webDriver = new();
    private readonly Mock<IWebElement> _element = new();

    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();
        _element.Reset();

        _seleniumBrowser.WebDriver = _webDriver.Object;

        _element.Setup(x => x.Displayed).Returns(true);
        _element.Setup(x => x.Enabled).Returns(true);
        _element.Setup(x => x.TagName).Returns("button");
    }

    [Test]
    [TestCaseSource(nameof(FindByProvider))]
    public void TestExecuteFindBy(string property, string value, By by)
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>()))
            .Returns<By>(select =>
            {
                Assert.That(select.GetType(), Is.EqualTo(by.GetType()));
                Assert.That(select.ToString(), Is.EqualTo(by.ToString()));
                return _element.Object;
            });

        var action = new FindElementAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element(property, value)
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariableObject("button"), Is.EqualTo(_element.Object));
    }

    public static object[][] FindByProvider()
    {
        return
        [
            ["id", "myId", By.Id("myId")],
            ["name", "myName", By.Name("myName")],
            ["tag-name", "button", By.TagName("button")],
            ["class-name", "myClass", By.ClassName("myClass")],
            ["link-text", "myLinkText", By.LinkText("myLinkText")],
            ["css-selector", "myCss", By.CssSelector("myCss")],
            ["xpath", "myXpath", By.XPath("myXpath")]
        ];
    }

    [Test]
    public void TestExecuteFindByVariableSupport()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>()))
            .Returns<By>(select =>
            {
                Assert.That(select.GetType(), Is.EqualTo(typeof(By)));
                Assert.That(select.ToString(), Is.EqualTo(By.Id("clickMe").ToString()));
                return _element.Object;
            });

        Context.SetVariable("myId", "clickMe");

        var action = new FindElementAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("id", "${myId}")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariableObject("button"), Is.EqualTo(_element.Object));
    }

    [Test]
    public void TestExecuteFindByValidation()
    {
        _element.Setup(x => x.Text).Returns("Click Me!");
        _element.Setup(x => x.GetAttribute("type")).Returns("submit");
        _element.Setup(x => x.GetCssValue("color")).Returns("red");

        _webDriver.Setup(x => x.FindElement(It.IsAny<By>()))
            .Returns<By>(select =>
            {
                Assert.That(select.GetType(), Is.EqualTo(typeof(By)));
                Assert.That(select.ToString(), Is.EqualTo(By.Name("clickMe").ToString()));
                return _element.Object;
            });

        var action = new FindElementAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("name", "clickMe")
            .SetTagName("button")
            .SetText("Click Me!")
            .SetAttribute("type", "submit")
            .SetStyle("color", "red")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariableObject("button"), Is.EqualTo(_element.Object));
    }

    [Test]
    [TestCaseSource(nameof(ValidationErrorProvider))]
    public void TestExecuteFindByValidationFailed(string tagName, string text, string attribute, string cssStyle, bool displayed, bool enabled, string errorMsg)
    {
        _element.Setup(x => x.TagName).Returns("button");
        _element.Setup(x => x.Text).Returns("Click Me!");
        _element.Setup(x => x.GetAttribute("type")).Returns("submit");
        _element.Setup(x => x.GetCssValue("color")).Returns("red");

        _webDriver.Setup(x => x.FindElement(It.IsAny<By>()))
            .Returns<By>(select =>
            {
                Assert.That(select.GetType(), Is.EqualTo(typeof(By)));
                Assert.That(select.ToString(), Is.EqualTo(By.Name("clickMe").ToString()));
                return _element.Object;
            });

        var action = new FindElementAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("name", "clickMe")
            .SetTagName(tagName)
            .SetText(text)
            .SetAttribute("type", attribute)
            .SetStyle("color", cssStyle)
            .SetDisplayed(displayed)
            .SetEnabled(enabled)
            .Build();

        var ex = Assert.Throws<ValidationException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.EndWith(errorMsg));
    }

    public static object[][] ValidationErrorProvider()
    {
        return
        [
            ["input", "Click Me!", "submit", "red", true, true, "tag-name expected 'input', but was 'button'"],
            ["button", "Click!", "submit", "red", true, true, "text expected 'Click!', but was 'Click Me!'"],
            ["button", "Click Me!", "cancel", "red", true, true, "attribute 'type' expected 'cancel', but was 'submit'"
            ],
            ["button", "Click Me!", "submit", "red", false, true, "'displayed' expected 'False', but was 'True'"],
            ["button", "Click Me!", "submit", "red", true, false, "'enabled' expected 'False', but was 'True'"],
            ["button", "Click Me!", "submit", "blue", true, true, "css style 'color' expected 'blue', but was 'red'"]
        ];
    }

    [Test]
    public void TestElementNotFound()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns((IWebElement)null);

        var action = new FindElementAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("id", "myButton")
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Failed to find element 'By.Id: myButton' on page"));
    }

    [Test]
    public void TestElementUnsupportedProperty()
    {
        var action = new FindElementAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("unsupported", "wrong")
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Unknown selector type: unsupported"));
    }
}
