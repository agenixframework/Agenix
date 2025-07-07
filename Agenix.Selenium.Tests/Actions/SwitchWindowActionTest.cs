using System.Collections.Generic;
using System.Collections.ObjectModel;
using Agenix.Api.Exceptions;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class SwitchWindowActionTest : AbstractNUnitSetUp
{
    private SeleniumBrowser _seleniumBrowser;
    private Mock<IWebDriver> _webDriver;
    private Mock<ITargetLocator> _locator;

    [SetUp]
    public void SetupMethod()
    {
        _webDriver = new Mock<IWebDriver>();
        _locator = new Mock<ITargetLocator>();
        _seleniumBrowser = new SeleniumBrowser();

        _seleniumBrowser.WebDriver = _webDriver.Object;

        _webDriver.Setup(x => x.SwitchTo()).Returns(_locator.Object);
    }

    [Test]
    public void TestSwitchToActiveWindow()
    {
        var windows = new ReadOnlyCollection<string>
        ([
            "active_window",
            "last_window",
            "other_window"]
        );

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable(SeleniumHeaders.SeleniumLastWindow, "last_window");
        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");

        var action = new SwitchWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumLastWindow), Is.EqualTo("last_window"));
        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumActiveWindow), Is.EqualTo("active_window"));

        _locator.Verify(x => x.Window(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void TestSwitchWindow()
    {
        var windows = new ReadOnlyCollection<string>
        (
            ["active_window",
            "other_window"]
        );

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");
        Context.SetVariable("myWindow", "other_window");

        var action = new SwitchWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Window("myWindow")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumLastWindow), Is.EqualTo("active_window"));
        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumActiveWindow), Is.EqualTo("other_window"));

        _locator.Verify(x => x.Window("other_window"), Times.Once);
    }

    [Test]
    public void TestSwitchWindowNotFound()
    {
        var windows = new ReadOnlyCollection<string>
        (
            ["active_window"]
        );

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");
        Context.SetVariable("myWindow", "other_window");

        var action = new SwitchWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Window("myWindow")
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Failed to find window.*"));
    }
}
