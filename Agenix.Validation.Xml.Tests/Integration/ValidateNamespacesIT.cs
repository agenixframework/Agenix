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
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Validation.Xml.Dsl;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class ValidateNamespacesIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void ValidateNamespaces()
    {
        runner.Given(Echo("Test: Success with single namespace validation"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<Message>Hello</Message>" +
                  "</trq:TestRequest>")
        );

        runner.Then(Receive("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<Message>Hello</Message>" +
                  "</trq:TestRequest>")
            .Validate(XmlSupport.Xml()
                .Namespace("trq", "http://agenix.org/schemas/test")
                .SchemaValidation(false))
            .Timeout(TimeSpan.FromSeconds(5).Seconds)
        );

        runner.Given(Echo("Test: Success with multiple namespace validations"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body(
                "<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\" xmlns:msg=\"http://agenix.org/schemas/message\">" +
                "<msg:Message>Hello</msg:Message>" +
                "</trq:TestRequest>")
        );

        runner.Then(Receive("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body(
                "<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\" xmlns:msg=\"http://agenix.org/schemas/message\">" +
                "<msg:Message>Hello</msg:Message>" +
                "</trq:TestRequest>")
            .Validate(XmlSupport.Xml()
                .Namespace("trq", "http://agenix.org/schemas/test")
                .Namespace("msg", "http://agenix.org/schemas/message")
                .SchemaValidation(false))
            .Timeout(TimeSpan.FromSeconds(5).Seconds)
        );

        runner.Given(Echo("Test: Success with multiple nested namespace validations"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<msg:Message xmlns:msg=\"http://agenix.org/schemas/message\">Hello</msg:Message>" +
                  "</trq:TestRequest>")
        );

        runner.Then(Receive("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<msg:Message xmlns:msg=\"http://agenix.org/schemas/message\">Hello</msg:Message>" +
                  "</trq:TestRequest>")
            .Validate(XmlSupport.Xml()
                .Namespace("trq", "http://agenix.org/schemas/test")
                .Namespace("msg", "http://agenix.org/schemas/message")
                .SchemaValidation(false))
            .Timeout(TimeSpan.FromSeconds(5).Seconds)
        );

        runner.Given(Echo("Test: Failure because of missing namespace"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                  "<Message>Hello</Message>" +
                  "</trq:TestRequest>")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("direct:test")
                .Message()
                .Type(MessageType.XML)
                .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                      "<Message>Hello</Message>" +
                      "</trq:TestRequest>")
                .Validate(XmlSupport.Xml()
                    .Namespace("trq", "http://agenix.org/schemas/test")
                    .Namespace("missing", "http://agenix.org/schemas/missing")
                    .SchemaValidation(false))
                .Timeout(TimeSpan.FromSeconds(5).Seconds)
            )
        );

        runner.Given(Echo("Test: Failure because of wrong namespace prefix"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<wrong:TestRequest xmlns:wrong=\"http://agenix.org/schemas/test\">" +
                  "<Message>Hello</Message>" +
                  "</wrong:TestRequest>")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("direct:test")
                .Message()
                .Type(MessageType.XML)
                .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                      "<Message>Hello</Message>" +
                      "</trq:TestRequest>")
                .Validate(XmlSupport.Xml()
                    .Namespace("trq", "http://agenix.org/schemas/test")
                    .SchemaValidation(false))
                .Timeout(TimeSpan.FromSeconds(5).Seconds)
            )
        );

        runner.Given(Echo("Test: Failure because of wrong namespace uri"));

        runner.When(Send("direct:test")
            .Message()
            .Type(MessageType.XML)
            .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/wrong\">" +
                  "<Message>Hello</Message>" +
                  "</trq:TestRequest>")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("direct:test")
                .Message()
                .Type(MessageType.XML)
                .Body("<trq:TestRequest xmlns:trq=\"http://agenix.org/schemas/test\">" +
                      "<Message>Hello</Message>" +
                      "</trq:TestRequest>")
                .Validate(XmlSupport.Xml()
                    .Namespace("trq", "http://agenix.org/schemas/test")
                    .SchemaValidation(false))
                .Timeout(TimeSpan.FromSeconds(5).Seconds)
            )
        );
    }
}
