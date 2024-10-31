using System;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Message;

[TestFixture]
public class MessagePayloadUtilsTest
{
    [Test]
    public void ShouldPrettyPrintJson()
    {
        ClassicAssert.AreEqual(MessagePayloadUtils.PrettyPrint(""), "");
        ClassicAssert.AreEqual(MessagePayloadUtils.PrettyPrint("{}"), "{}");
        ClassicAssert.AreEqual(MessagePayloadUtils.PrettyPrint("[]"), "[]");
        ClassicAssert.AreEqual(MessagePayloadUtils.PrettyPrint("{\"user\":\"agenix\"}"),
            string.Format("{{{0}  \"user\": \"agenix\"{0}}}", Environment.NewLine));
        ClassicAssert.AreEqual(MessagePayloadUtils.PrettyPrint("{\"text\":\"<?;,' '[]:>\"}"),
            string.Format("{{{0}  \"text\": \"<?;,' '[]:>\"{0}}}", Environment.NewLine));
        ClassicAssert.AreEqual(MessagePayloadUtils.PrettyPrint("[22, 32]"),
            string.Format("[{0}22,{0}32{0}]", Environment.NewLine));
    }

    [Test]
    public void ShouldPrettyPrintXml()
    {
        ClassicAssert.AreEqual(MessagePayloadUtils.PrettyPrint(""), "");
        ClassicAssert.AreEqual(MessagePayloadUtils.PrettyPrint("<root></root>"),
            string.Format("<root>{0}</root>{0}", Environment.NewLine));
    }
}