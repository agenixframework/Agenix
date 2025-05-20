using System.Net.Http;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using TestContext = Agenix.Api.Context.TestContext;
using Agenix.Core.Validation.Json;
using Agenix.Core.Validation.Xml;
using Agenix.Http.Actions;
using Agenix.Http.Client;
using Agenix.Http.Message;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Core.NUnitTestProject.Http.Actions.Dsl;

public class ReceiveHttpMessageTestActionBuilderTest : AbstractNUnitSetUp
{
    private readonly Mock<HttpEndpointConfiguration> _configuration = new();
    private readonly Mock<HttpClient> _httpClient = new();

    [Test]
    public void TestHttpRequestProperties()
    {
        Mock.Get(_httpClient.Object).Reset();
        Mock.Get(_configuration.Object).Reset();

        Mock.Get(_configuration.Object).Setup(x => x.Timeout).Returns(100L);
        _httpClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _httpClient.Setup(m => m.CreateConsumer()).Returns(_httpClient.Object);
        Mock.Get(_httpClient.Object).Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(
            new HttpMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .Method(HttpMethod.Get)
                .Path("/test/foo")
                .QueryParam("noValue")
                .QueryParam("param1", "value1")
                .QueryParam("param2", "value2"));

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(HttpActionBuilder.Http().Client(_httpClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Type(MessageType.XML)
        );

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(test.GetActionCount(), 1);
        ClassicAssert.AreEqual(test.GetActions()[0].GetType(), typeof(ReceiveMessageAction));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual(action.Name, "http:receive-response");

        ClassicAssert.AreEqual(action.ValidationContexts.Count, 2L);
        ClassicAssert.AreEqual(action.ValidationContexts[0].GetType(), typeof(HeaderValidationContext));
        ClassicAssert.AreEqual(action.ValidationContexts[1].GetType(), typeof(XmlMessageValidationContext));

        var messageBuilder = (HttpMessageBuilder)action.MessageBuilder;
        ClassicAssert.AreEqual(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            "<TestRequest><Message>Hello World!</Message></TestRequest>");
    }
}