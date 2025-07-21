using Agenix.Api.Endpoint;
using Agenix.Api.Spi;
using Agenix.Playwright.Endpoint;
using Moq;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Playwright.Tests.Endpoint.Builder;

[TestFixture]
public class PlaywrightEndpointComponentTests
{
    [SetUp]
    public void SetupMethod()
    {
        _referenceResolver = new Mock<IReferenceResolver>();
        _context = new TestContext();
        _context.SetReferenceResolver(_referenceResolver.Object);
    }

    private Mock<IReferenceResolver> _referenceResolver;
    private TestContext _context;

    [Test]
    public void TestCreateBrowserEndpoint()
    {
        var component = new PlaywrightEndpointComponent();

        var endpoint = component.CreateEndpoint("playwright:browser", _context);

        Assert.That(endpoint.GetType(), Is.EqualTo(typeof(PlaywrightBrowser)));
        Assert.That(((PlaywrightBrowser)endpoint).EndpointConfiguration.BrowserType,
            Is.EqualTo("chromium"));

        endpoint = component.CreateEndpoint("playwright:firefox", _context);

        Assert.That(endpoint.GetType(), Is.EqualTo(typeof(PlaywrightBrowser)));
        Assert.That(((PlaywrightBrowser)endpoint).EndpointConfiguration.BrowserType,
            Is.EqualTo("firefox"));
    }

    [Test]
    public void TestCreateBrowserEndpointWithParameters()
    {
        var component = new PlaywrightEndpointComponent();

        var endpoint = component.CreateEndpoint(
            "playwright:chromium?start-page=https://localhost:8080&headless=true&timeout=10000&viewport-width=1920&viewport-height=1080",
            _context);

        Assert.That(endpoint.GetType(), Is.EqualTo(typeof(PlaywrightBrowser)));

        var playwrightBrowser = (PlaywrightBrowser)endpoint;
        Assert.That(playwrightBrowser.EndpointConfiguration.StartPageUrl, Is.EqualTo("https://localhost:8080"));
        Assert.That(playwrightBrowser.EndpointConfiguration.Headless, Is.True);
        Assert.That(playwrightBrowser.EndpointConfiguration.DefaultTimeout, Is.EqualTo(10000f));
        Assert.That(playwrightBrowser.EndpointConfiguration.Viewport.Width, Is.EqualTo(1920));
        Assert.That(playwrightBrowser.EndpointConfiguration.Viewport.Height, Is.EqualTo(1080));
    }

    [Test]
    public void TestCreateBrowserEndpointWithBooleanParameters()
    {
        var component = new PlaywrightEndpointComponent();

        var endpoint = component.CreateEndpoint(
            "playwright:webkit?headless=false&javascript=true&ignore-https-errors=true&accept-downloads=true",
            _context);

        Assert.That(endpoint.GetType(), Is.EqualTo(typeof(PlaywrightBrowser)));

        var playwrightBrowser = (PlaywrightBrowser)endpoint;
        Assert.That(playwrightBrowser.EndpointConfiguration.BrowserType, Is.EqualTo("webkit"));
        Assert.That(playwrightBrowser.EndpointConfiguration.Headless, Is.False);
        Assert.That(playwrightBrowser.EndpointConfiguration.JavaScript, Is.True);
        Assert.That(playwrightBrowser.EndpointConfiguration.IgnoreHttpsErrors, Is.True);
        Assert.That(playwrightBrowser.EndpointConfiguration.AcceptDownloads, Is.True);
    }

    [Test]
    public void TestLookupAll()
    {
        var components = IEndpointComponent.Lookup();

        Assert.That(components.ContainsKey("playwright"), Is.True);
        Assert.That(components["playwright"].GetType(), Is.EqualTo(typeof(PlaywrightEndpointComponent)));
    }

    [Test]
    public void TestLookupByQualifier()
    {
        var component = IEndpointComponent.Lookup("playwright");
        Assert.That(component.IsPresent, Is.True);
        Assert.That(component.Value.GetType(), Is.EqualTo(typeof(PlaywrightEndpointComponent)));
    }
}
