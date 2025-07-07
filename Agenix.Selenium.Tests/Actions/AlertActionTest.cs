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

namespace Agenix.Selenium.Tests.Actions;

using Moq;
using OpenQA.Selenium;

/// <summary>
/// @since 2.7
/// </summary>
public class AlertActionTest : AbstractNUnitSetUp
{
    private readonly SeleniumBrowser _seleniumBrowser = new();
    private readonly Mock<IWebDriver> _webDriver = new();
    private readonly Mock<ITargetLocator> _locator = new();
    private readonly Mock<IAlert> _alert = new();


    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();
        _alert.Reset();
        _locator.Reset();

        _seleniumBrowser.WebDriver = (_webDriver.Object);

        _webDriver.Setup(x => x.SwitchTo()).Returns(_locator.Object);
        _alert.Setup(x => x.Text).Returns("This is a warning!");
    }

    [Test]
    public void TestExecuteAccept()
    {
        _locator.Setup(x => x.Alert()).Returns(_alert.Object);

        var action = new AlertAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Build();

        action.Execute(Context);

        _alert.Verify(x => x.Accept(), Times.Once);
    }

    [Test]
    public void TestExecuteDismiss()
    {
        var locatorMock = new Mock<ITargetLocator>();
        _webDriver.Setup(x => x.SwitchTo()).Returns(locatorMock.Object);
        locatorMock.Setup(x => x.Alert()).Returns(_alert.Object);

        var action = new AlertAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .DismissAlert()
            .Build();

        action.Execute(Context);

        _alert.Verify(x => x.Dismiss(), Times.Once);
    }

    [Test]
    public void TestExecuteTextValidation()
    {
        _locator.Setup(x => x.Alert()).Returns(_alert.Object);

        var action = new AlertAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetText("This is a warning!")
            .Build();

        action.Execute(Context);

        _alert.Verify(x => x.Accept(), Times.Once);
    }

    [Test]
    public void TestExecuteTextValidationVariableSupport()
    {
        _locator.Setup(x => x.Alert()).Returns(_alert.Object);

        Context.SetVariable("alertText", "This is a warning!");

        var action = new AlertAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetText("${alertText}")
            .Build();

        action.Execute(Context);

        _alert.Verify(x => x.Accept(), Times.Once);
    }

    [Test]
    public void TestExecuteTextValidationMatcherSupport()
    {
        _locator.Setup(x => x.Alert()).Returns(_alert.Object);

        var action = new AlertAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetText("@StartsWith('This is')@")
            .Build();

        action.Execute(Context);

        _alert.Verify(x => x.Accept(), Times.Once);
    }

    [Test]
    public void TestExecuteTextValidationError()
    {
        _locator.Setup(x => x.Alert()).Returns(_alert.Object);

        var action = new AlertAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetText("This is not a warning!")
            .Build();

        var ex = Assert.Throws<ValidationException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Failed to validate alert dialog text.*"));

        _alert.Verify(x => x.Accept(), Times.Never);
    }

    [Test]
    public void TestAlertNotFound()
    {
        var locatorMock = new Mock<ITargetLocator>();
        _webDriver.Setup(x => x.SwitchTo()).Returns(locatorMock.Object);
        locatorMock.Setup(x => x.Alert()).Returns((IAlert)null);

        var action = new AlertAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Failed to access alert dialog - not found"));
    }
}
