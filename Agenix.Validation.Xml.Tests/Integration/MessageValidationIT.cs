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
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;

namespace Agenix.Validation.Xml.Tests.Integration;

/// <summary>
///     Test sends messages to a message channel and receives these messages performing validation.
/// </summary>
[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class MessageValidationIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void MessageValidationTest()
    {
        // Test validation success
        runner.Given(Echo("Test validation success"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        // Test validation success - auto select message type
        runner.Given(Echo("Test validation success - auto select message type"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        // Test header validation success
        runner.Given(Echo("Test header validation success"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Header("Operation", "sayHello")
        );

        runner.When(Send("hello.endpoint")
            .Message()
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Header("Operation", "sayHello")
        );

        // Test header validation success - auto select message type
        runner.Given(Echo("Test header validation success - auto select message type"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Header("Operation", "sayHello")
        );

        // Test validation errors
        runner.Given(Echo("Test validation errors"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Type(MessageType.XML)
                .Body("<Text>Goodbye Agenix</Text>")
                .Header("Operation", "sayHello")
            )
        );

        // Test validation errors - auto select message type
        runner.Given(Echo("Test validation errors - auto select message type"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Body("<Text>Goodbye Agenix</Text>")
                .Header("Operation", "sayHello")
            )
        );

        // Test header validation error
        runner.Given(Echo("Test header validation error"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Type(MessageType.XML)
                .Header("Operation", "sayGoodbye")
            )
        );

        runner.When(Send("hello.endpoint")
            .Message()
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Type(MessageType.XML)
                .Header("Operation", "sayGoodbye")
            )
        );

        // Test header validation error - auto select a message type
        runner.Given(Echo("Test header validation error - auto select message type"));

        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<Text>Hello Agenix</Text>")
            .Header("Operation", "sayHello")
        );

        runner.Then(Assert()
            .Exception(typeof(ValidationException))
            .When(Receive("hello.endpoint")
                .Message()
                .Header("Operation", "sayGoodbye")
            )
        );
    }
}
