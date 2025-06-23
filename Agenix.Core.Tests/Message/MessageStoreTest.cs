using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Message;

/// <summary>
///     Test class for validating the functionality of the MessageStore within the Agenix framework.
/// </summary>
public class MessageStoreTest
{
    private readonly DirectEndpoint _directEndpoint = new DirectEndpointBuilder()
        .Queue(new DefaultMessageQueue("foo"))
        .Build();

    private TestContext _context;
    private ITestCaseRunner _t;

    [SetUp]
    public void SetupMethod()
    {
        _context = TestContextFactory.NewInstance().GetObject();
        _t = TestCaseRunnerFactory.CreateRunner(_context);

        _context.MessageValidatorRegistry.AddMessageValidator("simple", new SimpleMessageValidator());
    }

    [Test]
    public void ShouldStoreMessages()
    {
        _t.Run(Send()
            .Endpoint(_directEndpoint)
            .Message()
            .Name("request")
            .Body("Agenix rocks!"));

        ClassicAssert.NotNull(_context.MessageStore.GetMessage("request"));
        ClassicAssert.AreEqual("Agenix rocks!", _context.MessageStore.GetMessage("request").GetPayload<string>());

        _t.Run(Receive()
            .Endpoint(_directEndpoint)
            .Message()
            .Name("response")
            .Body("Agenix rocks!"));

        ClassicAssert.NotNull(_context.MessageStore.GetMessage("response"));
        ClassicAssert.AreEqual("Agenix rocks!", _context.MessageStore.GetMessage("response").GetPayload<string>());
    }

    [Test]
    public void ShouldStoreMessagesFromValidationCallback()
    {
        _t.Run(Send()
            .Endpoint(_directEndpoint)
            .Message()
            .Name("request")
            .Body("Agenix is awesome!"));

        ClassicAssert.NotNull(_context.MessageStore.GetMessage("request"));
        ClassicAssert.AreEqual("Agenix is awesome!", _context.MessageStore.GetMessage("request").GetPayload<string>());

        _t.Run(Receive()
            .Endpoint(_directEndpoint)
            .Message()
            .Name("response")
            .Validate((message, context) =>
            {
                var request = context.MessageStore.GetMessage("request");
                ClassicAssert.AreEqual(request.GetPayload<string>(), message.GetPayload<string>());
            }));


        ClassicAssert.NotNull(_context.MessageStore.GetMessage("response"));
        ClassicAssert.AreEqual("Agenix is awesome!", _context.MessageStore.GetMessage("response").GetPayload<string>());
    }

    private class SimpleMessageValidator : IMessageValidator<IValidationContext>
    {
        public void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
            List<IValidationContext> validationContexts)
        {
            ClassicAssert.AreEqual(receivedMessage.Payload, controlMessage.Payload);
            foreach (var ctx in validationContexts)
            {
                ctx.UpdateStatus(ValidationStatus.PASSED);
            }
        }

        public bool SupportsMessageType(string messageType, IMessage message)
        {
            return true;
        }
    }
}
