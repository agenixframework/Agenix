using Agenix.Api.Endpoint;
using Agenix.Playwright.Endpoint;

namespace Agenix.Playwright.Tests.Endpoint.Builder;

[TestFixture]
public class PlaywrightEndpointsTest
{
    [Test]
    public void ShouldLookupEndpoints()
    {
        var endpointBuilders = IEndpointBuilder<IEndpoint>.Lookup();
        Assert.That(endpointBuilders.ContainsKey("playwright.browser"), Is.True);
    }

    [Test]
    public void ShouldLookupEndpoint()
    {
        var endpointBuilder = IEndpointBuilder<PlaywrightBrowser>.Lookup("playwright.browser");
        Assert.That(endpointBuilder.IsPresent, Is.True);
        Assert.That(endpointBuilder.Value.GetType(), Is.EqualTo(typeof(PlaywrightBrowserBuilder)));
    }
}
