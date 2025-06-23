#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion


using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Core.Actions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Validation.Xml.Dsl;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class ValidateValuesWithNamespaceIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void ValidateValuesWithNamespaceTest()
    {
        runner.Given(CreateVariables()
            .Variable("correlationId", "agenix:randomNumber(10)")
            .Variable("messageId", "agenix:randomNumber(10)")
            .Variable("user", "Agenix")
        );

        runner.When(Send("hello.endpoint")
            .Name("Send asynchronous hello request: Agenix -> HelloService")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">\n" +
                  "    <MessageId>${messageId}</MessageId>\n" +
                  "    <CorrelationId>${correlationId}</CorrelationId>\n" +
                  "    <User>${user}</User>\n" +
                  "    <Text>Hello Agenix</Text>\n" +
                  "</HelloRequest>")
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Validate(XmlSupport.Xml()
                .Expression("//def:HelloRequest/def:MessageId", "${messageId}")
                .Expression("//def:HelloRequest/def:CorrelationId", "${correlationId}")
                .Expression("//def:HelloRequest/def:Text", "agenix:concat('Hello ', ${user})"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );


        // 3rd send/receive with default namespace prefix
        runner.When(Send("hello.endpoint")
            .Name("Send asynchronous hello request: Agenix -> HelloService")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">\n" +
                  "    <MessageId>${messageId}</MessageId>\n" +
                  "    <CorrelationId>${correlationId}</CorrelationId>\n" +
                  "    <User>${user}</User>\n" +
                  "    <Text>Hello Agenix</Text>\n" +
                  "</HelloRequest>")
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Validate(XmlSupport.Xml()
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='MessageId']", "${messageId}")
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='CorrelationId']", "${correlationId}")
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='Text']",
                    "agenix:concat('Hello ', ${user})")
                .Namespace("", "http://agenix.org/schemas/samples/HelloService.xsd"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );


        // 7th send/receive with extraction and C# assertion
        runner.When(Send("hello.endpoint")
            .Name("Send asynchronous hello request: Agenix -> HelloService")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">\n" +
                  "    <MessageId>${messageId}</MessageId>\n" +
                  "    <CorrelationId>${correlationId}</CorrelationId>\n" +
                  "    <User>${user}</User>\n" +
                  "    <Text>Hello Agenix</Text>\n" +
                  "</HelloRequest>")
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Validate(XmlSupport.Xml()
                .Expression("//def:HelloRequest/def:MessageId", "${messageId}")
                .Expression("//def:HelloRequest/def:CorrelationId", "${correlationId}")
                .Expression("//def:HelloRequest/def:Text", "agenix:concat('Hello ', ${user})"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
            .Extract(XpathSupport.Xpath().Expression("//def:HelloRequest/def:Text", "extractedText"))
        );

        runner.Then(DefaultTestActionBuilder.Action(ctx =>
        {
            Assert.That(ctx.GetVariable("extractedText"),
                Is.EqualTo(ctx.ReplaceDynamicContentInString("Hello ${user}")));
        }).Name("Check extracted text"));

        runner.Given(Echo("Test: Validation matcher value extraction"));

        runner.When(Send("hello.endpoint")
            .Name("Send asynchronous hello request: Agenix -> HelloService")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">\n" +
                  "    <MessageId>${messageId}</MessageId>\n" +
                  "    <CorrelationId>${correlationId}</CorrelationId>\n" +
                  "    <User>${user}</User>\n" +
                  "    <Text>Hello Agenix</Text>\n" +
                  "</HelloRequest>")
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">\n" +
                  "    <MessageId>${messageId}</MessageId>\n" +
                  "    <CorrelationId>${correlationId}</CorrelationId>\n" +
                  "    <User>@Variable('serviceName')@</User>\n" +
                  "    <Text>@Variable('extractedText')@</Text>\n" +
                  "</HelloRequest>")
            .Header("Operation", "@Variable()@")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(DefaultTestActionBuilder.Action(ctx =>
        {
            Assert.That(ctx.GetVariable("Operation"), Is.EqualTo("sayHello"));
            Assert.That(ctx.GetVariable("serviceName"), Is.EqualTo("Agenix"));
            Assert.That(ctx.GetVariable("extractedText"),
                Is.EqualTo(ctx.ReplaceDynamicContentInString("Hello ${user}")));
        }).Name("Check extracted text"));
    }
}
