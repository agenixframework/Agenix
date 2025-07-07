using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Agenix.Selenium.Util;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class StopBrowserActionTest : AbstractNUnitSetUp
{
    private SeleniumBrowser _seleniumBrowser;
    private Mock<IWebDriver> _webDriver;

    [SetUp]
    public void SetupMethod()
    {
        _webDriver = new Mock<IWebDriver>();
        _seleniumBrowser = new SeleniumBrowser();

        _seleniumBrowser.WebDriver = _webDriver.Object;
        _seleniumBrowser.EndpointConfiguration.BrowserType = BrowserType.CHROME.GetBrowserName();
    }

    [Test]
    public void TestStop()
    {
        Context.SetVariable(SeleniumHeaders.SeleniumBrowser, "ChromeBrowser");

        var action = new StopBrowserAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariables().ContainsKey(SeleniumHeaders.SeleniumBrowser), Is.False);
        Assert.That(_seleniumBrowser.WebDriver, Is.Null);

        _webDriver.Verify(x => x.Quit(), Times.Once);
    }
}
