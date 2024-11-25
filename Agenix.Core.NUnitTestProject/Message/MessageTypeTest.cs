using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Message;

public class MessageTypeTest
{
    [Test]
    public void TestKnowsMessageType()
    {
        ClassicAssert.AreEqual(MessageTypeExtensions.Knows("xml"), true);
        ClassicAssert.AreEqual(MessageTypeExtensions.Knows("XML"), true);

        ClassicAssert.AreEqual(MessageTypeExtensions.Knows("PLAINTEXT"), true);
        ClassicAssert.AreEqual(MessageTypeExtensions.Knows("plaintext"), true);
        ClassicAssert.AreEqual(MessageTypeExtensions.Knows("json"), true);
        ClassicAssert.AreEqual(MessageTypeExtensions.Knows("csv"), true);

        ClassicAssert.AreEqual(MessageTypeExtensions.Knows("msexcel"), false);
        ClassicAssert.AreEqual(MessageTypeExtensions.Knows(""), false);
    }
}