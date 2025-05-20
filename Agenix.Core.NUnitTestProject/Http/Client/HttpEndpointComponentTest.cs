using System.Net.Http;
using Agenix.Core.Spi;
using Agenix.Http.Client;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Core.NUnitTestProject.Http.Client;

public class HttpEndpointComponentTest
{
    private readonly IReferenceResolver _referenceResolver = new Mock<IReferenceResolver>().Object;
    private readonly TestContext _context = new();

    [SetUp]
    public void SetUp()
    {
        _context.SetReferenceResolver(_referenceResolver);
    }
    
    [Test]
    public void TestCreateClientEndpoint()
    {
        var component = new HttpEndpointComponent();

        var endpoint = component.CreateEndpoint("http://localhost:8088/test", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("http://localhost:8088/test", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Post, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(5000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }
    
    [Test]
    public void TestCreateClientEndpointWithParameters()
    {
        var component = new HttpEndpointComponent();
        var endpoint = component.CreateEndpoint("http://localhost:8088/test?requestMethod=DELETE&customParam=foo", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("http://localhost:8088/test?customParam=foo", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Delete, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(5000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }
    
    [Test]
    public void TestCreateClientEndpointWithCustomParameters()
    {
        var component = new HttpEndpointComponent();
        var endpoint = component.CreateEndpoint("http://localhost:8088/test?requestMethod=GET&timeout=10000", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("http://localhost:8088/test", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Get, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(10000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }
    
    [Test]
    public void TestCreateClientHttpsEndpoint()
    {
        var component = new HttpsEndpointComponent();

        var endpoint = component.CreateEndpoint("https://localhost:8088/test", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("https://localhost:8088/test", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Post, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(5000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }
    
    [Test]
    public void TestCreateClientHttpsEndpointWithParameters()
    {
        var component = new HttpsEndpointComponent();
        var endpoint = component.CreateEndpoint("https://localhost:8088/test?requestMethod=DELETE&customParam=foo", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("https://localhost:8088/test?customParam=foo", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Delete, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(5000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }
    
    [Test]
    public void TestCreateClientHttpsEndpointWithCustomParameters()
    {
        var component = new HttpsEndpointComponent();
        var endpoint = component.CreateEndpoint("https://localhost:8088/test?requestMethod=GET&timeout=10000", _context);

        ClassicAssert.AreEqual(typeof(HttpClient), endpoint.GetType());

        ClassicAssert.AreEqual("https://localhost:8088/test", ((HttpClient)endpoint).EndpointConfiguration.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Get, ((HttpClient)endpoint).EndpointConfiguration.RequestMethod);
        ClassicAssert.AreEqual(10000L, ((HttpClient)endpoint).EndpointConfiguration.Timeout);
    }
}