using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;

namespace Agenix.Core.Tests.NUnitIntegration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(Endpoints)])]
public class AgenixConfigurationIT
{
    [Test]
    public void ShouldLoadConfiguration()
    {
        ClassicAssert.IsNotNull(_foo);

        _runner.Run(Send(_directEndpoint)
            .Name("Send-direct")
            .Description("Send a message to the direct endpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello!")
        );

        _runner.Run(Receive(_directEndpoint)
            .Name("Receive-direct")
            .Description("Receive a message from the direct endpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello!")
        );

        _runner.Run(Send("DirectEndpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hi!")
        );

        _runner.Run(Receive("DirectEndpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hi!")
        );

        _runner.Run(Send(_foo)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello again!")
        );

        _runner.Run(Receive(_foo)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello again!")
        );

        ClassicAssert.IsNotNull(_agenix);
    }

    private class Endpoints
    {
        [BindToRegistry] private readonly IMessageQueue _messages = new DefaultMessageQueue("messages");

        [BindToRegistry(Name = "DirectEndpoint")]
        private IEndpoint _directEndpoint = new DirectEndpointBuilder()
            .Queue("TEST.direct.queue")
            .Build();

        [BindToRegistry]
        public DirectEndpoint Foo()
        {
            return new DirectEndpointBuilder()
                .Queue(_messages)
                .Build();
        }

        [BindToRegistry(Name = "TEST.direct.queue")]
        private IMessageQueue Queue()
        {
            return new DefaultMessageQueue("FOO.direct.queue");
        }
    }
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixFramework] private Agenix _agenix;


    [AgenixEndpoint(Name = "DirectEndpoint")]
    private IEndpoint _directEndpoint;

    [AgenixEndpoint(Name = "Foo")] private IEndpoint _foo;

    [AgenixResource] private ITestActionRunner _runner;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}
