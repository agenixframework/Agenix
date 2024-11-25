using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Message;

/// <summary>
///     This class contains unit tests for the MessageSelectorBuilder class.
/// </summary>
public class MessageSelectorBuilderTest
{
    [Test]
    public void TestToKeyValueDictionary()
    {
        var headerMap = MessageSelectorBuilder.WithString("foo = 'bar'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(1, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("bar", headerMap["foo"]);

        headerMap = MessageSelectorBuilder.WithString("foo = 'bar' AND operation = 'foo'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(2, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("bar", headerMap["foo"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("operation"));
        ClassicAssert.AreEqual("foo", headerMap["operation"]);

        headerMap = MessageSelectorBuilder.WithString("foo='bar' AND operation='foo'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(2, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("bar", headerMap["foo"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("operation"));
        ClassicAssert.AreEqual("foo", headerMap["operation"]);

        headerMap = MessageSelectorBuilder.WithString("foo='bar' AND operation='foo' AND foobar='true'")
            .ToKeyValueDictionary();

        ClassicAssert.AreEqual(3, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("bar", headerMap["foo"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("operation"));
        ClassicAssert.AreEqual("foo", headerMap["operation"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foobar"));
        ClassicAssert.AreEqual("true", headerMap["foobar"]);

        headerMap = MessageSelectorBuilder.WithString("A='Avalue' AND B='Bvalue' AND N='Nvalue'")
            .ToKeyValueDictionary();

        ClassicAssert.AreEqual(3, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("A"));
        ClassicAssert.AreEqual("Avalue", headerMap["A"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("B"));
        ClassicAssert.AreEqual("Bvalue", headerMap["B"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("N"));
        ClassicAssert.AreEqual("Nvalue", headerMap["N"]);

        headerMap = MessageSelectorBuilder.WithString("foo='OPERAND' AND bar='ANDROID'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(2, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("OPERAND", headerMap["foo"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("bar"));
        ClassicAssert.AreEqual("ANDROID", headerMap["bar"]);

        headerMap = MessageSelectorBuilder.WithString("foo='ANDROID'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(1, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("ANDROID", headerMap["foo"]);

        headerMap = MessageSelectorBuilder.WithString("xpath://foo[@key='primary']/value='bar' AND operation='foo'")
            .ToKeyValueDictionary();

        ClassicAssert.AreEqual(2, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("xpath://foo[@key='primary']/value"));
        ClassicAssert.AreEqual("bar", headerMap["xpath://foo[@key='primary']/value"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("operation"));
        ClassicAssert.AreEqual("foo", headerMap["operation"]);
    }
}