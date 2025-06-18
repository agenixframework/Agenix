using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Json;
using Moq;
using NHamcrest;
using NHamcrest.Core;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Validation.Json.JsonMessageValidationContext.Builder;
using static Moq.It;
using Is = NHamcrest.Is;
using Contains = NHamcrest.Contains;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.NHamcrest.Tests.Actions;

public class ReceiveMessageTestActionBuilderTest : AbstractNUnitSetUp
{
    private readonly IEndpointConfiguration _configuration = Mock.Of<IEndpointConfiguration>();
    private readonly IConsumer _messageConsumer = Mock.Of<IConsumer>();
    private readonly IEndpoint _messageEndpoint = Mock.Of<IEndpoint>();

    [Test]
    public void TestReceiveBuilderWithJsonPathExpressions()
    {
        Mock.Get(_messageEndpoint).Reset();
        Mock.Get(_messageConsumer).Reset();
        Mock.Get(_configuration).Reset();

        Mock.Get(_messageEndpoint).Setup(m => m.CreateConsumer()).Returns(_messageConsumer);
        Mock.Get(_messageEndpoint).Setup(m => m.EndpointConfiguration).Returns(_configuration);
        Mock.Get(_configuration).Setup(c => c.Timeout).Returns(100L);
        Mock.Get(_messageConsumer).Setup(m => m.Receive(IsAny<TestContext>(), IsAny<long>())).Returns(
            new DefaultMessage(
                    "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\",\"active\": true}, \"index\":5, \"id\":\"x123456789x\"}")
                .SetHeader("operation", "sayHello"));

        Context.MessageValidatorRegistry.AddMessageValidator("jsonPathMessageValidator", new JsonPathValidator());


        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(Receive().Endpoint(_messageEndpoint)
            .Message()
            .Type(MessageType.JSON)
            .Body(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\",\"active\": true}, \"index\":5, \"id\":\"x123456789x\"}")
            .Validate(Json()
                .Expression("$.person.name", "John")
                .Expression("$.person.active", true)
                .Expression("$.id", Matches.AnyOf(Contains.String("123456789"), Is.Null()))
                .Expression("$.text", "Hello World!")
                .Expression("$.index", 5)
                .Build()
            )
            .Build
        );

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(ReceiveMessageAction), test.GetActions()[0].GetType());

        var action = (ReceiveMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("receive", action.Name);

        ClassicAssert.AreEqual(nameof(MessageType.JSON), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint, action.Endpoint);
        ClassicAssert.AreEqual(3, action.ValidationContexts.Count);
        ClassicAssert.IsTrue(action.ValidationContexts.Exists(c => c is HeaderValidationContext));
        ClassicAssert.IsTrue(action.ValidationContexts.Exists(c => c is JsonMessageValidationContext));
        ClassicAssert.IsTrue(action.ValidationContexts.Exists(c => c is JsonPathMessageValidationContext));

        var validationContext =
            action.ValidationContexts.FirstOrDefault(c => c is JsonPathMessageValidationContext) as
                JsonPathMessageValidationContext;
        if (validationContext == null)
        {
            throw new AssertionException("Missing validation context");
        }

        ClassicAssert.IsTrue(action.MessageBuilder is DefaultMessageBuilder);
        ClassicAssert.AreEqual(5, validationContext.JsonPathExpressions.Count);
        ClassicAssert.AreEqual("John", validationContext.JsonPathExpressions["$.person.name"]);
        ClassicAssert.AreEqual(true, validationContext.JsonPathExpressions["$.person.active"]);
        ClassicAssert.AreEqual("Hello World!", validationContext.JsonPathExpressions["$.text"]);
        ClassicAssert.AreEqual(5, validationContext.JsonPathExpressions["$.index"]);
        ClassicAssert.AreEqual(typeof(AnyOfMatcher<string>), validationContext.JsonPathExpressions["$.id"].GetType());
    }

    public class JsonPathValidator : DefaultMessageValidator
    {
        public override IValidationContext FindValidationContext(List<IValidationContext> validationContexts)
        {
            var validationContext = validationContexts
                .FirstOrDefault(x => x is JsonPathMessageValidationContext);

            return validationContext ?? base.FindValidationContext(validationContexts);
        }
    }
}
