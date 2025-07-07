using Agenix.Api.Endpoint;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Http.Client;
using Agenix.Selenium.Endpoint;
using Agenix.Selenium.Util;
using Moq;
using OpenQA.Selenium;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Selenium.Tests.Endpoint.Builder
{
    [TestFixture]
    public class SeleniumEndpointComponentTest
    {
        private Mock<IReferenceResolver> _referenceResolver;
        private Mock<IWebDriver> _chromeDriver;
        private TestContext _context;

        [SetUp]
        public void SetupMethod()
        {
            _referenceResolver = new Mock<IReferenceResolver>();
            _chromeDriver = new Mock<IWebDriver>();
            _context = new TestContext();
            _context.SetReferenceResolver(_referenceResolver.Object);
        }

        [Test]
        public void TestCreateBrowserEndpoint()
        {
            var component = new SeleniumEndpointComponent();

            var endpoint = component.CreateEndpoint("selenium:browser", _context);

            Assert.That(endpoint.GetType(), Is.EqualTo(typeof(SeleniumBrowser)));
            Assert.That(((SeleniumBrowser)endpoint).EndpointConfiguration.BrowserType,
                       Is.EqualTo(BrowserType.CHROME.GetBrowserName()));

            endpoint = component.CreateEndpoint("selenium:firefox", _context);

            Assert.That(endpoint.GetType(), Is.EqualTo(typeof(SeleniumBrowser)));
            Assert.That(((SeleniumBrowser)endpoint).EndpointConfiguration.BrowserType,
                       Is.EqualTo(BrowserType.FIREFOX.GetBrowserName()));
            Assert.That(((SeleniumBrowser)endpoint).EndpointConfiguration.Timeout,
                       Is.EqualTo(5000L));
        }

        [Test]
        public void TestCreateBrowserEndpointWithParameters()
        {
            var component = new SeleniumEndpointComponent();

            _referenceResolver.Reset();
            _referenceResolver.Setup(x => x.IsResolvable("chromeDriver")).Returns(true);
            _referenceResolver.Setup(x => x.Resolve<IWebDriver>("chromeDriver")).Returns(_chromeDriver.Object);

            var endpoint = component.CreateEndpoint(
                "selenium:chrome?start-page=https://localhost:8080&remote-server=https://localhost:8081&webDriver=chromeDriver&timeout=10000",
                _context);

            Assert.That(endpoint.GetType(), Is.EqualTo(typeof(SeleniumBrowser)));

            var seleniumBrowser = (SeleniumBrowser)endpoint;
            Assert.That(seleniumBrowser.EndpointConfiguration.WebDriver, Is.EqualTo(_chromeDriver.Object));
            Assert.That(seleniumBrowser.EndpointConfiguration.StartPageUrl, Is.EqualTo("https://localhost:8080"));
            Assert.That(seleniumBrowser.EndpointConfiguration.RemoteServerUrl, Is.EqualTo("https://localhost:8081"));
            Assert.That(seleniumBrowser.EndpointConfiguration.Timeout, Is.EqualTo(10000L));
        }

        [Test]
        public void TestLookupAll()
        {
            var components = IEndpointComponent.Lookup();

            Assert.That(components.Count, Is.EqualTo(4));
            Assert.That(components.ContainsKey("direct"), Is.True);
            Assert.That(components["direct"].GetType(), Is.EqualTo(typeof(DirectEndpointComponent)));
            Assert.That(components.ContainsKey("http"), Is.True);
            Assert.That(components["http"].GetType(), Is.EqualTo(typeof(HttpEndpointComponent)));
            Assert.That(components.ContainsKey("https"), Is.True);
            Assert.That(components["https"].GetType(), Is.EqualTo(typeof(HttpsEndpointComponent)));
            Assert.That(components.ContainsKey("selenium"), Is.True);
            Assert.That(components["selenium"].GetType(), Is.EqualTo(typeof(SeleniumEndpointComponent)));
        }

        [Test]
        public void TestLookupByQualifier()
        {
            var component = IEndpointComponent.Lookup("selenium");
            Assert.That(component.IsPresent, Is.True);
        }
    }
}
