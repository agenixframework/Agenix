using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Agenix.Selenium.Util;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class StopBrowserActionTest : AbstractNUnitSetUp
{
    [SetUp]
    public void SetupMethod()
    {
        _webDriver = new Mock<IWebDriver>();
        _seleniumBrowser = new SeleniumBrowser();

        _seleniumBrowser.WebDriver = _webDriver.Object;
        _seleniumBrowser.EndpointConfiguration.BrowserType = BrowserType.CHROME.GetBrowserName();
    }

    private SeleniumBrowser _seleniumBrowser;
    private Mock<IWebDriver> _webDriver;

    [Test]
    public async Task TestStop()
    {
        Context.SetVariable(SeleniumHeaders.SeleniumBrowser, "ChromeBrowser");

        var action = new StopBrowserAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Build();

        await action.ExecuteAsync(Context);

        Assert.That(Context.GetVariables().ContainsKey(SeleniumHeaders.SeleniumBrowser), Is.False);
        Assert.That(_seleniumBrowser.WebDriver, Is.Null);

        _webDriver.Verify(x => x.Quit(), Times.Once);
    }
}
