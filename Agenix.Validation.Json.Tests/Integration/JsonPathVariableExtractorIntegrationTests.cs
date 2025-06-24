using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Endpoint.Direct.Annotation;
using Agenix.Core.Message;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Validation.Json.Dsl.JsonPathSupport;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Json.Tests.Integration;

[NUnitAgenixSupport]
public class JsonPathVariableExtractorIT
{
    private const string JsonBody = "{\"user\":\"christoph\", \"age\": 32}";

    [AgenixEndpoint]
    [DirectEndpointConfig(Queue = "test")]
    private DirectEndpoint _direct;

    [BindToRegistry] private IMessageQueue test = new DefaultMessageQueue("test");

    [AgenixResource] protected ITestCaseRunner runner;

    [AgenixResource] protected TestContext context;

    [Test]
    public void ShouldPerformJsonPathVariableExtract()
    {
        runner.Given(Send(_direct)
            .Message()
            .Type(MessageType.JSON)
            .Body(JsonBody)
        );

        runner.Then(Receive(_direct)
            .Message()
            .Body(JsonBody)
            .Extract(JsonPath()
                .Expression("$.user", "user")
                .Expression("$.age", "age"))
        );

        Assert.That(context.GetVariable("user"), Is.EqualTo("christoph"));
        Assert.That(context.GetVariable<long>("age"), Is.EqualTo(32L));
    }

    [Test]
    public void ShouldFailOnJsonPathVariableExtract()
    {
        runner.Given(Send(_direct)
            .Message()
            .Type(MessageType.JSON)
            .Body(JsonBody)
        );

        Assert.Throws<TestCaseFailedException>(() =>
            runner.Then(Receive(_direct)
                .Message()
                .Body(JsonBody)
                .Extract(JsonPath().Expression("$.wrong", "user"))
            )
        );
    }

    [Test]
    public void ShouldPerformJsonPathValidationWithMultipleExpressions()
    {
        runner.Given(Send(_direct)
            .Message()
            .Type(MessageType.JSON)
            .Body(JsonBody)
        );

        runner.Then(Receive(_direct)
            .Message()
            .Body(JsonBody)
            .Extract(JsonPath().Expression("$.user", "user"))
            .Extract(JsonPath().Expression("$.age", "age"))
        );

        Assert.That(context.GetVariable("user"), Is.EqualTo("christoph"));
        Assert.That(context.GetVariable<long>("age"), Is.EqualTo(32L));
    }
}
