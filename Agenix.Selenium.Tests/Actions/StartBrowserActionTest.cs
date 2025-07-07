using System;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Agenix.Selenium.Util;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class StartBrowserActionTest : AbstractNUnitSetUp
{
    private Mock<SeleniumBrowser> _seleniumBrowser = new();
    private Mock<SeleniumBrowserConfiguration> _seleniumBrowserConfiguration = new();
    private Mock<IWebDriver> _webDriver = new();
    private Mock<INavigation> _navigation = new();

    [SetUp]
    public void SetupMethod()
    {
        _seleniumBrowser.Reset();
        _seleniumBrowserConfiguration.Reset();
        _webDriver.Reset();
        _navigation.Reset();

        _seleniumBrowser.Setup(x => x.WebDriver).Returns(_webDriver.Object);
        _seleniumBrowser.Setup(x => x.EndpointConfiguration).Returns(_seleniumBrowserConfiguration.Object);
        _seleniumBrowser.Setup(x => x.Name).Returns("ChromeBrowser");
        _seleniumBrowserConfiguration.Setup(x => x.BrowserType).Returns(BrowserType.CHROME.GetBrowserName());
        _webDriver.Setup(x => x.Navigate()).Returns(_navigation.Object);
    }

    [Test]
    public void TestStart()
    {
        _seleniumBrowser.Setup(x => x.IsStarted).Returns(false);

        var action = new StartBrowserAction.Builder()
            .WithBrowser(_seleniumBrowser.Object)
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumBrowser), Is.EqualTo("ChromeBrowser"));

        _seleniumBrowser.Verify(x => x.Start(), Times.Once);
    }

    [Test]
    public void TestStartWithStartPage()
    {
        _seleniumBrowser.Setup(x => x.IsStarted).Returns(false);
        _seleniumBrowserConfiguration.Setup(x => x.StartPageUrl).Returns("http://localhost:8080");

        _navigation.Setup(x => x.GoToUrl(It.IsAny<Uri>()))
            .Callback<Uri>(url =>
            {
                Assert.That(url.ToString(), Is.EqualTo("http://localhost:8080/"));
            });

        var action = new StartBrowserAction.Builder()
            .WithBrowser(_seleniumBrowser.Object)
            .Build();

        action.Execute(Context); ;

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumBrowser), Is.EqualTo("ChromeBrowser"));

        _seleniumBrowser.Verify(x => x.Start(), Times.Once);
        _navigation.Verify(x => x.GoToUrl(It.IsAny<Uri>()), Times.Once);
    }

    [Test]
    public void TestStartAlreadyStarted()
    {
        _seleniumBrowser.Setup(x => x.IsStarted).Returns(true);

        var action = new StartBrowserAction.Builder()
            .WithBrowser(_seleniumBrowser.Object)
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumBrowser), Is.EqualTo("ChromeBrowser"));

        _seleniumBrowser.Verify(x => x.Stop(), Times.Never);
        _seleniumBrowser.Verify(x => x.Start(), Times.Never);
    }

    [Test]
    public void TestStartAlreadyStartedNotAllowed()
    {
        _seleniumBrowser.Setup(x => x.IsStarted).Returns(true);

        var action = new StartBrowserAction.Builder()
            .WithBrowser(_seleniumBrowser.Object)
            .AllowAlreadyStarted(false)
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumBrowser), Is.EqualTo("ChromeBrowser"));

        _seleniumBrowser.Verify(x => x.Stop(), Times.Once);
        _seleniumBrowser.Verify(x => x.Start(), Times.Once);
    }
}
