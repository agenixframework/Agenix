using System.Collections.Generic;
using System.Net;
using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Http.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Http.Message;

public class HttpMessageUtilsTest
{
    [Test]
    public void TestCopy()
    {
        // GIVEN
        var from = new HttpMessage("fooMessage")
            .Header("X-Foo", "foo")
            .Cookie(new Cookie("Foo", "fooCookie"));
        from.AddHeaderData("HeaderData");
        from.Name = "FooMessage";

        var to = new HttpMessage("existingPayload")
            .Header("X-Existing", "existing")
            .Cookie(new Cookie("Existing", "existingCookie"))
            .AddHeaderData("ExistingHeaderData");

        to.Name = "ExistingMessage";

        // WHEN
        HttpMessageUtils.Copy(from, to);

        // THEN
        ClassicAssert.AreNotEqual(from.Id, to.Id, "IDs should be different.");
        ClassicAssert.AreEqual("FooMessage", to.Name, "Names should match.");
        ClassicAssert.AreEqual("fooMessage", to.Payload, "Payload should match.");
        ClassicAssert.AreEqual(7, to.GetHeaders().Count, "There should be four headers in the 'to' message.");
        ClassicAssert.IsNotNull(to.GetHeader(MessageHeaders.Id), "ID header should be present.");
        ClassicAssert.IsNotNull(to.GetHeader(MessageHeaders.MessageType), "MESSAGE_TYPE header should be present.");
        ClassicAssert.IsNotNull(to.GetHeader(MessageHeaders.Timestamp), "TIMESTAMP header should be present.");
        ClassicAssert.AreEqual("Foo=fooCookie", to.GetHeader(HttpMessageHeaders.HttpCookiePrefix + "Foo"),
            "Custom header should match.");
        ClassicAssert.AreEqual("Existing=existingCookie",
            to.GetHeader(HttpMessageHeaders.HttpCookiePrefix + "Existing"), "Custom header should match.");
        ClassicAssert.AreEqual(2, to.GetHeaderData().Count, "There should be one piece of header data.");
        ClassicAssert.AreEqual("ExistingHeaderData", to.GetHeaderData()[0], "Header data should match.");
        ClassicAssert.AreEqual("HeaderData", to.GetHeaderData()[1], "Header data should match.");
    }

    [Test]
    public void TestConvertAndCopy()
    {
        // GIVEN
        var from = new DefaultMessage("fooMessage")
            .SetHeader("X-Foo", "foo");
        from.AddHeaderData("HeaderData");
        from.Name = "FooMessage";

        var to = new HttpMessage();

        // WHEN
        HttpMessageUtils.Copy(from, to);

        // THEN
        ClassicAssert.AreNotEqual(from.Id, to.Id, "IDs should be different.");
        ClassicAssert.AreEqual("FooMessage", to.Name, "Names should match.");
        ClassicAssert.AreEqual("fooMessage", to.Payload, "Payload should match.");
        ClassicAssert.AreEqual("foo", to.GetHeader("X-Foo"), "Custom header should match.");
        ClassicAssert.AreEqual(1, to.GetHeaderData().Count, "There should be one piece of header data.");
        ClassicAssert.AreEqual("HeaderData", to.GetHeaderData()[0], "Header data should match.");
    }

    public static IEnumerable<TestCaseData> QueryParamStrings()
    {
        yield return new TestCaseData("", new Dictionary<string, string>());
        yield return new TestCaseData("key=value", new Dictionary<string, string> { { "key", "value" } });
        yield return new TestCaseData("key1=value1,key2=value2",
            new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } });
        yield return new TestCaseData("key1,key2=value2",
            new Dictionary<string, string> { { "key1", "" }, { "key2", "value2" } });
        yield return new TestCaseData("key1,key2", new Dictionary<string, string> { { "key1", "" }, { "key2", "" } });
    }

    [Test]
    [TestCaseSource(nameof(QueryParamStrings))]
    public void TestQueryParamsExtraction(string queryParamString, Dictionary<string, string> expectedParams)
    {
        // GIVEN
        var message = new HttpMessage();
        message.QueryParams(queryParamString);

        // WHEN - Extract query params
        var actualParams = message.GetQueryParams();

        // THEN - Check if the size matches
        ClassicAssert.AreEqual(expectedParams.Count, actualParams.Count);

        // Check if each expected key-value pair matches what's in the actual HTTP message query params
        foreach (var kv in expectedParams)
        {
            ClassicAssert.IsTrue(actualParams.ContainsKey(kv.Key), $"Missing key: {kv.Key}");
            ClassicAssert.IsTrue(actualParams[kv.Key].Contains(kv.Value),
                $"Value for key '{kv.Key}' does not match expected value '{kv.Value}'.");
        }
    }
}