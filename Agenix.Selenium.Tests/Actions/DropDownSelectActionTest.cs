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
using Agenix.Selenium.Util;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

public class DropDownSelectActionTest : AbstractNUnitSetUp
{
    private readonly Mock<IWebElement> _element = new();
    private readonly Mock<IWebDriver> _webDriver = new();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private SeleniumBrowser _seleniumBrowser;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();
        _element.Reset();

        _seleniumBrowser = new SeleniumBrowser();
        _seleniumBrowser.WebDriver = _webDriver.Object;

        _element.Setup(x => x.Displayed).Returns(true);
        _element.Setup(x => x.Enabled).Returns(true);
        _element.Setup(x => x.TagName).Returns("select");
    }

    [Test]
    public async Task TestExecuteSelect()
    {
        var option = new Mock<IWebElement>();

        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);

        var webElements = new ReadOnlyCollection<IWebElement>([option.Object]);
        _element.Setup(x => x.FindElements(It.IsAny<By>())).Returns(webElements);
        option.Setup(x => x.Enabled).Returns(true);
        option.Setup(x => x.Selected).Returns(false);

        var action = new DropDownSelectAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("name", "dropdown")
            .SetOption("select_me")
            .Build();

        await action.ExecuteAsync(Context);

        option.Verify(x => x.Click(), Times.Once);
    }

    [Test]
    public async Task TestExecuteMultiSelect()
    {
        var option = new Mock<IWebElement>();

        _seleniumBrowser.EndpointConfiguration.BrowserType = BrowserType.INTERNET_EXPLORER.GetBrowserName();

        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        var webElements = new ReadOnlyCollection<IWebElement>([option.Object]);
        _element.Setup(x => x.FindElements(It.IsAny<By>())).Returns(webElements);
        option.Setup(x => x.Enabled).Returns(true);
        option.Setup(x => x.Selected).Returns(false);

        var action = new DropDownSelectAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("name", "dropdown")
            .SetOptions("option1", "option2")
            .Build();

        await action.ExecuteAsync(Context);

        option.Verify(x => x.Click(), Times.Exactly(2));
    }
}
