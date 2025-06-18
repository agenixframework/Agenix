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
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Validation.Xml.Dsl;
using NUnit.Framework;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;

namespace Agenix.Validation.Xml.Tests.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(EndpointConfig)])]
public class HamcrestValidationMatcherIT
{
    [AgenixEndpoint(Name = "hello.endpoint")]
    private IEndpoint _helloEndpoint;

    [AgenixResource] protected TestContext context;

    [AgenixResource] protected ITestCaseRunner runner;

    [Test]
    [Description("Tests the @AssertThat()@ validator")]
    [Author("asuruceanu")]
    public void HamcrestValidationMatcherTest()
    {
        runner.When(Send("hello.endpoint")
            .Message()
            .Body("<HelloMessage>" +
                  "  <message>Hello foo!</message>" +
                  "</HelloMessage>"));

        runner.Then(Receive("hello.endpoint")
            .Message()
            .Body("<HelloMessage>" +
                  "  <message>@AssertThat(Not(EqualTo(bar)))@</message>" +
                  "</HelloMessage>")
            .Validate(XmlSupport.Xml()
                .Expression("/HelloMessage/message", "@AssertThat(ContainsString(foo!))@")
                .Expression("//message", "@AssertThat(HasSize(1))@")));
    }
}
