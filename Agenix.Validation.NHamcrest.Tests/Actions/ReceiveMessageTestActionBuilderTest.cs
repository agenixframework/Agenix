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
using Contains = NHamcrest.Contains;
using Is = NHamcrest.Is;
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
        Assert.That(test.GetActionCount(), NUnit.Framework.Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), NUnit.Framework.Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, NUnit.Framework.Is.EqualTo("receive"));

        Assert.That(action.MessageType, NUnit.Framework.Is.EqualTo(nameof(MessageType.JSON)));
        Assert.That(action.Endpoint, NUnit.Framework.Is.EqualTo(_messageEndpoint));
        Assert.That(action.ValidationContexts.Count, NUnit.Framework.Is.EqualTo(3));
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
        Assert.That(validationContext.JsonPathExpressions.Count, NUnit.Framework.Is.EqualTo(5));
        Assert.That(validationContext.JsonPathExpressions["$.person.name"], NUnit.Framework.Is.EqualTo("John"));
        Assert.That(validationContext.JsonPathExpressions["$.person.active"], NUnit.Framework.Is.EqualTo(true));
        Assert.That(validationContext.JsonPathExpressions["$.text"], NUnit.Framework.Is.EqualTo("Hello World!"));
        Assert.That(validationContext.JsonPathExpressions["$.index"], NUnit.Framework.Is.EqualTo(5));
        Assert.That(validationContext.JsonPathExpressions["$.id"].GetType(), NUnit.Framework.Is.EqualTo(typeof(AnyOfMatcher<string>)));
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
