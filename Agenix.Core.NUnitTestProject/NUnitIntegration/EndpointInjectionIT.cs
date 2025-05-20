using System;
using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Endpoint.Direct.Annotation;
using Agenix.Core.Message;
using Agenix.Core.Spi;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration;

[NUnitAgenixSupport]
public class EndpointInjectionIT
{
    [Test]
    public void InjectEndpoint()
    {
        ClassicAssert.NotNull(foo);

        _runner.Run(Send(directEndpoint)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello!"));

        _runner.Run(Receive(directEndpoint)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello!"));

        _runner.Run(Send("directEndpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hi!"));

        _runner.Run(Receive("directEndpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hi!"));

        _runner.Run(Send(foo)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello Agenix!"));

        _runner.Run(Receive(foo)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello Agenix!"));

        ClassicAssert.NotNull(agenix);
    }

    /// <summary>
    ///     Validates messages by comparing their payloads and determining if the message type is supported.
    /// </summary>
    private class AnonymousMessageValidator : IMessageValidator<IValidationContext>
    {
        public void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
            List<IValidationContext> validationContexts)
        {
            ClassicAssert.AreEqual(receivedMessage.GetPayload<string>(), controlMessage.GetPayload<string>());
            foreach (var ctx in validationContexts)
            {
                ctx.UpdateStatus(ValidationStatus.PASSED);
            }
        }

        public bool SupportsMessageType(string messageType, IMessage message)
        {
            return messageType.Equals(MessageType.PLAINTEXT.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }

    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private ITestActionRunner _runner;


    [AgenixFramework] private Agenix agenix;

    [AgenixEndpoint] [DirectEndpointConfig(QueueName = "FOO.direct.queue")] [BindToRegistry]
    private IEndpoint directEndpoint;

    [AgenixEndpoint] private IEndpoint foo;

    [BindToRegistry] private readonly IMessageQueue messages = new DefaultMessageQueue("messages");

    [BindToRegistry]
    public DirectEndpoint Foo()
    {
        return new DirectEndpointBuilder()
            .Queue(messages)
            .Build();
    }

    [BindToRegistry(Name = "FOO.direct.queue")]
    public IMessageQueue Queue()
    {
        return new DefaultMessageQueue("FOO.direct.queue");
    }

    [BindToRegistry(Name = "plaintextValidator")]
    public IMessageValidator<IValidationContext> PlaintextValidator()
    {
        return new AnonymousMessageValidator();
    }

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}