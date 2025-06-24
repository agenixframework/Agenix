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
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;

namespace Agenix.Validation.Xml.Tests.Integration;

/// <summary>
///     This test shows the usage of inline test variables. In xml data definition you can
///     use the escape sequence ${variable_name} to add variable values to the xml template.
///     The parameter "variable_name" will be the name of a valid test variable or a test function.
/// </summary>
[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class InlineVariablesIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void InlineVariablesTest()
    {
        // Define test variables
        runner.Given(CreateVariables()
            .Variable("text", "Hallo")
            .Variable("text2", "Test Framework")
        );

        // First send/receive cycle with dynamic content
        runner.When(Send("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <xml>
                    <message>${text}</message>
                    <message>${text2}</message>
                    <message>agenix:concat(${text}, ' Test', ' Framework!')</message>
                    <message>agenix:upperCase('klein')</message>
                    <message>Text is: agenix:lowerCase('GROSS')</message>
                    <message>${text} ${text2}</message>
                  </xml>
                  """)
            .Header("operation", "Greetings")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <xml>
                    <message>Hallo</message>
                    <message>Test Framework</message>
                    <message>Hallo Test Framework!</message>
                    <message>KLEIN</message>
                    <message>Text is: gross</message>
                    <message>Hallo Test Framework</message>
                  </xml>
                  """)
            .Header("operation", "Greetings")
        );

        // Second send/receive cycle with resolved values
        runner.When(Send("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <xml>
                    <message>Hallo</message>
                    <message>Test Framework</message>
                    <message>Hallo Test Framework!</message>
                    <message>KLEIN</message>
                    <message>Text is: gross</message>
                    <message>Hallo Test Framework</message>
                  </xml>
                  """)
            .Header("operation", "Greetings")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body("""
                  <xml>
                    <message>${text}</message>
                    <message>${text2}</message>
                    <message>agenix:concat(${text}, ' Test', ' Framework!')</message>
                    <message>agenix:upperCase('klein')</message>
                    <message>Text is: agenix:lowerCase('GROSS')</message>
                    <message>${text} ${text2}</message>
                  </xml>
                  """)
            .Header("operation", "Greetings")
        );
    }
}
