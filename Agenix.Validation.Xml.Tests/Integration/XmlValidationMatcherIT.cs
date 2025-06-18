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
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;

namespace Agenix.Validation.Xml.Tests.Integration;

/// <summary>
///     Test demonstrates XML validation using validation matchers within CDATA sections.
/// </summary>
[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class XmlValidationMatcherIT
{
    [AgenixResource] protected TestContext context;

    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint helloEndpoint;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    public void XmlValidationMatcherTest()
    {
        runner.Given(CreateVariables()
            .Variable("greetingText", "Hello Agenix")
        );

        const string sendData = """
                                <data>
                                  <greeting>Hello Agenix</greeting>
                                  <timestamp>2012-07-01T00:00:00</timestamp>
                                </data>
                                """;

        const string expectedData = """
                                    <data>
                                      <greeting>${greetingText}</greeting>
                                      <timestamp>@Ignore@</timestamp>
                                    </data>
                                    """;

        runner.When(Send("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body($"<testRequestMessage><text>agenix:CdataSection('{sendData}')</text></testRequestMessage>")
        );

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Type(MessageType.XML)
            .Body(
                $"<testRequestMessage><text>agenix:CdataSection('@matchesXml('{expectedData}')@')</text></testRequestMessage>")
        );
    }
}
