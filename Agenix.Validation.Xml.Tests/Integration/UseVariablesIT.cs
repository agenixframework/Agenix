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
using Agenix.Core.Dsl;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Validation.Xml.Dsl;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Actions.TraceVariablesAction.Builder;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Validation.PathExpressionValidationContext.Builder;

namespace Agenix.Validation.Xml.Tests.Integration;

/// <summary>
///     This test shows how to use test variables.
///     Variables can be defined and referenced throughout the test workflow.
///     They can be used for dynamic content generation, message validation,
///     and storing extracted values from received messages.
/// </summary>
[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class UseVariablesIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void UseVariablesTest()
    {
        // Define test variables with random values
        runner.Given(CreateVariables()
            .Variable("correlationId", "agenix:randomNumber(10)")
            .Variable("messageId", "agenix:randomNumber(10)")
        );

        // Send asynchronous hello request: Agenix -> HelloService
        runner.When(Send("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <HelloRequest>
                      <MessageId>${messageId}</MessageId>
                      <CorrelationId>${correlationId}</CorrelationId>
                      <User>Christoph</User>
                      <Text>Hello Agenix</Text>
                  </HelloRequest>
                  """)
            .Header("Operation", "sayHello")
            .Header("CorrelationId", "${correlationId}")
        );

        // Receive message and extract values into new variables
        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Validate(PathExpression().Xpath("//HelloRequest/CorrelationId", "${correlationId}").Build())
            .Extract(MessageSupport.MessageHeaderSupport.FromHeaders()
                .Header("Operation", "operation")
                .Header("CorrelationId", "id")
            )
            .Extract(XpathSupport.Xpath()
                .Expression("//*[local-name()='HelloRequest']/*[local-name()='User']", "user")
                .AsExtractor())
        );

        // Echo the extracted operation variable
        runner.Then(Echo("${operation}"));

        // Trace all variables to see their current values
        runner.Then(TraceVariables()
            .Variable("id")
            .Variable("correlationId")
            .Variable("operation")
            .Variable("messageId")
            .Variable("user")
        );
    }
}
