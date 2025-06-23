using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Core.Message.Selector;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Message.Selector;

public class HeaderMatchingMessageSelectorTest : AbstractNUnitSetUp
{
    [Test]
    public void TestHeaderMatchingSelector()
    {
        var messageSelector = new HeaderMatchingMessageSelector("operation", "foo", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foobar");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestHeaderMatchingSelectorValidationMatcher()
    {
        var messageSelector = new HeaderMatchingMessageSelector("operation", "@Contains(foo)@", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "barfoobar");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "bar");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestHeaderMatchingSelectorMultipleValues()
    {
        var messageSelector = new HeaderMatchingMessageSelector("foo", "bar", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestHeaderMatchingSelectorMissingHeader()
    {
        var messageSelector = new HeaderMatchingMessageSelector("operation", "foo", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestHeaderMatchingSelectorWithMessageObjectPayload()
    {
        var messageSelector = new HeaderMatchingMessageSelector("operation", "foo", Context);

        var acceptMessage = new DefaultMessage(new DefaultMessage("FooTest")
            .SetHeader("operation", "foo"));

        var declineMessage = new DefaultMessage(new DefaultMessage("FooTest")
            .SetHeader("operation", "foobar"));

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));

        messageSelector = new HeaderMatchingMessageSelector(MessageHeaders.Id, acceptMessage.Id, Context);

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }
}
