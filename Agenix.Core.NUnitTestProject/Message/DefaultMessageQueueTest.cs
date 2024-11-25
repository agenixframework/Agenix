using Agenix.Core.Message;
using Agenix.Core.Message.Selector;
using Agenix.Core.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Message;

public class DefaultMessageQueueTest
{
    private TestContext _context;

    [SetUp]
    public void SetupMocks()
    {
        _context = new TestContext();
    }

    [Test]
    public void TestReceiveSelected()
    {
        var queue = new DefaultMessageQueue("testQueue");
        queue.SetPollingInterval(100L);

        queue.Send(new DefaultMessage("FooMessage").SetHeader("foo", "bar"));

        var selector = new HeaderMatchingMessageSelector("foo", "bar", _context);

        var receivedMessage = queue.Receive(message => selector.Accept(message), 1000L);

        ClassicAssert.AreEqual(receivedMessage.GetPayload<string>(), "FooMessage");
        ClassicAssert.AreEqual(receivedMessage.GetHeaders()["foo"], "bar");
    }

    [Test]
    public void TestWithRetry()
    {
        var queue = new DefaultMessageQueue("testQueue");
        queue.SetPollingInterval(100L);

        queue.Send(new DefaultMessage("FooMessage").SetHeader("foo", "bar"));

        var retries = new AtomicLong();
        var selector = new CustomHeaderMatchingMessageSelectorGreaterThan7("foo", "bar", _context, retries);

        var receivedMessage = queue.Receive(message => selector.Accept(message), 1000L);

        ClassicAssert.AreEqual(receivedMessage.GetPayload<string>(), "FooMessage");
        ClassicAssert.AreEqual(receivedMessage.GetHeaders()["foo"], "bar");
        ClassicAssert.AreEqual(retries.Get(), 8L);
    }

    [Test]
    public void TestRetryExceeded()
    {
        var queue = new DefaultMessageQueue("testQueue");
        queue.SetPollingInterval(500L);

        queue.Send(new DefaultMessage("FooMessage").SetHeader("foos", "bars"));

        var retries = new AtomicLong();
        var selector = new CustomHeaderMatchingMessageSelector("foo", "bar", _context, retries);

        var receivedMessage = queue.Receive(message => selector.Accept(message), 1000L);

        ClassicAssert.IsNull(receivedMessage);
        ClassicAssert.AreEqual(retries.Get(), 3L);
    }

    [Test]
    public void TestRetryExceededWithTimeoutRest()
    {
        var queue = new DefaultMessageQueue("testQueue");
        queue.SetPollingInterval(400L);

        queue.Send(new DefaultMessage("FooMessage").SetHeader("foos", "bars"));

        var retries = new AtomicLong();
        var selector = new CustomHeaderMatchingMessageSelector("foo", "bar", _context, retries);

        var receivedMessage = queue.Receive(message => selector.Accept(message), 1000L);

        ClassicAssert.IsNull(receivedMessage);
        ClassicAssert.AreEqual(retries.Get(), 4L);
    }

    private class CustomHeaderMatchingMessageSelector(
        string headerName,
        string headerValue,
        TestContext context,
        AtomicLong retries)
        : HeaderMatchingMessageSelector(headerName, headerValue, context)
    {
        public override bool Accept(IMessage message)
        {
            retries.IncrementAndGet();
            return base.Accept(message);
        }
    }

    private class CustomHeaderMatchingMessageSelectorGreaterThan7(
        string headerName,
        string headerValue,
        TestContext context,
        AtomicLong retries)
        : HeaderMatchingMessageSelector(headerName, headerValue, context)
    {
        public override bool Accept(IMessage message)
        {
            return retries.IncrementAndGet() > 7;
        }
    }
}