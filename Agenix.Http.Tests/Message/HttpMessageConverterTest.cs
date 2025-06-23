using System.Net;
using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Http.Client;
using Agenix.Http.Message;
using Moq;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Http.Tests.Message;

public class HttpMessageConverterTest
{
    private readonly string _payload = "Hello World!";
    private Mock<CookieConverter> _cookieConverterMock;

    private HttpEndpointConfiguration _endpointConfiguration;
    private HttpMessage _message;
    private HttpMessageConverter _messageConverter;
    private TestContext _testContext;

    [SetUp]
    public void BeforeTest()
    {
        _cookieConverterMock = new Mock<CookieConverter>();
        _messageConverter = new HttpMessageConverter(_cookieConverterMock.Object);

        _endpointConfiguration = new HttpEndpointConfiguration();
        _testContext = new TestContext();
        _message = new HttpMessage();
    }

    [Test]
    public void TestDefaultMessageIsConvertedOnOutbound()
    {
        // GIVEN
        IMessage message = new DefaultMessage(_payload);

        // WHEN
        var httpEntity = _messageConverter.ConvertOutbound(message, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(_payload, httpEntity.Content.ReadAsStringAsync().Result);
    }

    [Test]
    public void TestHttpMessageCookiesArePreservedOnOutbound()
    {
        // GIVEN
        var cookie = new Cookie("foo", "bar");
        _message.Cookie(cookie);

        var expectedCookie = "foo=bar";

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        httpRequestMessage.Headers.TryGetValues("Cookie", out var cookies);
        ClassicAssert.IsNotNull(cookies);

        var enumerable = cookies.ToList();
        ClassicAssert.AreEqual(1, enumerable.Count());
        ClassicAssert.AreEqual(expectedCookie, enumerable.First());
    }

    [Test]
    public void TestHttpMessageCookiesValuesAreReplacedOnOutbound()
    {
        // GIVEN
        var cookie = new Cookie("foo", "${foobar}");
        _message.Cookie(cookie);

        _testContext.SetVariable("foobar", "bar");

        var expectedCookie = "foo=bar";

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        httpRequestMessage.Headers.TryGetValues("Cookie", out var cookies);
        ClassicAssert.IsNotNull(cookies);
        var enumerable = cookies.ToList();
        ClassicAssert.AreEqual(1, enumerable.Count);
        ClassicAssert.AreEqual(expectedCookie, enumerable.First());
    }

    [Test]
    public void TestHttpMessageHeadersAreReplacedOnOutbound()
    {
        // GIVEN
        _message.Header("foo", "bar");

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        httpRequestMessage.Headers.TryGetValues("foo", out var fooHeader);
        ClassicAssert.IsNotNull(fooHeader);
        var enumerable = fooHeader.ToList();
        ClassicAssert.AreEqual(1, enumerable.Count);
        ClassicAssert.AreEqual("bar", enumerable.First());
    }

    [Test]
    public void TestHttpContentTypeIsPresent()
    {
        // GIVEN
        _endpointConfiguration.ContentType = "foobar/text";

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        httpRequestMessage.Content.Headers.TryGetValues(HttpMessageHeaders.HttpContentType, out var contentTypeHeader);
        ClassicAssert.IsNotNull(contentTypeHeader);
        var enumerable = contentTypeHeader.ToList();
        ClassicAssert.AreEqual(1, enumerable.Count);
        ClassicAssert.AreEqual("foobar/text; charset=UTF-8", enumerable.First());
    }

    [Test]
    public void TestHttpContentTypeContainsAlteredCharsetIsPresent()
    {
        // GIVEN
        _endpointConfiguration.ContentType = "foobar/text";
        _endpointConfiguration.Charset = "whatever";

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        httpRequestMessage.Content.Headers.TryGetValues(HttpMessageHeaders.HttpContentType, out var contentTypeHeader);
        ClassicAssert.IsNotNull(contentTypeHeader);
        var enumerable = contentTypeHeader.ToList();
        ClassicAssert.AreEqual(1, enumerable.Count);
        ClassicAssert.AreEqual("foobar/text; charset=whatever", enumerable.First());
    }

    [Test]
    public void TestHttpContentTypeCharsetIsMissingWhenEmptyIsPresent()
    {
        // GIVEN
        _endpointConfiguration.ContentType = "foobar/text";
        _endpointConfiguration.Charset = "";

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        httpRequestMessage.Content.Headers.TryGetValues(HttpMessageHeaders.HttpContentType, out var contentTypeHeader);
        ClassicAssert.IsNotNull(contentTypeHeader);
        var enumerable = contentTypeHeader.ToList();
        ClassicAssert.AreEqual(1, enumerable.Count);
        ClassicAssert.AreEqual("foobar/text", enumerable.First());
    }

    [Test]
    public void TestHttpMethodBodyIsSetForPostOnOutbound()
    {
        // GIVEN
        _message.SetHeader(HttpMessageHeaders.HttpRequestMethod, HttpMethod.Post);
        _message.Payload = _payload;

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(_payload, httpRequestMessage.Content.ReadAsStringAsync().Result);
        ClassicAssert.AreEqual(_message.GetRequestMethod(), httpRequestMessage.Method);
    }

    [Test]
    public void TestHttpContentTypeJson()
    {
        // GIVEN
        _endpointConfiguration.ContentType = "application/json";
        _message.Payload = "{\"name\":\"christoph\", \"age\": 32}";

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        httpRequestMessage.Content.Headers.TryGetValues(HttpMessageHeaders.HttpContentType, out var contentTypeHeader);
        ClassicAssert.IsNotNull(contentTypeHeader);
        var enumerable = contentTypeHeader.ToList();
        ClassicAssert.AreEqual(1, enumerable.Count);
        ClassicAssert.AreEqual("application/json; charset=UTF-8", enumerable.First());
        ClassicAssert.AreEqual(_message.Payload, httpRequestMessage.Content.ReadAsStringAsync().Result);
    }

    [Test]
    public void TestHttpContentTypeXml()
    {
        // GIVEN
        _endpointConfiguration.ContentType = "application/xml";
        _message.Payload =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<configuration>\n    <appSettings>\n        <add key=\"myVariable\" value=\"test\"/>\n        <add key=\"user\" value=\"Agenix\"/>\n        <add key=\"welcomeText\" value=\"Hello ${user}!\"/>\n        <add key=\"todayDate\" value=\"Today is agenix:CurrentDate('yyyy-MM-dd')!\"/>\n    </appSettings>\n</configuration>";

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        httpRequestMessage.Content.Headers.TryGetValues(HttpMessageHeaders.HttpContentType, out var contentTypeHeader);
        ClassicAssert.IsNotNull(contentTypeHeader);
        var enumerable = contentTypeHeader.ToList();
        ClassicAssert.AreEqual(1, enumerable.Count);
        ClassicAssert.AreEqual("application/xml; charset=UTF-8", enumerable.First());
        ClassicAssert.AreEqual(_message.Payload, httpRequestMessage.Content.ReadAsStringAsync().Result);
    }

    [Test]
    public void TestHttpMethodBodyIsSetForPutOnOutbound()
    {
        // GIVEN
        _message.SetHeader(HttpMessageHeaders.HttpRequestMethod, HttpMethod.Put);
        _message.Payload = _payload;

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(_payload, httpRequestMessage.Content.ReadAsStringAsync().Result);
        ClassicAssert.AreEqual(_message.GetRequestMethod(), httpRequestMessage.Method);
    }

    [Test]
    public void TestHttpMethodBodyIsSetForDeleteOnOutbound()
    {
        // GIVEN
        _message.SetHeader(HttpMessageHeaders.HttpRequestMethod, HttpMethod.Delete);
        _message.Payload = _payload;

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(_payload, httpRequestMessage.Content.ReadAsStringAsync().Result);
        ClassicAssert.AreEqual(_message.GetRequestMethod(), httpRequestMessage.Method);
    }

    [Test]
    public void TestHttpMethodBodyIsSetForPatchOnOutbound()
    {
        // GIVEN
        _message.SetHeader(HttpMessageHeaders.HttpRequestMethod, HttpMethod.Patch);
        _message.Payload = _payload;

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(_payload, httpRequestMessage.Content.ReadAsStringAsync().Result);
        ClassicAssert.AreEqual(_message.GetRequestMethod(), httpRequestMessage.Method);
    }

    [Test]
    public void TestHttpMethodBodyIsSetForGetOnOutbound()
    {
        // GIVEN
        _message.SetHeader(HttpMessageHeaders.HttpRequestMethod, HttpMethod.Get);
        _message.Payload = _payload;

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.IsNull(httpRequestMessage.Content);
        ClassicAssert.AreEqual(_message.GetRequestMethod(), httpRequestMessage.Method);
    }

    [Test]
    public void TestHttpMethodBodyIsSetForHeadOnOutbound()
    {
        // GIVEN
        _message.SetHeader(HttpMessageHeaders.HttpRequestMethod, HttpMethod.Head);
        _message.Payload = _payload;

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.IsNull(httpRequestMessage.Content);
        ClassicAssert.AreEqual(_message.GetRequestMethod(), httpRequestMessage.Method);
    }

    [Test]
    public void TestHttpMethodBodyIsSetForOptionsOnOutbound()
    {
        // GIVEN
        _message.SetHeader(HttpMessageHeaders.HttpRequestMethod, HttpMethod.Options);
        _message.Payload = _payload;

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.IsNull(httpRequestMessage.Content);
        ClassicAssert.AreEqual(_message.GetRequestMethod(), httpRequestMessage.Method);
    }

    [Test]
    public void TestHttpMethodBodyIsSetForTraceOnOutbound()
    {
        // GIVEN
        _message.SetHeader(HttpMessageHeaders.HttpRequestMethod, HttpMethod.Trace);
        _message.Payload = _payload;

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.IsNull(httpRequestMessage.Content);
        ClassicAssert.AreEqual(_message.GetRequestMethod(), httpRequestMessage.Method);
    }

    [Test]
    public void TestHttpMessageWithStatusCodeContainsCookiesOnOutbound()
    {
        // GIVEN
        _message.SetHeader(HttpMessageHeaders.HttpStatusCode, "200");
        var cookie = new Cookie("foo", "bar");
        _message.Cookie(cookie);

        var expectedCookie = "foo=bar";

        // WHEN
        var httpRequestMessage = _messageConverter.ConvertOutbound(_message, _endpointConfiguration, _testContext);

        // THEN
        httpRequestMessage.Headers.TryGetValues("Cookie", out var cookies);
        ClassicAssert.IsNotNull(cookies);

        var enumerable = cookies.ToList();
        ClassicAssert.AreEqual(1, enumerable.Count());
        ClassicAssert.AreEqual(expectedCookie, enumerable.First());
    }

    [Test]
    public void TestAgenixDefaultHeaderAreSetOnInbound()
    {
        // WHEN
        var httpMessage =
            _messageConverter.ConvertInbound(new HttpResponseMessage(), _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.IsTrue(httpMessage.GetHeaders().ContainsKey("agenix_message_id"));
        ClassicAssert.IsTrue(httpMessage.GetHeaders().ContainsKey("agenix_message_timestamp"));
    }

    [Test]
    public void TestHttpEntityMessageBodyIsPreservedOnInbound()
    {
        var httpResponseMessage = new HttpResponseMessage { Content = new StringContent(_payload) };

        // WHEN
        var httpMessage = _messageConverter.ConvertInbound(httpResponseMessage, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(httpMessage.GetPayload<string>(), _payload);
    }

    [Test]
    public void TestHttpEntityDefaultMessageBodyIsSetOnInbound()
    {
        var httpResponseMessage = new HttpResponseMessage { Content = new StringContent("") };

        // WHEN
        var httpMessage = _messageConverter.ConvertInbound(httpResponseMessage, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(httpMessage.GetPayload<string>(), "");


        // WHEN
        httpMessage = _messageConverter.ConvertInbound(new HttpResponseMessage(), _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(httpMessage.GetPayload<string>(), "");
    }

    [Test]
    public void TestCustomHeadersAreSetOnInbound()
    {
        // GIVEN
        var httpResponseMessage = new HttpResponseMessage();

        httpResponseMessage.Headers.Add("foo", "bar");

        // WHEN
        var httpMessage = _messageConverter.ConvertInbound(httpResponseMessage, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual("bar", httpMessage.GetHeader("foo"));
    }

    [Test]
    public void testCustomHeadersListsAreConvertedToStringOnInbound()
    {
        // GIVEN
        var httpResponseMessage = new HttpResponseMessage();

        httpResponseMessage.Headers.Add("foo", "bar");
        httpResponseMessage.Headers.Add("foo", "foobar");
        httpResponseMessage.Headers.Add("foo", "foo");

        // WHEN
        var httpMessage = _messageConverter.ConvertInbound(httpResponseMessage, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual("bar,foobar,foo", httpMessage.GetHeader("foo"));
    }

    [Test]
    public void TestStatusCodeIsSetOnInbound()
    {
        // GIVEN
        var httpResponseMessage = new HttpResponseMessage();

        httpResponseMessage.StatusCode = HttpStatusCode.Forbidden;

        // WHEN
        var httpMessage =
            (HttpMessage)_messageConverter.ConvertInbound(httpResponseMessage, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(HttpStatusCode.Forbidden, httpMessage.GetStatusCode());
    }

    [Test]
    public void TestHttpVersionIsSetOnInbound()
    {
        // GIVEN
        var httpResponseMessage = new HttpResponseMessage();

        httpResponseMessage.StatusCode = HttpStatusCode.OK;

        // WHEN
        var httpMessage =
            (HttpMessage)_messageConverter.ConvertInbound(httpResponseMessage, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual("HTTP/1.1", httpMessage.GetVersion());
    }

    [Test]
    public void TestHttpVersion2IsSetOnInbound()
    {
        // GIVEN
        var httpResponseMessage = new HttpResponseMessage();

        httpResponseMessage.StatusCode = HttpStatusCode.OK;
        httpResponseMessage.Version = HttpVersion.Version20;

        // WHEN
        var httpMessage =
            (HttpMessage)_messageConverter.ConvertInbound(httpResponseMessage, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual("HTTP/2.0", httpMessage.GetVersion());
    }

    [Test]
    public void TestNoCookiesPreservedByDefaultOnInbound()
    {
        // GIVEN

        // WHEN
        _messageConverter.ConvertInbound(new HttpResponseMessage(), _endpointConfiguration, _testContext);

        // THEN
        _cookieConverterMock.Verify(x => x.ConvertCookies(It.IsAny<HttpResponseMessage>()), Times.Never);
    }

    [Test]
    public void TestCookiesPreservedOnConfigurationOnInbound()
    {
        var cookie = new Cookie { Name = "foo" };

        var httpResponseMessage = new HttpResponseMessage();
        httpResponseMessage.Content = new StringContent("foobar");
        httpResponseMessage.StatusCode = HttpStatusCode.OK;

        _cookieConverterMock.Setup(m => m.ConvertCookies(httpResponseMessage)).Returns([cookie]);

        _endpointConfiguration.HandleCookies = true;

        // WHEN
        var httpMessage =
            (HttpMessage)_messageConverter.ConvertInbound(httpResponseMessage, _endpointConfiguration, _testContext);

        // THEN
        ClassicAssert.AreEqual(1, httpMessage.GetCookies().Count);
        ClassicAssert.AreSame(cookie, httpMessage.GetCookies()[0]);
        ClassicAssert.AreEqual("foo", httpMessage.GetCookies()[0].Name);
    }
}
