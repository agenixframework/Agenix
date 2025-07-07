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
using OpenQA.Selenium.Interactions;

namespace Agenix.Selenium.Tests.Actions;

public class HoverActionTest : AbstractNUnitSetUp
{
    private readonly SeleniumBrowser _seleniumBrowser = new();
    private readonly Mock<IWebDriver> _webDriver = new();
    private readonly Mock<IActionExecutor> _actionExecutor = new();
    private readonly Mock<IWebElement> _element = new();

    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();
        _actionExecutor.Reset();
        _element.Reset();

        // Configure the WebDriver to implement IActionExecutor
        _webDriver.As<IActionExecutor>();
        _webDriver.As<IActionExecutor>().Setup(x => x.PerformActions(It.IsAny<IList<ActionSequence>>()));

        _seleniumBrowser.WebDriver = _webDriver.Object;

        _element.Setup(x => x.Displayed).Returns(true);
        _element.Setup(x => x.Enabled).Returns(true);
        _element.Setup(x => x.TagName).Returns("button");
    }

    [Test]
    public void TestExecute()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);

        var action = new HoverAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("id", "myButton")
            .Build();

        action.Execute(Context);
    }

    [Test]
    public void TestElementNotFound()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns((IWebElement)null);

        var action = new HoverAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("id", "myButton")
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Failed to find element 'By.Id: myButton' on page"));
    }
}
