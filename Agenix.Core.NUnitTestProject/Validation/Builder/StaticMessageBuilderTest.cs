using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Validation.Builder;

public class StaticMessageBuilderTest : AbstractNUnitSetUp
{
    private StaticMessageBuilder _staticMessageBuilder;

    [Test]
    public void TestBuildMessageContent()
    {
        var testMessage = new DefaultMessage("TestMessage").SetHeader("header1", "value1");
        _staticMessageBuilder = new StaticMessageBuilder(testMessage);

        var resultingMessage = _staticMessageBuilder.Build(Context, MessageType.PLAINTEXT.ToString());

        Assert.That(resultingMessage.Payload, Is.EqualTo(testMessage.Payload));
        Assert.That(resultingMessage.GetHeader(MessageHeaders.Id),
            Is.Not.EqualTo(testMessage.GetHeader(MessageHeaders.Id)));
        Assert.That(resultingMessage.GetHeaders().Count, Is.EqualTo(testMessage.GetHeaders().Count + 1));
        Assert.That(resultingMessage.GetHeader("header1"), Is.EqualTo(testMessage.GetHeader("header1")));
        Assert.That(resultingMessage.GetType(), Is.EqualTo(MessageType.PLAINTEXT.ToString()));
    }

    [Test]
    public void TestBuildMessageContentWithAdditionalHeader()
    {
        var testMessage = new DefaultMessage("TestMessage").SetHeader("header1", "value1");
        _staticMessageBuilder = new StaticMessageBuilder(testMessage);

        _staticMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(new Dictionary<string, object>
        {
            { "additional", "new" }
        }));

        var resultingMessage = _staticMessageBuilder.Build(Context, MessageType.PLAINTEXT.ToString());
        Assert.That(resultingMessage.Payload, Is.EqualTo(testMessage.Payload));
        Assert.That(resultingMessage.GetHeader(MessageHeaders.Id),
            Is.Not.EqualTo(testMessage.GetHeader(MessageHeaders.Id)));
        Assert.That(resultingMessage.GetHeader("additional"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("additional"), Is.EqualTo("new"));
    }

    [Test]
    public void TestBuildMessageContentWithAdditionalHeaderData()
    {
        var testMessage = new DefaultMessage("TestMessage").SetHeader("header1", "value1");
        _staticMessageBuilder = new StaticMessageBuilder(testMessage);

        _staticMessageBuilder.AddHeaderBuilder(new DefaultHeaderDataBuilder("TestMessageData"));

        var resultingMessage = _staticMessageBuilder.Build(Context, MessageType.PLAINTEXT.ToString());
        Assert.That(resultingMessage.Payload, Is.EqualTo(testMessage.Payload));
        Assert.That(resultingMessage.GetHeader(MessageHeaders.Id),
            Is.Not.EqualTo(testMessage.GetHeader(MessageHeaders.Id)));
        Assert.That(resultingMessage.GetHeaderData().Count, Is.EqualTo(1L));
        Assert.That(resultingMessage.GetHeaderData()[0], Is.EqualTo("TestMessageData"));
    }

    [Test]
    public void TestBuildMessageContentWithMultipleHeaderData()
    {
        var testMessage = new DefaultMessage("TestMessage").SetHeader("header1", "value1");
        _staticMessageBuilder = new StaticMessageBuilder(testMessage);

        _staticMessageBuilder.AddHeaderBuilder(new DefaultHeaderDataBuilder("TestMessageData1"));
        _staticMessageBuilder.AddHeaderBuilder(new DefaultHeaderDataBuilder("TestMessageData2"));

        var resultingMessage = _staticMessageBuilder.Build(Context, MessageType.PLAINTEXT.ToString());
        Assert.That(resultingMessage.Payload, Is.EqualTo(testMessage.Payload));
        Assert.That(resultingMessage.GetHeader("header1"), Is.EqualTo(testMessage.GetHeader("header1")));
        Assert.That(resultingMessage.GetHeader(MessageHeaders.Id),
            Is.Not.EqualTo(testMessage.GetHeader(MessageHeaders.Id)));
        Assert.That(resultingMessage.GetHeaderData().Count, Is.EqualTo(2L));
        Assert.That(resultingMessage.GetHeaderData()[0], Is.EqualTo("TestMessageData1"));
        Assert.That(resultingMessage.GetHeaderData()[1], Is.EqualTo("TestMessageData2"));
    }

    [Test]
    public void TestBuildMessageContentWithAdditionalHeaderResource()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var headerResource = "file:" + testDirectory + @"\ResourcesTest\validation\builder\payload-data-resource.txt";

        var testMessage = new DefaultMessage("TestMessage").SetHeader("header1", "value1");
        _staticMessageBuilder = new StaticMessageBuilder(testMessage);
        _staticMessageBuilder.AddHeaderBuilder(new FileResourceHeaderDataBuilder(headerResource));

        var resultingMessage = _staticMessageBuilder.Build(Context, MessageType.PLAINTEXT.ToString());
        Assert.That(resultingMessage.Payload, Is.EqualTo(testMessage.Payload));
        Assert.That(resultingMessage.GetHeader("header1"), Is.EqualTo(testMessage.GetHeader("header1")));
        Assert.That(resultingMessage.GetHeader(MessageHeaders.Id),
            Is.Not.EqualTo(testMessage.GetHeader(MessageHeaders.Id)));
        Assert.That(resultingMessage.GetHeaderData().Count, Is.EqualTo(1L));
        Assert.That(resultingMessage.GetHeaderData()[0], Is.EqualTo("TestMessageData"));
    }

    [Test]
    public void TestBuildMessageContentWithVariableSupport()
    {
        Context.SetVariable("payload", "TestMessage");
        Context.SetVariable("header", "value1");

        var testMessage = new DefaultMessage("${payload}").SetHeader("header1", "${header}");
        _staticMessageBuilder = new StaticMessageBuilder(testMessage);

        var resultingMessage = _staticMessageBuilder.Build(Context, MessageType.PLAINTEXT.ToString());
        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessage"));
        Assert.That(resultingMessage.GetHeader("header1"), Is.EqualTo("value1"));
        Assert.That(resultingMessage.GetHeader(MessageHeaders.Id),
            Is.Not.EqualTo(testMessage.GetHeader(MessageHeaders.Id)));
    }

    [Test]
    public void TestBuildMessageContentWithObjectPayload()
    {
        //GIVEN
        var testMessage = new DefaultMessage(1000).SetHeader("header1", 1000);
        _staticMessageBuilder = new StaticMessageBuilder(testMessage);

        //WHEN
        var resultingMessage = _staticMessageBuilder.Build(Context, MessageType.PLAINTEXT.ToString());

        //THEN
        Assert.That(resultingMessage.Payload, Is.EqualTo(testMessage.Payload));
        Assert.That(resultingMessage.GetHeader("header1"), Is.EqualTo(testMessage.GetHeader("header1")));
        Assert.That(resultingMessage.GetHeader(MessageHeaders.Id),
            Is.Not.EqualTo(testMessage.GetHeader(MessageHeaders.Id)));
    }

    [Test]
    public void TestNullValueInHeaders()
    {
        //GIVEN
        _staticMessageBuilder = new StaticMessageBuilder(new DefaultMessage()
            .SetHeader("foo", "bar")
            .SetHeader("bar", null));

        //WHEN
        var headers = _staticMessageBuilder.BuildMessageHeaders(Context);

        //THEN
        Assert.That(headers.ContainsKey("bar"), Is.True);
        Assert.That(headers.ContainsKey("foo"), Is.True);
        Assert.That(headers["bar"], Is.Null);
        Assert.That(headers["foo"], Is.Not.Null);
        Assert.That(headers["foo"], Is.EqualTo("bar"));
    }
}