using Agenix.Core.Message;
using Agenix.Validation.Json.Message.Selector;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Message.Selector
{
    public class JsonPathPayloadMessageSelectorTest : AbstractNUnitSetUp
    {
        [Test]
        public void TestJsonPathEvaluation()
        {
            var messageSelector = new JsonPathPayloadMessageSelector("jsonPath:$.foo.text", "foobar", Context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(messageSelector.Accept(new DefaultMessage("{ \"foo\": { \"text\": \"foobar\" } }")), Is.True);
                Assert.That(messageSelector.Accept(new DefaultMessage("{ \"foo\": { \"text\": \"barfoo\" } }")), Is.False);
                Assert.That(messageSelector.Accept(new DefaultMessage("{ \"bar\": { \"text\": \"foobar\" } }")), Is.False);
                Assert.That(messageSelector.Accept(new DefaultMessage("This is plain text!")), Is.False);
            }
        }

        [Test]
        public void TestJsonPathEvaluationValidationMatcher()
        {
            var messageSelector = new JsonPathPayloadMessageSelector("jsonPath:$.foo.text", "@StartsWith(foo)@", Context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(messageSelector.Accept(new DefaultMessage("{ \"foo\": { \"text\": \"foobar\" } }")), Is.True);
                Assert.That(messageSelector.Accept(new DefaultMessage("{ \"foo\": { \"text\": \"barfoo\" } }")), Is.False);
                Assert.That(messageSelector.Accept(new DefaultMessage("{ \"bar\": { \"text\": \"foobar\" } }")), Is.False);
                Assert.That(messageSelector.Accept(new DefaultMessage("This is plain text!")), Is.False);
            }
        }

        [Test]
        public void TestJsonPathEvaluationWithMessageObjectPayload()
        {
            var messageSelector = new JsonPathPayloadMessageSelector("jsonPath:$.foo.text", "foobar", Context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(messageSelector.Accept(new DefaultMessage(new DefaultMessage("{ \"foo\": { \"text\": \"foobar\" } }"))), Is.True);
                Assert.That(messageSelector.Accept(new DefaultMessage(new DefaultMessage("{ \"foo\": { \"text\": \"barfoo\" } }"))), Is.False);
                Assert.That(messageSelector.Accept(new DefaultMessage(new DefaultMessage("{ \"bar\": { \"text\": \"foobar\" } }"))), Is.False);
                Assert.That(messageSelector.Accept(new DefaultMessage(new DefaultMessage("This is plain text!"))), Is.False);
            }
        }
    }
}
