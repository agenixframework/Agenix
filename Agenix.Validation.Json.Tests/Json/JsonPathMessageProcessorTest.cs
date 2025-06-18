using Agenix.Api.Exceptions;
using Agenix.Core.Message;
using Agenix.Validation.Json.Validation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonPathMessageProcessorTest : AbstractNUnitSetUp
{
    [Test]
    public void TestConstructWithJsonPath()
    {
        var message = new DefaultMessage("{ \"TestMessage\": { \"Text\": \"Hello World!\" }}");

        var jsonPathExpressions = new Dictionary<string, object> { { "$.TestMessage.Text", "Hello!" } };

        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        processor.ProcessMessage(message, Context);

        ClassicAssert.AreEqual("{\"TestMessage\":{\"Text\":\"Hello!\"}}", message.GetPayload<string>());
    }

    [Test]
    public void TestConstructWithJsonPathMultipleValues()
    {
        var message = new DefaultMessage("{ \"TestMessage\": { \"Text\": \"Hello World!\", \"Id\": 1234567}}");

        var jsonPathExpressions = new Dictionary<string, object>
        {
            { "$.TestMessage.Text", "Hello!" }, { "$.TestMessage.Id", "9999999" }
        };

        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        processor.ProcessMessage(message, Context);

        ClassicAssert.AreEqual("{\"TestMessage\":{\"Text\":\"Hello!\",\"Id\":9999999}}", message.GetPayload<string>());
    }

    [Test]
    public void TestConstructWithJsonPathWithArrays()
    {
        var message =
            new DefaultMessage(
                "{ \"TestMessage\": [{ \"Text\": \"Hello World!\" }, { \"Text\": \"Another Hello World!\" }]}");

        var jsonPathExpressions = new Dictionary<string, object> { { "$..Text", "Hello!" } };

        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        processor.ProcessMessage(message, Context);

        ClassicAssert.AreEqual("{\"TestMessage\":[{\"Text\":\"Hello!\"},{\"Text\":\"Hello!\"}]}",
            message.GetPayload<string>());
    }

    [Test]
    public void TestConstructWithJsonPathNoResult()
    {
        var message = new DefaultMessage("{ \"TestMessage\": { \"Text\": \"Hello World!\" }}");

        var jsonPathExpressions = new Dictionary<string, object> { { "$.TestMessage.Unknown", "Hello!" } };

        var processor = new JsonPathMessageProcessor.Builder()
            .IgnoreNotFound(true)
            .Expressions(jsonPathExpressions)
            .Build();

        processor.ProcessMessage(message, Context);

        ClassicAssert.AreEqual("{\"TestMessage\":{\"Text\":\"Hello World!\"}}", message.GetPayload<string>());
    }

    [Test]
    public void TestConstructFailOnUnknownJsonPath()
    {
        var message = new DefaultMessage("{ \"TestMessage\": { \"Text\": \"Hello World!\" }}");

        var jsonPathExpressions = new Dictionary<string, object> { { "$.TestMessage.Unknown", "Hello!" } };

        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        Assert.Throws<UnknownElementException>(() => processor.ProcessMessage(message, Context));
    }
}
