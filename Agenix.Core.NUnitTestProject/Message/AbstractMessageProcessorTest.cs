using System;
using Agenix.Api.Message;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.NUnitTestProject.Message;

public class AbstractMessageProcessorTest : AbstractNUnitSetUp
{
    [Test]
    public void TestProcessMessage()
    {
        var processor = new MockMessageProcessor();

        var inMessage = new DefaultMessage("Hello Agenix!");
        inMessage.SetType(MessageType.XML.ToString());
        processor.Process(inMessage, Context);
        ClassicAssert.AreEqual("Processed!", inMessage.GetPayload<string>());

        inMessage = new DefaultMessage("Hello Agenix!");
        inMessage.SetType(MessageType.PLAINTEXT.ToString());
        processor.Process(inMessage, Context);
        ClassicAssert.AreEqual("Hello Agenix!", inMessage.GetPayload<string>());
    }

    private class MockMessageProcessor : AbstractMessageProcessor
    {
        public override bool SupportsMessageType(string messageType)
        {
            return string.Equals(messageType, MessageType.XML.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override void ProcessMessage(IMessage message, TestContext context)
        {
            message.Payload = "Processed!";
        }

        protected override string GetName()
        {
            return "MockProcessor";
        }
    }
}