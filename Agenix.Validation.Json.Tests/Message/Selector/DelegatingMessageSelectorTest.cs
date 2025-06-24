using Agenix.Core.Message;
using Agenix.Core.Message.Selector;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Message.Selector
{
    public class DelegatingMessageSelectorTest : AbstractNUnitSetUp
    {
        [Test]
        public void TestJsonPathEvaluationDelegation()
        {
            var messageSelector = new DelegatingMessageSelector("foo = 'bar' AND jsonPath:$.foo.text = 'foobar'", Context);

            var acceptMessage = new DefaultMessage("{ \"foo\": { \"text\": \"foobar\"} }")
                .SetHeader("foo", "bar")
                .SetHeader("operation", "foo");

            var declineMessage = new DefaultMessage("{ \"foo\": { \"text\": \"barfoo\"} }")
                .SetHeader("foo", "bar")
                .SetHeader("operation", "foo");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(messageSelector.Accept(acceptMessage), Is.True);
                Assert.That(messageSelector.Accept(declineMessage), Is.False);
            }

            messageSelector = new DelegatingMessageSelector("jsonPath:$.foo.text = 'foobar'", Context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(messageSelector.Accept(acceptMessage), Is.True);
                Assert.That(messageSelector.Accept(declineMessage), Is.False);
            }
        }
    }
}
