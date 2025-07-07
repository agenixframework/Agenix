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

using System.Collections.ObjectModel;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

public class FillFormActionTest : AbstractNUnitSetUp
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
        _element.Setup(x => x.TagName).Returns("input");
    }

    [Test]
    public void TestExecute()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);

        var action = new FillFormAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Field("username", "foo_user")
            .Field("password", "secret")
            .Build();

        action.Execute(Context);

        _element.Verify(x => x.Clear(), Times.Exactly(2));
        _element.Verify(x => x.SendKeys("foo_user"), Times.Once);
        _element.Verify(x => x.SendKeys("secret"), Times.Once);
    }

    [Test]
    public void TestExecuteWithSelect()
    {
        var option = new Mock<IWebElement>();

        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.TagName).Returns("select");
        var webElements = new ReadOnlyCollection<IWebElement>([option.Object]);
        _element.Setup(x => x.FindElements(It.IsAny<By>())).Returns(webElements);
        option.Setup(x => x.Enabled).Returns(true);
        option.Setup(x => x.Selected).Returns(false);

        var action = new FillFormAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Field("remember-me", "yes")
            .Build();

        action.Execute(Context);

        option.Verify(x => x.Click(), Times.Once);
    }

    [Test]
    public void TestExecuteWithJson()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);

        var action = new FillFormAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .FromJson("""
                {
                    "username": "foo_user",
                    "password": "secret"
                }
                """)
            .Build();

        action.Execute(Context);

        _element.Verify(x => x.Clear(), Times.Exactly(2));
        _element.Verify(x => x.SendKeys("foo_user"), Times.Once);
        _element.Verify(x => x.SendKeys("secret"), Times.Once);
    }

    [Test]
    public void TestExecuteWithFormSubmit()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);

        var action = new FillFormAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Field("username", "foo_user")
            .Field("password", "secret")
            .Submit("save")
            .Build();

        action.Execute(Context);

        _element.Verify(x => x.Clear(), Times.Exactly(2));
        _element.Verify(x => x.SendKeys("foo_user"), Times.Once);
        _element.Verify(x => x.SendKeys("secret"), Times.Once);
        _element.Verify(x => x.Click(), Times.Once);
    }
}
