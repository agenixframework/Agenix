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
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Validation.Xml.Dsl;
using NUnit.Framework;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class ValidateXMLDataIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void ValidateXMLData()
    {
        runner.Given(CreateVariables()
            .Variable("correlationId", "agenix:randomNumber(10)")
            .Variable("messageId", "agenix:randomNumber(10)")
            .Variable("user", "Christoph")
        );

        // Test 1: Basic XML validation with ignore element
        runner.When(Send("direct:hello")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">" +
                  "<MessageId>${messageId}</MessageId>" +
                  "<CorrelationId>${correlationId}</CorrelationId>" +
                  "<User>${user}</User>" +
                  "<Text>Hello ${user}</Text>" +
                  "</HelloRequest>")
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(Receive("direct:hello")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">" +
                  "<MessageId>${messageId}</MessageId>" +
                  "<CorrelationId>${correlationId}</CorrelationId>" +
                  "<User>${user}</User>" +
                  "<Text>xxx</Text>" +
                  "</HelloRequest>")
            .Validate(XmlSupport.Xml()
                .Ignore("HelloRequest.Text"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        // Test 2: XML validation with namespace context and XPath ignore
        runner.When(Send("direct:hello")
            .Message()
            .Type(MessageType.XML)
            .Body("<HelloRequest xmlns=\"http://agenix.org/schemas/samples/HelloService.xsd\">" +
                  "<MessageId>${messageId}</MessageId>" +
                  "<CorrelationId>${correlationId}</CorrelationId>" +
                  "<User>${user}</User>" +
                  "<Text>Hello ${user}</Text>" +
                  "</HelloRequest>")
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        runner.Then(Receive("direct:hello")
            .Message()
            .Type(MessageType.XML)
            .Body("<ns0:HelloRequest xmlns:ns0=\"http://agenix.org/schemas/samples/HelloService.xsd\">" +
                  "<ns0:MessageId>${messageId}</ns0:MessageId>" +
                  "<ns0:CorrelationId>${correlationId}</ns0:CorrelationId>" +
                  "<ns0:User>${user}</ns0:User>" +
                  "<ns0:Text>xxx</ns0:Text>" +
                  "</ns0:HelloRequest>")
            .Validate(XmlSupport.Xml()
                .NamespaceContext("ns", "http://agenix.org/schemas/samples/HelloService.xsd")
                .Ignore("//ns:HelloRequest/ns:Text"))
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );
    }
}
