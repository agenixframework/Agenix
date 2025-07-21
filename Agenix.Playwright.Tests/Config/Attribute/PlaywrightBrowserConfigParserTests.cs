using Agenix.Api.Annotations;
using Agenix.Api.Config.Annotation;
using Agenix.Api.Endpoint;
using Agenix.Api.Spi;
using Agenix.Core.Annotations;
using Agenix.Core.Endpoint.Direct.Annotation;
using Agenix.Playwright.Config;
using Agenix.Playwright.Endpoint;
using Microsoft.Playwright;
using Moq;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Playwright.Tests.Config.Attribute;

public class PlaywrightBrowserConfigParserTest
{
    private readonly TestContext _context = Core.Agenix.NewInstance().AgenixContext.CreateTestContext();
    private readonly Mock<Action<IBrowserContext>> _contextEventHandler = new();
    private readonly Mock<Action<IPage>> _pageEventHandler = new();

    private readonly Mock<IReferenceResolver> _referenceResolver = new();

    [PlaywrightBrowserConfig]
    [AgenixEndpoint(Name = "browser1")]
    private PlaywrightBrowser browser1;

    [AgenixEndpoint]
    [PlaywrightBrowserConfig(
        Type = "firefox",
        Headless = false,
        StartPage = "https://microsoft.com",
        ViewportWidth = 1920,
        ViewportHeight = 1080,
        DefaultTimeout = 60000f,
        DefaultNavigationTimeout = 45000f,
        JavaScript = false,
        UserAgent = "CustomUserAgent",
        Locale = "en-US",
        TimezoneId = "America/New_York",
        PageEventListeners = ["pageEventHandler"],
        ContextEventListeners = ["contextEventHandler"]
    )]
    private PlaywrightBrowser browser2;

    [AgenixEndpoint]
    [PlaywrightBrowserConfig(
        Type = "webkit",
        Channel = "stable",
        ExecutablePath = "/custom/path/webkit",
        IgnoreHttpsErrors = true,
        IsMobile = true,
        HasTouch = true,
        DeviceScaleFactor = 2.0f,
        ColorScheme = "Dark",
        ReducedMotion = "Reduce"
    )]
    private PlaywrightBrowser browser3;

    [AgenixEndpoint]
    [PlaywrightBrowserConfig(
        Type = "chromium",
        Permissions = "geolocation,camera,microphone",
        HttpCredentialsUsername = "testuser",
        HttpCredentialsPassword = "testpass",
        ExtraHttpHeaders = "Authorization:Bearer token123,Content-Type:application/json",
        AcceptDownloads = true,
        VideoDir = "/tmp/videos",
        VideoWidth = 1280,
        VideoHeight = 720,
        RecordTrace = true,
        TraceDir = "/tmp/traces"
    )]
    private PlaywrightBrowser browser4;

    [SetUp]
    public void SetUp()
    {
        _referenceResolver.Reset();
        _pageEventHandler.Reset();
        _contextEventHandler.Reset();

        _referenceResolver.Setup(x => x.Resolve<Action<IPage>>("pageEventHandler"))
            .Returns(_pageEventHandler.Object);

        _referenceResolver.Setup(x => x.Resolve<Action<IBrowserContext>>("contextEventHandler"))
            .Returns(_contextEventHandler.Object);

        _context.SetReferenceResolver(_referenceResolver.Object);
    }

    [Test]
    public void ParseBrowserConfig_MinimalConfig_ShouldParseSuccessfully()
    {
        AgenixAnnotations.InjectEndpoints(this, _context);

        Assert.That(browser1, Is.Not.Null);
        Assert.That(browser1.EndpointConfiguration.BrowserType, Is.EqualTo("chromium"));
        Assert.That(browser1.EndpointConfiguration.Headless, Is.True);
        Assert.That(browser1.EndpointConfiguration.StartPageUrl, Is.EqualTo("about:blank"));
        Assert.That(browser1.EndpointConfiguration.JavaScript, Is.True);
        Assert.That(browser1.EndpointConfiguration.DefaultTimeout, Is.EqualTo(30000f));
        Assert.That(browser1.EndpointConfiguration.DefaultNavigationTimeout, Is.EqualTo(30000f));
        Assert.That(browser1.EndpointConfiguration.PageEventHandlers, Is.Empty);
        Assert.That(browser1.EndpointConfiguration.ContextEventHandlers, Is.Empty);
    }

    [Test]
    public void ParseBrowserConfig_FirefoxWithFullConfig_ShouldParseSuccessfully()
    {
        AgenixAnnotations.InjectEndpoints(this, _context);

        Assert.That(browser2, Is.Not.Null);
        Assert.That(browser2.EndpointConfiguration.BrowserType, Is.EqualTo("firefox"));
        Assert.That(browser2.EndpointConfiguration.Headless, Is.False);
        Assert.That(browser2.EndpointConfiguration.StartPageUrl, Is.EqualTo("https://microsoft.com"));
        Assert.That(browser2.EndpointConfiguration.Viewport.Width, Is.EqualTo(1920));
        Assert.That(browser2.EndpointConfiguration.Viewport.Height, Is.EqualTo(1080));
        Assert.That(browser2.EndpointConfiguration.DefaultTimeout, Is.EqualTo(60000f));
        Assert.That(browser2.EndpointConfiguration.DefaultNavigationTimeout, Is.EqualTo(45000f));
        Assert.That(browser2.EndpointConfiguration.JavaScript, Is.False);
        Assert.That(browser2.EndpointConfiguration.UserAgent, Is.EqualTo("CustomUserAgent"));
        Assert.That(browser2.EndpointConfiguration.Locale, Is.EqualTo("en-US"));
        Assert.That(browser2.EndpointConfiguration.TimezoneId, Is.EqualTo("America/New_York"));
        Assert.That(browser2.EndpointConfiguration.PageEventHandlers.Count, Is.EqualTo(1));
        Assert.That(browser2.EndpointConfiguration.ContextEventHandlers.Count, Is.EqualTo(1));
    }

    [Test]
    public void ParseBrowserConfig_WebkitWithDeviceSettings_ShouldParseSuccessfully()
    {
        AgenixAnnotations.InjectEndpoints(this, _context);

        Assert.That(browser3, Is.Not.Null);
        Assert.That(browser3.EndpointConfiguration.BrowserType, Is.EqualTo("webkit"));
        Assert.That(browser3.EndpointConfiguration.Channel, Is.EqualTo("stable"));
        Assert.That(browser3.EndpointConfiguration.ExecutablePath, Is.EqualTo("/custom/path/webkit"));
        Assert.That(browser3.EndpointConfiguration.IgnoreHttpsErrors, Is.True);
        Assert.That(browser3.EndpointConfiguration.IsMobile, Is.True);
        Assert.That(browser3.EndpointConfiguration.HasTouch, Is.True);
        Assert.That(browser3.EndpointConfiguration.DeviceScaleFactor, Is.EqualTo(2.0f));
        Assert.That(browser3.EndpointConfiguration.ColorScheme, Is.EqualTo(ColorScheme.Dark));
        Assert.That(browser3.EndpointConfiguration.ReducedMotion, Is.EqualTo(ReducedMotion.Reduce));
    }

    [Test]
    public void ParseBrowserConfig_ChromiumWithAdvancedFeatures_ShouldParseSuccessfully()
    {
        AgenixAnnotations.InjectEndpoints(this, _context);

        Assert.That(browser4, Is.Not.Null);
        Assert.That(browser4.EndpointConfiguration.BrowserType, Is.EqualTo("chromium"));
        Assert.That(browser4.EndpointConfiguration.Permissions, Is.Not.Empty);
        Assert.That(browser4.EndpointConfiguration.Permissions, Contains.Item("geolocation"));
        Assert.That(browser4.EndpointConfiguration.Permissions, Contains.Item("camera"));
        Assert.That(browser4.EndpointConfiguration.Permissions, Contains.Item("microphone"));
        Assert.That(browser4.EndpointConfiguration.HttpCredentials, Is.Not.Null);
        Assert.That(browser4.EndpointConfiguration.HttpCredentials.Username, Is.EqualTo("testuser"));
        Assert.That(browser4.EndpointConfiguration.HttpCredentials.Password, Is.EqualTo("testpass"));
        Assert.That(browser4.EndpointConfiguration.ExtraHttpHeaders, Is.Not.Empty);
        Assert.That(browser4.EndpointConfiguration.ExtraHttpHeaders["Authorization"], Is.EqualTo("Bearer token123"));
        Assert.That(browser4.EndpointConfiguration.ExtraHttpHeaders["Content-Type"], Is.EqualTo("application/json"));
        Assert.That(browser4.EndpointConfiguration.AcceptDownloads, Is.True);
        Assert.That(browser4.EndpointConfiguration.VideoDir, Is.EqualTo("/tmp/videos"));
        Assert.That(browser4.EndpointConfiguration.VideoSize.Width, Is.EqualTo(1280));
        Assert.That(browser4.EndpointConfiguration.VideoSize.Height, Is.EqualTo(720));
        Assert.That(browser4.EndpointConfiguration.RecordTrace, Is.True);
        Assert.That(browser4.EndpointConfiguration.TraceDir, Is.EqualTo("/tmp/traces"));
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
        Assert.That(validators["playwright.browser"], Is.Not.Null);
        Assert.That(validators["playwright.browser"].GetType(), Is.EqualTo(typeof(PlaywrightBrowserConfigParser)));
    }

    [Test]
    public void TestLookupByQualifier()
    {
        Assert.That(IAnnotationConfigParser<System.Attribute, IEndpoint>.Lookup("playwright.browser").IsPresent,
            Is.True);
    }
}
