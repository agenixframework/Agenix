using System;
using System.Collections.Generic;
using System.Text;
using Agenix.Core.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Util;

public class DefaultTypeConverterTest
{
    private readonly DefaultTypeConverter _converter = DefaultTypeConverter.INSTANCE;

    [Test]
    public void TestConvertIfNecessary()
    {
        var payload = "Hello Agenix!";

        ClassicAssert.AreEqual("[foo, bar]",
            _converter.ConvertIfNecessary<string>(new List<string> { "foo", "bar" }, typeof(string)));
        ClassicAssert.AreEqual("[foo, bar]",
            _converter.ConvertIfNecessary<string>(new[] { "foo", "bar" }, typeof(string)));
        ClassicAssert.AreEqual("[foo, bar]",
            _converter.ConvertIfNecessary<string>(new object[] { "foo", "bar" }, typeof(string)));
        ClassicAssert.AreEqual("{foo=bar}",
            _converter.ConvertIfNecessary<string>(new Dictionary<object, object> { { "foo", "bar" } }, typeof(string)));
        ClassicAssert.AreEqual("null", _converter.ConvertIfNecessary<string>(null, typeof(string)));
        ClassicAssert.AreEqual(Convert.ToByte(1), _converter.ConvertIfNecessary<byte>("1", typeof(byte)));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<byte>(1, typeof(byte)));
        ClassicAssert.AreEqual(payload, _converter.ConvertIfNecessary<string>(payload, typeof(string)));
        ClassicAssert.AreEqual(Encoding.UTF8.GetBytes(payload),
            _converter.ConvertIfNecessary<byte[]>(payload, typeof(byte[])));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<short>("1", typeof(short)));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<short>(1, typeof(short)));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<int>("1", typeof(int)));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<int>(1, typeof(int)));
        ClassicAssert.AreEqual(1L, _converter.ConvertIfNecessary<long>("1", typeof(long)));
        ClassicAssert.AreEqual(1L, _converter.ConvertIfNecessary<long>(1L, typeof(long)));
        ClassicAssert.AreEqual(1.0F, _converter.ConvertIfNecessary<float>("1", typeof(float)));
        ClassicAssert.AreEqual(1.0F, _converter.ConvertIfNecessary<float>(1.0F, typeof(float)));
        ClassicAssert.AreEqual(1.0D, _converter.ConvertIfNecessary<double>("1", typeof(double)));
        ClassicAssert.AreEqual(1.0D, _converter.ConvertIfNecessary<double>(1.0D, typeof(double)));
        ClassicAssert.AreEqual(true, _converter.ConvertIfNecessary<bool>("true", typeof(bool)));
        ClassicAssert.AreEqual(false, _converter.ConvertIfNecessary<bool>(false, typeof(bool)));
        ClassicAssert.AreEqual(3, _converter.ConvertIfNecessary<string[]>("[a,b,c]", typeof(string[])).Length);
        ClassicAssert.AreEqual(3, _converter.ConvertIfNecessary<string[]>("[a, b, c]", typeof(string[])).Length);
        ClassicAssert.AreEqual(3, _converter.ConvertIfNecessary<List<string>>("[a,b,c]", typeof(List<string>)).Count);
        ClassicAssert.AreEqual(3, _converter.ConvertIfNecessary<List<string>>("[a, b, c]", typeof(List<string>)).Count);
        ClassicAssert.AreEqual("value",
            _converter.ConvertIfNecessary<Dictionary<string, object>>("{key=value}", typeof(Dictionary<string, object>))
                ["key"]);
        ClassicAssert.AreEqual("value2",
            _converter.ConvertIfNecessary<Dictionary<string, object>>("{key1=value1, key2=value2}",
                typeof(Dictionary<string, object>))["key2"]);
        ClassicAssert.AreEqual("value2",
            _converter.ConvertIfNecessary<Dictionary<string, object>>("{key1=value1,key2=value2}",
                typeof(Dictionary<string, object>))["key2"]);
        ClassicAssert.AreEqual("[1, 2]", _converter.ConvertIfNecessary<string>(new[] { 1, 2 }, typeof(string)));

        // TODO: fix below causes
        //ClassicAssert.AreEqual("[1, 2]", _converter.ConvertIfNecessary<string>(new List<int> { 1, 2 }, typeof(string)));
        ClassicAssert.AreEqual("[1, 2]", _converter.ConvertIfNecessary<string>(new[] { 1, 2 }, typeof(string)));
    }
}