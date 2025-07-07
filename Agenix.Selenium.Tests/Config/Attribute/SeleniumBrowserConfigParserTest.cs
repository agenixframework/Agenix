using Agenix.Api.Annotations;
using Agenix.Api.Config.Annotation;
using Agenix.Api.Endpoint;
using Agenix.Api.Spi;
using Agenix.Core.Annotations;
using Agenix.Core.Endpoint.Direct.Annotation;
using Agenix.Selenium.Config;
using Agenix.Selenium.Endpoint;
using Agenix.Selenium.Util;
using Moq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Events;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Selenium.Tests.Config.Attribute;

public class SeleniumBrowserConfigParserTest
{
    [SeleniumBrowserConfig]
    [AgenixEndpoint(Name = "browser1")]
    private SeleniumBrowser browser1;

    [AgenixEndpoint]
    [SeleniumBrowserConfig(
        Type = "firefox",
        Version = "1.0",
        EventListeners = ["eventListener"],
        JavaScript = false,
        WebDriver = "webDriver",
        FirefoxProfile = "firefoxProfile",
        StartPage = "https://microsoft.com",
        Timeout = 10000L
    )]
    private SeleniumBrowser browser2;

    [AgenixEndpoint]
    [SeleniumBrowserConfig(
        Type = "internet explorer",
        RemoteServer = "http://localhost:9090/selenium"
    )]
    private SeleniumBrowser browser3;

    [AgenixEndpoint]
    [SeleniumBrowserConfig(Type = "chrome")]
    private SeleniumBrowser browser4;

    private readonly Mock<IReferenceResolver> _referenceResolver = new();
    private readonly Mock<Action<EventFiringWebDriver>> _eventHandler = new();
    private readonly Mock<IWebDriver> _webDriver = new();
    private readonly Mock<FirefoxProfile> _firefoxProfile = new();

    private readonly TestContext _context = Core.Agenix.NewInstance().AgenixContext.CreateTestContext();
    private static readonly string[] Names = ["eventListener"];

    [SetUp]
    public void SetUp()
    {
        // Initialize all mocks (equivalent to MockitoAnnotations.openMocks(this))
        _referenceResolver.Reset();
        _eventHandler.Reset();
        _webDriver.Reset();
        _firefoxProfile.Reset();

        // Set up mock behaviors (equivalent to when().thenReturn())
        _referenceResolver.Setup(x => x.Resolve<IWebDriver>("webDriver")).Returns(_webDriver.Object);

        _referenceResolver.Setup(x => x.Resolve<FirefoxProfile>("firefoxProfile"))
            .Returns(_firefoxProfile.Object);

        _referenceResolver.Setup(x => x.Resolve<Action<EventFiringWebDriver>>("eventListener"))
            .Returns(_eventHandler.Object);

        _referenceResolver.Setup(x => x.Resolve<Action<EventFiringWebDriver>>(Names))
            .Returns([_eventHandler.Object]);

        _context.SetReferenceResolver(_referenceResolver.Object);
    }

    [Test]
    public void ParseBrowserConfig_BrowserUsingMinimalConfig_ShouldParseConfigurationSuccessfully()
    {
        AgenixAnnotations.InjectEndpoints(this, _context);

        Assert.That(browser1, Is.Not.Null);
        Assert.That(browser1.EndpointConfiguration.BrowserType, Is.EqualTo(BrowserType.CHROME.GetBrowserName()));
        Assert.That(browser1.EndpointConfiguration.StartPageUrl, Is.Null);
        Assert.That(browser1.EndpointConfiguration.EventHandlers, Is.Empty);
        Assert.That(browser1.EndpointConfiguration.JavaScript, Is.True);
        Assert.That(browser1.EndpointConfiguration.WebDriver, Is.Null);
        Assert.That(browser1.EndpointConfiguration.FirefoxProfile, Is.Not.Null);
        Assert.That(browser1.EndpointConfiguration.RemoteServerUrl, Is.Null);
        Assert.That(browser1.EndpointConfiguration.Timeout, Is.EqualTo(5000L));
    }

    [Test]
    public void ParseBrowserConfig_FirefoxBrowserUsingFullConfig_ShouldParseConfigurationSuccessfully()
    {
        AgenixAnnotations.InjectEndpoints(this, _context);

        Assert.That(browser2, Is.Not.Null);
        Assert.That(browser2.EndpointConfiguration.BrowserType, Is.EqualTo(BrowserType.FIREFOX.GetBrowserName()));
        Assert.That(browser2.EndpointConfiguration.StartPageUrl, Is.EqualTo("https://microsoft.com"));
        Assert.That(browser2.EndpointConfiguration.EventHandlers.Count, Is.EqualTo(1));
        Assert.That(browser2.EndpointConfiguration.EventHandlers[0], Is.EqualTo(_eventHandler.Object));
        Assert.That(browser2.EndpointConfiguration.WebDriver, Is.EqualTo(_webDriver.Object));
        Assert.That(browser2.EndpointConfiguration.FirefoxProfile, Is.EqualTo(_firefoxProfile.Object));
        Assert.That(browser2.EndpointConfiguration.JavaScript, Is.False);
        Assert.That(browser2.EndpointConfiguration.RemoteServerUrl, Is.Null);
        Assert.That(browser2.EndpointConfiguration.Timeout, Is.EqualTo(10000L));
    }

    [Test]
    public void ParseBrowserConfig_RemoteBrowserConfig_ShouldParseConfigurationSuccessfully()
    {
        AgenixAnnotations.InjectEndpoints(this, _context);

        Assert.That(browser3, Is.Not.Null);
        Assert.That(browser3.EndpointConfiguration.BrowserType, Is.EqualTo(BrowserType.INTERNET_EXPLORER.GetBrowserName()));
        Assert.That(browser3.EndpointConfiguration.RemoteServerUrl, Is.EqualTo("http://localhost:9090/selenium"));
    }

    [Test]
    public void ParseBrowserConfig_ChromeBrowserConfig_ShouldParseConfigurationSuccessfully()
    {
        AgenixAnnotations.InjectEndpoints(this, _context);

        Assert.That(browser4, Is.Not.Null);
        Assert.That(browser4.EndpointConfiguration.BrowserType, Is.EqualTo(BrowserType.CHROME.GetBrowserName()));
    }

    [Test]
    public void TestLookupAll()
    {
        var validators = IAnnotationConfigParser<System.Attribute, IEndpoint>.Lookup();
        Assert.That(validators.Count, Is.EqualTo(3));
        Assert.That(validators["direct.async"], Is.Not.Null);
        Assert.That(validators["direct.async"].GetType(), Is.EqualTo(typeof(DirectEndpointConfigParser)));
        Assert.That(validators["direct.sync"], Is.Not.Null);
        Assert.That(validators["direct.sync"].GetType(), Is.EqualTo(typeof(DirectSyncEndpointConfigParser)));
        Assert.That(validators["selenium.browser"], Is.Not.Null);
        Assert.That(validators["selenium.browser"].GetType(), Is.EqualTo(typeof(SeleniumBrowserConfigParser)));
    }

    [Test]
    public void TestLookupByQualifier()
    {
        Assert.That(IAnnotationConfigParser<System.Attribute, IEndpoint>.Lookup("selenium.browser").IsPresent, Is.True);
    }

}
