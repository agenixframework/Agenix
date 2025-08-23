using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Agenix.Selenium.Util;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class NavigateActionTest : AbstractNUnitSetUp
{
    [SetUp]
    public void SetupMethod()
    {
        _webDriver = new Mock<IWebDriver>();
        _navigation = new Mock<INavigation>();

        _seleniumBrowser = new SeleniumBrowser();
        _seleniumBrowser.WebDriver = _webDriver.Object;

        _webDriver.Setup(x => x.Navigate()).Returns(_navigation.Object);
    }

    private SeleniumBrowser _seleniumBrowser;
    private Mock<IWebDriver> _webDriver;
    private Mock<INavigation> _navigation;

    [Test]
    public async Task TestNavigatePageUrl()
    {
        _seleniumBrowser.EndpointConfiguration.BrowserType = BrowserType.CHROME.GetBrowserName();

        _navigation.Setup(x => x.GoToUrl(It.IsAny<Uri>()))
            .Callback<Uri>(url =>
            {
                Assert.That(url.ToString(), Is.EqualTo("http://localhost:8080/"));
            });

        var action = new NavigateAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetPage("http://localhost:8080")
            .Build();

        await action.ExecuteAsync(Context);

        _navigation.Verify(x => x.GoToUrl(It.IsAny<Uri>()), Times.Once);
    }

    [Test]
    public async Task TestNavigatePageUrlInternetExplorer()
    {
        _seleniumBrowser.EndpointConfiguration.BrowserType = BrowserType.INTERNET_EXPLORER.GetBrowserName();

        _navigation.Setup(x => x.GoToUrl(It.IsAny<string>()))
            .Callback<string>(url =>
            {
                Assert.That(url.ToString(), Does.StartWith("http://localhost:8080?timestamp="));
            });

        var action = new NavigateAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetPage("http://localhost:8080")
            .Build();

        await action.ExecuteAsync(Context);

        _navigation.Verify(x => x.GoToUrl(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task TestNavigateRelativePageUrl()
    {
        _seleniumBrowser.EndpointConfiguration.BrowserType = BrowserType.INTERNET_EXPLORER.GetBrowserName();
        _seleniumBrowser.EndpointConfiguration.StartPageUrl = "http://localhost:8080";

        var action = new NavigateAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetPage("info")
            .Build();

        await action.ExecuteAsync(Context);

        _navigation.Verify(x => x.GoToUrl("http://localhost:8080/info"), Times.Once);
    }

    [Test]
    public async Task TestExecuteBack()
    {
        var action = new NavigateAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetPage("back")
            .Build();

        await action.ExecuteAsync(Context);

        _navigation.Verify(x => x.Back(), Times.Once);
    }

    [Test]
    public async Task TestExecuteForward()
    {
        var action = new NavigateAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetPage("forward")
            .Build();

        await action.ExecuteAsync(Context);

        _navigation.Verify(x => x.Forward(), Times.Once);
    }

    [Test]
    public async Task TestExecuteRefresh()
    {
        var action = new NavigateAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetPage("refresh")
            .Build();

        await action.ExecuteAsync(Context);

        _navigation.Verify(x => x.Refresh(), Times.Once);
    }
}
