using System;
using Agenix.Api.Exceptions;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class WaitUntilActionTest : AbstractNUnitSetUp
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
    public void TestWaitForHidden()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.Displayed).Returns(false);

        var action = new WaitUntilAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("class-name", "clickable")
            .Condition("hidden")
            .Build();

        action.Execute(Context);

        _element.Verify(x => x.Displayed, Times.AtLeastOnce);
    }

    [Test]
    public void TestWaitForHiddenTimeout()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.Displayed).Returns(true);

        var action = new WaitUntilAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("class-name", "clickable")
            .Condition("hidden")
            .Timeout(1000L)
            .Build();

        Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
    }

    [Test]
    public void TestWaitForVisible()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.Displayed).Returns(true);

        var action = new WaitUntilAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("class-name", "clickable")
            .Condition("visible")
            .Build();

        action.Execute(Context);

        _element.Verify(x => x.Displayed, Times.AtLeastOnce);
    }

    [Test]
    public void TestWaitForVisibleTimeout()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.Displayed).Returns(false);

        var action = new WaitUntilAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("class-name", "clickable")
            .Condition("visible")
            .Timeout(1000L)
            .Build();

        Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
    }

    [Test]
    public void TestUnsupportedWaitCondition()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.Displayed).Returns(false);

        var action = new WaitUntilAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("class-name", "clickable")
            .Condition("unknown")
            .Build();

        Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
    }
}
