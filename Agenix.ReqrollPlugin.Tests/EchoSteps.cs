
using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using Agenix.Core.Spi;
using Reqnroll;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;

[Binding]
[AgenixConfiguration(Classes = [typeof(Endpoints)])]
public class EchoSteps
{
    [AgenixResource]
    protected ITestCaseRunner runner;

    [AgenixResource]
    protected TestContext context;
    
    [AgenixEndpoint(Name = "EchoEndpoint")]
    private IEndpoint _directEndpoint;

    [Given(@"^My name is (.*)$")]
    public void MyNameIs(string name)
    {
        context.SetVariable("username", name);
    }

    [When(@"^I say hello.*")]
    public void SayHello()
    {
        runner.When(Send("EchoEndpoint")
            .Name("Send message to echo endpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello, my name is ${username}!"));
    }

    [When(@"^I say goodbye.*")]
    public void SayGoodbye()
    {
        runner.When(Send("EchoEndpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Goodbye from ${username}!"));
    }

    [Then(@"^the service should return: ""([^""]*)""$")]
    public void VerifyReturn(string body)
    {
        runner.Then(Receive("EchoEndpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body(body));
    }
    
    private class Endpoints
    {
        [BindToRegistry] private readonly IMessageQueue _messages = new DefaultMessageQueue("messages");

        [BindToRegistry(Name = "EchoEndpoint")]
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
}