using System;
using System.Linq;
using System.Net;
using Agenix.Api.Message;
using Agenix.Http.Message;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Http.Message;

public class HttpMessageBuilderTest
{
    private HttpMessage _message;

    [SetUp]
    public void BeforeTest()
    {
        _message = new HttpMessage();
        _message.SetHeader(MessageHeaders.Timestamp, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1);
    }

    [Test]
    public void TestDefaultMessageHeader()
    {
        //GIVEN
        var builder = GetBuilder();

        //WHEN
        var builtMessage = builder.Build(new TestContext(), MessageType.XML.ToString());

        //THEN
        ClassicAssert.AreEqual(3, builtMessage.GetHeaders().Count);
        ClassicAssert.NotNull(_message.GetHeader(MessageHeaders.Id));
        ClassicAssert.NotNull(_message.GetHeader(MessageHeaders.Timestamp));
        ClassicAssert.AreNotEqual(_message.GetHeader(MessageHeaders.Id), builtMessage.GetHeader(MessageHeaders.Id));
        ClassicAssert.AreNotEqual(_message.GetHeader(MessageHeaders.Timestamp),
            builtMessage.GetHeader(MessageHeaders.Timestamp));
        ClassicAssert.AreEqual(MessageType.XML.ToString(), builtMessage.GetType());
    }

    [Test]
    public void TestHeaderVariableSubstitution()
    {
        // GIVEN
        var builder = GetBuilder();

        var testContext = new TestContext();
        testContext.SetVariable("testHeader", "foo");
        testContext.SetVariable("testValue", "bar");

        _message.SetHeader("${testHeader}", "${testValue}");

        // WHEN
        var builtMessage = builder.Build(testContext, MessageType.XML.ToString());

        // THEN
        ClassicAssert.AreEqual("bar", builtMessage.GetHeader("foo"));
    }

    [Test]
    public void TestTemplateHeadersArePreserved()
    {
        // GIVEN
        var builder = GetBuilder();
        _message.SetHeader("foo", "bar");

        // WHEN
        var builtMessage = builder.Build(new TestContext(), MessageType.XML.ToString());

        // THEN
        ClassicAssert.AreEqual("bar", builtMessage.GetHeader("foo"));
    }

    [Test]
    public void TestCookieEnricherIsCalledForTemplateCookies()
    {
        // GIVEN
        var cookieEnricher = new CookieEnricher();
        var testContextMock = new TestContext();
        var templateCookie = new Cookie { Name = "foo" };
        _message.SetCookies([templateCookie]);


        var enrichedCookie = cookieEnricher.Enrich([templateCookie], testContextMock);
        var builder = new HttpMessageBuilder(_message, cookieEnricher);

        // WHEN
        var builtMessage = (HttpMessage)builder.Build(testContextMock, MessageType.XML.ToString());

        // THEN
        ClassicAssert.AreEqual(1, builtMessage.GetCookies().Count);
        ClassicAssert.AreEqual(enrichedCookie.First(), builtMessage.GetCookies().First());
    }

    /// Creates and returns an instance of HttpMessageBuilder using a predefined HttpMessage and a mock implementation of CookieEnricher.
    /// <returns>An instance of HttpMessageBuilder initialized with the specified HttpMessage and a mock CookieEnricher.</returns>
    private HttpMessageBuilder GetBuilder()
    {
        var cookieEnricherMock = new Mock<CookieEnricher>();
        return new HttpMessageBuilder(_message, cookieEnricherMock.Object);
    }
}
