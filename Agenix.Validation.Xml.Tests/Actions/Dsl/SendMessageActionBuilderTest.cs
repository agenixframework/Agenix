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


using Agenix.Api.Endpoint;
using Agenix.Api.IO;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Report;
using Agenix.Api.Spi;
using Agenix.Api.Xml;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Xml;
using Agenix.Validation.Xml.Dsl;
using Agenix.Validation.Xml.Message.Builder;
using Agenix.Validation.Xml.Tests.Message;
using Agenix.Validation.Xml.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Actions.Dsl;

public class SendMessageActionBuilderTest : AbstractNUnitSetUp
{
    private readonly Xml2Marshaller _marshaller = new(typeof(TestRequest));
    private readonly Mock<IEndpoint> _messageEndpoint = new();
    private readonly Mock<IProducer> _messageProducer = new();
    private readonly Mock<IReferenceResolver> _referenceResolver = new();
    private readonly Mock<IResource> _resource = new();

    [SetUp]
    public void SetUp()
    {
        _marshaller.SetProperty("omitxmldeclaration", "true");
        _marshaller.SetProperty("stripnamespaces", "true");
        _marshaller.SetProperty("removeemptylines", "true");
    }

    [Test]
    public void TestSendBuilderWithPayloadModel()
    {
        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
            });

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.ResolveAll<IMarshaller>())
            .Returns(new Dictionary<string, IMarshaller> { { "marshaller", _marshaller } });
        _referenceResolver.Setup(x => x.Resolve<IMarshaller>()).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);

        runner.Run(SendMessageAction.Builder.Send(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.XML)
            .Body(new MarshallingPayloadBuilder(new TestRequest("Hello Agenix!"))));

        var testCase = runner.GetTestCase();
        Assert.That(testCase.GetActionCount(), Is.EqualTo(1));
        Assert.That(testCase.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)testCase.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageBuilder, Is.TypeOf<DefaultMessageBuilder>());

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(0));
    }

    [Test]
    public void TestSendBuilderWithPayloadModelExplicitMarshaller()
    {
        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
            });

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(SendMessageAction.Builder.Send(_messageEndpoint.Object)
            .Message()
            .Body(new MarshallingPayloadBuilder(new TestRequest("Hello Agenix!"), _marshaller)));

        var testCase = runner.GetTestCase();
        Assert.That(testCase.GetActionCount(), Is.EqualTo(1));
        Assert.That(testCase.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)testCase.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageBuilder, Is.TypeOf<DefaultMessageBuilder>());

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(0));
    }

    [Test]
    public void TestSendBuilderWithPayloadModelExplicitMarshallerName()
    {
        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
            });

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.IsResolvable("myMarshaller")).Returns(true);
        _referenceResolver.Setup(x => x.Resolve<IMarshaller>("myMarshaller")).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(SendMessageAction.Builder.Send(_messageEndpoint.Object)
            .Message()
            .Body(new MarshallingPayloadBuilder(new TestRequest("Hello Agenix!"), "myMarshaller")));

        var testCase = runner.GetTestCase();
        Assert.That(testCase.GetActionCount(), Is.EqualTo(1));
        Assert.That(testCase.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)testCase.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageBuilder, Is.TypeOf<DefaultMessageBuilder>());

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(0));
    }

    [Test]
    public void TestSendBuilderExtractFromPayload()
    {
        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>"));
            });

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(SendMessageAction.Builder.Send(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .Extract(XpathSupport.Xpath()
                .Expression("/TestRequest/Message", "text")
                .Expression("/TestRequest/Message/@lang", "language")
            ));

        Assert.That(Context.GetVariable("text"), Is.Not.Null);
        Assert.That(Context.GetVariable("language"), Is.Not.Null);
        Assert.That(Context.GetVariable("text"), Is.EqualTo("Hello World!"));
        Assert.That(Context.GetVariable("language"), Is.EqualTo("ENG"));

        var testCase = runner.GetTestCase();
        Assert.That(testCase.GetActionCount(), Is.EqualTo(1));
        Assert.That(testCase.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)testCase.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));

        Assert.That(action.VariableExtractors.Count, Is.EqualTo(1));
        Assert.That(action.VariableExtractors[0], Is.TypeOf<XpathPayloadVariableExtractor>());

        var extractor = (XpathPayloadVariableExtractor)action.VariableExtractors[0];
        Assert.That(extractor.Expressions.ContainsKey("/TestRequest/Message"), Is.True);
        Assert.That(extractor.Expressions.ContainsKey("/TestRequest/Message/@lang"), Is.True);
    }
}
