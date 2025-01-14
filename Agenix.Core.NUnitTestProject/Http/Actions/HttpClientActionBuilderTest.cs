using System.Net;
using System.Net.Http;
using Agenix.Http.Actions;
using Agenix.Http.Message;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Util;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Core.NUnitTestProject.Http.Actions;

public class HttpClientActionBuilderTest
{
    private AutoMocker _autoMocker;

    private HttpClientActionBuilder _fixture;
    private Mock<HttpClient> _httpClientMock;

    [SetUp]
    public void BeforeMethodSetup()
    {
        _autoMocker = new AutoMocker();
        _httpClientMock = _autoMocker.GetMock<HttpClient>();
        _fixture = new HttpClientActionBuilder(_httpClientMock.Object);
    }

    [Test]
    public void ResponseWithHttpStatus()
    {
        var httpMessageBuilderSupport = _fixture.Receive()
            .Response(HttpStatusCode.OK) // Method under test
            .Message();

        var httpMessage = (HttpMessage)ReflectionUtils.GetInstanceFieldValue(httpMessageBuilderSupport, "httpMessage");
        ClassicAssert.NotNull(httpMessage);

        var headers = httpMessage.GetHeaders();
        ClassicAssert.AreEqual((int)HttpStatusCode.OK, headers[HttpMessageHeaders.HttpStatusCode]);
        ClassicAssert.AreEqual(HttpStatusCode.OK.ToString(), headers[HttpMessageHeaders.HttpReasonPhrase]);
    }
    
    [Test]
    public void RequestWithGetMethodAndQueryParameters()
    {
        var httpMessageBuilderSupport = _fixture.Send()
            .Get()
            .Path("/test")
            .QueryParam("q", "v")
            .Message();

        var httpMessage = (HttpMessage)ReflectionUtils.GetInstanceFieldValue(httpMessageBuilderSupport, "httpMessage");
        ClassicAssert.NotNull(httpMessage);

        var headers = httpMessage.GetHeaders();
        ClassicAssert.AreEqual(HttpMethod.Get.Method, headers[HttpMessageHeaders.HttpRequestMethod]);
        ClassicAssert.AreEqual("/test", headers[HttpMessageHeaders.HttpRequestUri]);
        ClassicAssert.AreEqual("q=v", headers[HttpMessageHeaders.HttpQueryParams]);
    }

    [Test]
    public void ResponseWithHttpStatusCode()
    {
        const int code = 123;

        var httpMessageBuilderSupport = new HttpClientActionBuilder(_httpClientMock.Object)
            .Receive()
            .Response((HttpStatusCode)code) // Method under test
            .Message();

        var httpMessage = (HttpMessage)ReflectionUtils.GetInstanceFieldValue(httpMessageBuilderSupport, "httpMessage");
        ClassicAssert.NotNull(httpMessage);

        var headers = httpMessage.GetHeaders();
        ClassicAssert.AreEqual(code, headers[HttpMessageHeaders.HttpStatusCode]);
        ClassicAssert.False(headers.ContainsKey(HttpMessageHeaders.HttpReasonPhrase));
    }

    [Test]
    public void IsReferenceResolverAwareTestActionBuilder()
    {
        ClassicAssert.True(_fixture is not null, "Is instanceof AbstractReferenceResolverAwareTestActionBuilder");
    }
}