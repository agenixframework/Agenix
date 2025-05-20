using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using Agenix.Api.Annotations;
using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Http.Actions;
using Agenix.Http.Client;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Container.Async.Builder;
using static Agenix.Core.Variable.MessageHeaderVariableExtractor.Builder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration.Http;

[NUnitAgenixSupport]
public class AsyncHttpIT
{
    private const string fakeApiRequestUrl = "https://jsonplaceholder.typicode.com";

    private readonly HttpClient _client = new HttpClientBuilder()
        .RequestUrl(fakeApiRequestUrl)
        .Build();

    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void AsyncHttpPost()
    {
        _gherkin.When(Async().Actions(
                HttpActionBuilder.Http()
                    .Client(_client)
                    .Send()
                    .Post("/posts")
                    .Message()
                    .Body("{\"title\":\"foo\",\"body\":\"bar\",\"userId\":1}")
                    .ContentType(MediaTypeNames.Application.Json)
                    .Extract(FromHeaders().Header(MessageHeaders.Id, "request#1")),
                HttpActionBuilder.Http()
                    .Client(_client)
                    .Receive()
                    .Response(HttpStatusCode.Created)
                    .Message()
                    .Type(MessageType.JSON)
                    .Body("{\"title\":\"foo\",\"body\":\"bar\",\"userId\":1,\"id\": 101}")
                    .Selector(new Dictionary<string, object> { [MessageHeaders.Id] = "${request#1}" })
            )
        );

        _gherkin.When(Async().Actions(
                HttpActionBuilder.Http()
                    .Client(_client)
                    .Send()
                    .Post("/posts")
                    .Message()
                    .Body("{\"title\":\"foo1\",\"body\":\"bar1\",\"userId\":2}")
                    .ContentType(MediaTypeNames.Application.Json)
                    .Extract(FromHeaders().Header(MessageHeaders.Id, "request#2")),
                HttpActionBuilder.Http()
                    .Client(_client)
                    .Receive()
                    .Response(HttpStatusCode.Created)
                    .Message()
                    .Type(MessageType.JSON)
                    .Body("{\"title\":\"foo1\",\"body\":\"bar1\",\"userId\":2,\"id\": 101}")
                    .Selector(new Dictionary<string, object> { [MessageHeaders.Id] = "${request#2}" })
            )
        );
    }
}