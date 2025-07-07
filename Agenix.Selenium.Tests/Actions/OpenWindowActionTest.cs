using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Agenix.Api.Exceptions;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Agenix.Selenium.Util;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class OpenWindowActionTest : AbstractNUnitSetUp
{
    private readonly SeleniumBrowser _seleniumBrowser = new();
    private readonly Mock<IWebDriver> _webDriver = new();
    private readonly Mock<ITargetLocator> _locator = new();

    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();
        _locator.Reset();

        // Configure the WebDriver to also implement IJavaScriptExecutor
        _webDriver.As<IJavaScriptExecutor>();

        _seleniumBrowser.WebDriver = _webDriver.Object;

        _webDriver.Setup(x => x.SwitchTo()).Returns(_locator.Object);
    }

    [Test]
    public void TestOpenWindow()
    {
        var windows = new ReadOnlyCollection<string>(new List<string> { "active_window", "new_window" });
        var initialWindows = new ReadOnlyCollection<string>(new List<string> { "active_window" });

        _webDriver.SetupSequence(x => x.WindowHandles)
            .Returns(initialWindows)
            .Returns(windows);

        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        _webDriver.As<IJavaScriptExecutor>()
            .Setup(x => x.ExecuteScript("window.open();"))
            .Returns(new List<object>());

        var action = new OpenWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetWindow("myNewWindow")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumLastWindow), Is.EqualTo("active_window"));
        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumActiveWindow), Is.EqualTo("new_window"));
        Assert.That(Context.GetVariable("myNewWindow"), Is.EqualTo("new_window"));

        _locator.Verify(x => x.Window("new_window"), Times.Once);
    }

    [Test]
    public void TestOpenWindowFailed()
    {
        var initialWindows = new ReadOnlyCollection<string>(new List<string> { "active_window" });

        _webDriver.Setup(x => x.WindowHandles).Returns(initialWindows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        _webDriver.As<IJavaScriptExecutor>()
            .Setup(x => x.ExecuteScript("window.open();"))
            .Returns(new List<object>());

        var action = new OpenWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetWindow("myNewWindow")
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Failed to open new window"));
    }
}
