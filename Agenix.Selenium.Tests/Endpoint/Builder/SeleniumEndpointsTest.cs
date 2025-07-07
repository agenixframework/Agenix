using Agenix.Api.Endpoint;
using Agenix.Selenium.Endpoint;

namespace Agenix.Selenium.Tests.Endpoint.Builder
{
    [TestFixture]
    public class SeleniumEndpointsTest
    {
        [Test]
        public void ShouldLookupEndpoints()
        {
            var endpointBuilders = IEndpointBuilder<IEndpoint>.Lookup();
            Assert.That(endpointBuilders.ContainsKey("selenium.browser"), Is.True);
        }

        [Test]
        public void ShouldLookupEndpoint()
        {
            var endpointBuilder = IEndpointBuilder<SeleniumBrowser>.Lookup("selenium.browser");
            Assert.That(endpointBuilder.IsPresent, Is.True);
            Assert.That(endpointBuilder.Value.GetType(), Is.EqualTo(typeof(SeleniumBrowserBuilder)));
        }
    }
}
