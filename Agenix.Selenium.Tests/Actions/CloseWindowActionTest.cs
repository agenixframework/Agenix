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
using Agenix.Api.Exceptions;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

public class CloseWindowActionTest : AbstractNUnitSetUp
{
    private readonly SeleniumBrowser _seleniumBrowser = new();
    private readonly Mock<IWebDriver> _webDriver = new();
    private readonly Mock<ITargetLocator> _locator = new();

    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();
        _locator.Reset();

        _seleniumBrowser.WebDriver = _webDriver.Object;

        _webDriver.Setup(x => x.SwitchTo()).Returns(_locator.Object);
    }

    [Test]
    public void TestCloseActiveWindow()
    {
        var windows = new ReadOnlyCollection<string>(["active_window", "last_window"]);

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable(SeleniumHeaders.SeleniumLastWindow, "last_window");
        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");

        var action = new CloseWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumLastWindow), Is.EqualTo("last_window"));
        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumActiveWindow), Is.EqualTo("last_window"));

        _webDriver.Verify(x => x.Close(), Times.Once);
        _locator.Verify(x => x.Window("last_window"), Times.Once);
    }

    [Test]
    public void TestCloseActiveWindowReturnToDefault()
    {
        var windows = new ReadOnlyCollection<string>(["active_window", "main_window"]);

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.SetupSequence(x => x.CurrentWindowHandle)
            .Returns("active_window")
            .Returns("active_window")
            .Returns("main_window");

        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");

        var action = new CloseWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariables().ContainsKey(SeleniumHeaders.SeleniumLastWindow), Is.False);
        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumActiveWindow), Is.EqualTo("main_window"));

        _webDriver.Verify(x => x.Close(), Times.Once);
        _locator.Verify(x => x.DefaultContent(), Times.Once);
    }

    [Test]
    public void TestCloseOtherWindow()
    {
        var windows = new ReadOnlyCollection<string>(["active_window", "last_window", "other_window"]);

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable(SeleniumHeaders.SeleniumLastWindow, "last_window");
        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");
        Context.SetVariable("myWindow", "other_window");

        var action = new CloseWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetWindow("myWindow")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumLastWindow), Is.EqualTo("last_window"));
        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumActiveWindow), Is.EqualTo("active_window"));

        _webDriver.Verify(x => x.Close(), Times.Once);
        _locator.Verify(x => x.Window("other_window"), Times.Once);
        _locator.Verify(x => x.Window("active_window"), Times.Once);
    }

    [Test]
    public void TestCloseOtherWindowNoActiveWindow()
    {
        var windows = new ReadOnlyCollection<string>(["active_window", "other_window"]);

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable("myWindow", "other_window");

        var action = new CloseWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetWindow("myWindow")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariables().ContainsKey(SeleniumHeaders.SeleniumLastWindow), Is.False);
        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumActiveWindow), Is.EqualTo("active_window"));

        _webDriver.Verify(x => x.Close(), Times.Once);
        _locator.Verify(x => x.Window("other_window"), Times.Once);
        _locator.Verify(x => x.Window("active_window"), Times.Once);
    }

    [Test]
    public void TestCloseWindowInvalidWindowName()
    {
        var action = new CloseWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetWindow("myWindow")
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Failed to find window handle.*"));
    }

    [Test]
    public void TestCloseWindowNotFound()
    {
        var windows = new ReadOnlyCollection<string>(["active_window", "last_window"]);

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable(SeleniumHeaders.SeleniumLastWindow, "last_window");
        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");
        Context.SetVariable("myWindow", "other_window");

        var action = new CloseWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetWindow("myWindow")
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Failed to find window.*"));
    }
}
