using System.Net;
using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Message;
using Agenix.Http.Message;
using NUnit.Framework.Legacy;

namespace Agenix.Http.Tests.Message;

public class HttpMessageTest
{
    private HttpMessage _httpMessage;

    [SetUp]
    public void SetUp()
    {
        _httpMessage = new HttpMessage();
    }

    [Test]
    public void TestSetCookies()
    {
        // GIVEN
        var cookieMock = new Cookie(); // Mocking Cookie
        var cookies = new List<Cookie> { cookieMock };

        // WHEN
        _httpMessage.SetCookies(cookies.ToArray());

        // THEN
        ClassicAssert.IsTrue(_httpMessage.GetCookies().Contains(cookieMock),
            "The cookie should be present in the HTTP message.");
    }

    [Test]
    public void TestCookiesWithSameNamesAreOverwritten()
    {
        // GIVEN
        var cookie = new Cookie("foo", "bar");
        _httpMessage.Cookie(cookie);

        var expectedCookie = new Cookie("foo", "foobar");

        // WHEN
        _httpMessage.Cookie(expectedCookie);

        // THEN
        ClassicAssert.AreEqual(1, _httpMessage.GetCookies().Count, "There should only be one cookie.");
        ClassicAssert.AreEqual("foobar", _httpMessage.GetCookies()[0].Value,
            "The cookie value should be updated to 'foobar'.");
    }

    [Test]
    public void TestSetCookiesOverwritesOldCookies()
    {
        // GIVEN
        var cookieMock1 = new Cookie();
        var cookieMock2 = new Cookie();

        _httpMessage.SetCookies(new List<Cookie> { cookieMock1, cookieMock2 }.ToArray());

        var expectedCookieMock = new Cookie();
        var newCookies = new List<Cookie> { expectedCookieMock }.ToArray();

        // WHEN
        _httpMessage.SetCookies(newCookies);

        // THEN
        ClassicAssert.IsTrue(_httpMessage.GetCookies().Contains(expectedCookieMock),
            "The expected cookie should be contained in the list.");
        ClassicAssert.AreEqual(1, _httpMessage.GetCookies().Count, "There should only be one cookie in the list.");
    }

    [Test]
    public void TestCopyConstructorPreservesCookies()
    {
        // GIVEN
        var expectedCookieMock = new Cookie();
        var originalMessage = new HttpMessage();
        originalMessage.Cookie(expectedCookieMock);

        // WHEN
        var messageCopy = new HttpMessage(originalMessage);

        // THEN
        ClassicAssert.IsTrue(messageCopy.GetCookies().SequenceEqual(originalMessage.GetCookies()),
            "The cookies in the copied message should match the original.");
    }

    [Test]
    public void TestParseQueryParamsAreParsedCorrectly()
    {
        // GIVEN
        var queryParamString = "foo=foobar,bar=barbar";

        var httpMessage = new HttpMessage();

        // WHEN
        var resultMessage = httpMessage.QueryParams(queryParamString);

        // THEN
        var queryParams = resultMessage.GetQueryParams();
        ClassicAssert.IsTrue(queryParams["foo"].Contains("foobar"), "The 'foo' parameter should contain 'foobar'.");
        ClassicAssert.IsTrue(queryParams["bar"].Contains("barbar"), "The 'bar' parameter should contain 'barbar'.");
    }

    [Test]
    public void TestParseQueryParamsSetsQueryParamHeaderName()
    {
        // GIVEN
        const string queryParamString = "foo=foobar,bar=barbar";

        var httpMessage = new HttpMessage();

        // WHEN
        var resultMessage = httpMessage.QueryParams(queryParamString);

        // THEN
        ClassicAssert.AreEqual(queryParamString, resultMessage.GetHeader(IEndpointUriResolver.QueryParamHeaderName),
            "The query parameter string should be correctly set as the header.");
    }

    [Test]
    public void TestQueryParamWithoutValueContainsNull()
    {
        // GIVEN
        const string queryParam = "foo";

        var httpMessage = new HttpMessage();

        // WHEN
        var resultMessage = httpMessage.QueryParam(queryParam);

        // THEN
        ClassicAssert.IsTrue(resultMessage.GetQueryParams()["foo"].Contains(null),
            "The 'foo' parameter should contain a null value.");
    }

    [Test]
    public void TestQueryParamWithValueIsSetCorrectly()
    {
        // GIVEN
        const string key = "foo";
        const string value = "foo";

        var httpMessage = new HttpMessage();

        // WHEN
        var resultMessage = httpMessage.QueryParam(key, value);

        // THEN
        ClassicAssert.IsTrue(resultMessage.GetQueryParams()[key].Contains(value),
            "The parameter 'foo' should correctly contain the value 'foo'.");
    }

    [Test]
    public void TestNewQueryParamIsAddedToExistingParams()
    {
        // GIVEN
        const string existingKey = "foo";
        const string existingValue = "foobar";
        var httpMessage = new HttpMessage();
        httpMessage.QueryParam(existingKey, existingValue);

        var newKey = "bar";
        var newValue = "barbar";

        // WHEN
        var resultMessage = httpMessage.QueryParam(newKey, newValue);

        // THEN
        ClassicAssert.IsTrue(resultMessage.GetQueryParams()[existingKey].Contains(existingValue),
            "The existing parameter 'foo' should contain the value 'foobar'.");
        ClassicAssert.IsTrue(resultMessage.GetQueryParams()[newKey].Contains(newValue),
            "The new parameter 'bar' should contain the value 'barbar'.");
    }

    [Test]
    public void TestNewQueryParamIsAddedQueryParamsHeader()
    {
        // GIVEN
        var httpMessage = new HttpMessage();
        httpMessage.QueryParam("foo", "foobar");

        const string expectedHeaderValue = "bar=barbar,foo=foobar";

        // WHEN
        var resultMessage = httpMessage.QueryParam("bar", "barbar");

        // THEN
        ClassicAssert.AreEqual(expectedHeaderValue, resultMessage.GetHeader(IEndpointUriResolver.QueryParamHeaderName),
            "The query parameters header should match the expected string.");
    }

    [Test]
    public void TestNewQueryParamIsAddedQueryParamHeaderName()
    {
        // GIVEN
        var httpMessage = new HttpMessage();
        httpMessage.QueryParam("foo", "foobar");

        var expectedHeaderValue = "bar=barbar,foo=foobar";

        // WHEN
        var resultMessage = httpMessage.QueryParam("bar", "barbar");

        // THEN
        ClassicAssert.AreEqual(expectedHeaderValue, resultMessage.GetHeader(IEndpointUriResolver.QueryParamHeaderName),
            "The query parameters header should match the expected string.");
    }

    [Test]
    public void TestDefaultStatusCodeIsNull()
    {
        // GIVEN
        var httpMessage = new HttpMessage();

        // WHEN
        var statusCode = httpMessage.GetStatusCode();

        // THEN
        ClassicAssert.IsNull(statusCode, "The default status code should be null.");
    }

    [Test]
    public void TestStringStatusCodeIsParsed()
    {
        // GIVEN
        var httpMessage = new HttpMessage();
        httpMessage.Header(HttpMessageHeaders.HttpStatusCode, "404");

        // WHEN
        var statusCode = httpMessage.GetStatusCode();

        // THEN
        ClassicAssert.AreEqual(HttpStatusCode.NotFound, statusCode,
            "The status code should be parsed and match HttpStatusCode.NotFound.");
    }

    [Test]
    public void TestIntegerStatusCodeIsParsed()
    {
        // GIVEN
        var httpMessage = new HttpMessage();
        httpMessage.Header(HttpMessageHeaders.HttpStatusCode, 403);

        // WHEN
        var statusCode = httpMessage.GetStatusCode();

        // THEN
        ClassicAssert.AreEqual(HttpStatusCode.Forbidden, statusCode,
            "The status code should be parsed and match HttpStatusCode.Forbidden.");
    }

    [Test]
    public void TestStatusCodeObjectIsPreserved()
    {
        // GIVEN
        var httpMessage = new HttpMessage();
        httpMessage.Header(HttpMessageHeaders.HttpStatusCode, HttpStatusCode.IMUsed);

        // WHEN
        var statusCode = httpMessage.GetStatusCode();

        // THEN
        ClassicAssert.AreEqual(HttpStatusCode.IMUsed, statusCode,
            "The status code should be preserved as HttpStatusCode.IMUsed.");
    }

    [Test]
    public void TestCanHandleCustomStatusCode()
    {
        // GIVEN
        var httpMessage = new HttpMessage();
        httpMessage.Header(HttpMessageHeaders.HttpStatusCode, 555);

        // WHEN
        var statusCode = httpMessage.GetStatusCode();

        // THEN
        ClassicAssert.AreEqual((HttpStatusCode)555, statusCode,
            "The custom status code should be handled correctly as 555.");
    }

    [Test]
    public void TestQueryParamWithMultipleParams()
    {
        // GIVEN
        var httpMessage = new HttpMessage();
        httpMessage.QueryParam("foo", "bar");

        var expectedHeaderValue = "foo=foobar,foo=bar";

        // WHEN
        var resultMessage = httpMessage.QueryParam("foo", "foobar");

        // THEN
        ClassicAssert.AreEqual(expectedHeaderValue, resultMessage.GetHeader(IEndpointUriResolver.QueryParamHeaderName),
            "The query parameters should be correctly concatenated as 'foo=bar,foo=foobar'.");
    }

    [Test]
    public void TestCopyConstructorPreservesIdAndTimestamp()
    {
        // Given
        _httpMessage.Payload = "myPayload";
        _httpMessage.SetHeader("k1", "v1");

        // When
        var copiedMessage = new HttpMessage(_httpMessage);

        // Then
        ClassicAssert.AreEqual(_httpMessage.GetHeader(MessageHeaders.Id), copiedMessage.GetHeader(MessageHeaders.Id),
            "The ID header should match.");
        ClassicAssert.AreEqual(_httpMessage.GetHeader(MessageHeaders.Timestamp),
            copiedMessage.GetHeader(MessageHeaders.Timestamp), "The Timestamp header should match.");
        ClassicAssert.AreEqual(_httpMessage.GetHeader("k1"), copiedMessage.GetHeader("k1"),
            "The custom header 'k1' should match.");
        ClassicAssert.AreEqual(_httpMessage.Payload, copiedMessage.Payload, "The payload should match.");
    }

    [Test]
    public void TestCopyConstructorWithAgenixOverwriteDoesNotPreserveIdAndTimestamp()
    {
        // Given
        _httpMessage.Payload = "myPayload";
        _httpMessage.SetHeader("k1", "v1");
        _httpMessage.SetHeader(MessageHeaders.Timestamp,
            (DateTime.UtcNow - TimeSpan.FromMilliseconds(1)).ToString("o"));

        // When
        var copiedMessage = new HttpMessage(_httpMessage, true);

        // Then
        ClassicAssert.AreNotEqual(_httpMessage.GetHeader(MessageHeaders.Id), copiedMessage.GetHeader(MessageHeaders.Id),
            "The ID header should not match.");
        ClassicAssert.AreNotEqual(_httpMessage.GetHeader(MessageHeaders.Timestamp),
            copiedMessage.GetHeader(MessageHeaders.Timestamp), "The Timestamp header should not match.");
        ClassicAssert.AreEqual(_httpMessage.GetHeader("k1"), copiedMessage.GetHeader("k1"),
            "The custom header 'k1' should match.");
        ClassicAssert.AreEqual(_httpMessage.Payload, copiedMessage.Payload, "The payload should match.");
    }
}
