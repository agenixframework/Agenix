using Agenix.Core.Message;
using Agenix.Core.Message.Selector;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Message.Selector;

public class PayloadMessageSelectorTest : AbstractNUnitSetUp
{
    [Test]
    public void TestPayloadEvaluation()
    {
        var messageSelector = new PayloadMatchingMessageSelector("payload", "foobar", Context);

        ClassicAssert.IsTrue(messageSelector.Accept(new DefaultMessage("foobar")));
        ClassicAssert.IsFalse(messageSelector.Accept(new DefaultMessage("barfoo")));
    }

    [Test]
    public void TestPayloadEvaluationValidationMatcher()
    {
        var messageSelector = new PayloadMatchingMessageSelector("payload", "@StartsWith(foo)@", Context);

        ClassicAssert.IsTrue(messageSelector.Accept(new DefaultMessage("foobar")));
        ClassicAssert.IsFalse(messageSelector.Accept(new DefaultMessage("barfoo")));
    }

    [Test]
    public void TestPayloadEvaluationWithMessageObjectPayload()
    {
        var messageSelector = new PayloadMatchingMessageSelector("payload", "foobar", Context);

        ClassicAssert.IsTrue(messageSelector.Accept(new DefaultMessage(new DefaultMessage("foobar"))));
        ClassicAssert.IsFalse(messageSelector.Accept(new DefaultMessage(new DefaultMessage("barfoo"))));
    }
}
