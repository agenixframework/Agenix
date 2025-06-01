using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Log;

public class LogMessageModifierTest
{
    private readonly Mock<IMessage> _message = new();

    [Test]
    public void TestMaskBody()
    {
        _message.Setup(msg => msg.GetPayload<string>()).Returns("foo");
        ClassicAssert.AreEqual(new MockLogModifier("bar").MaskBody(_message.Object), "bar");
    }

    [Test]
    public void TestMaskHeaders()
    {
        var singleElementDictionary = new Dictionary<string, object> { { "key", "value" } };

        _message.Setup(msg => msg.GetHeaders()).Returns(singleElementDictionary);


        MockLogModifier modifier = new("key=value");
        ClassicAssert.AreEqual(modifier.MaskHeaders(_message.Object), singleElementDictionary);

        modifier = new MockLogModifier("key=masked");
        ClassicAssert.AreEqual(modifier.MaskHeaders(_message.Object),
            new Dictionary<string, string> { { "key", AgenixSettings.GetLogMaskValue() } });
    }

    private class MockLogModifier(string result) : LogMessageModifierBase
    {
        private readonly string result = result;

        public override string Mask(string statement)
        {
            return result;
        }
    }
}
