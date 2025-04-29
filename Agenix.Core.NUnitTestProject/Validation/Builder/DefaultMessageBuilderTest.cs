using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Validation.Builder;

public class DefaultMessageBuilderTest : AbstractNUnitSetUp
{
    private const string resultingVariableTestPayload = "{ \"person\": { \"name\": \"Frauke\", \"age\": 20} }";

    private readonly string _initialVariableTestPayload = "{ \"person\": { \"name\": \"${name}\", \"age\": 20} }";
    private DefaultMessageBuilder _messageBuilder;

    [SetUp]
    public void SetUp()
    {
        _messageBuilder = new DefaultMessageBuilder();
        _messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("TestMessagePayload"));
    }

    [Test]
    public void TestMessageBuilder()
    {
        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessagePayload"));
    }

    [Test]
    public void TestMessageBuilderVariableSupport()
    {
        _messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("This ${placeholder} contains variables!"));
        Context.SetVariable("placeholder", "payload data");

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("This payload data contains variables!"));
    }

    [Test]
    public void TestVariablesInMessagePayloadsAreReplaced()
    {
        //GIVEN
        Context.SetVariable("name", "Frauke");
        _messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(_initialVariableTestPayload));

        //WHEN
        var message = _messageBuilder.Build(Context, MessageType.JSON.ToString());

        //THEN
        Assert.That(message.Payload, Is.EqualTo(resultingVariableTestPayload));
    }

    [Test]
    public void TestMessageBuilderWithPayloadResource()
    {
        var textPayloadResource =
            $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest.validation.builder/payload-data-resource.txt";

        _messageBuilder = new DefaultMessageBuilder();
        _messageBuilder.SetPayloadBuilder(new FileResourcePayloadBuilder(textPayloadResource));

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessageData"));
    }

    [Test]
    public void TestMessageBuilderWithPayloadResourceVariableSupport()
    {
        var textPayloadResource =
            $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest.validation.builder/variable-data-resource.txt";

        _messageBuilder = new DefaultMessageBuilder();
        _messageBuilder.SetPayloadBuilder(new FileResourcePayloadBuilder(textPayloadResource));
        Context.SetVariable("placeholder", "payload data");

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("This payload data contains variables!"));
    }

    [Test]
    public void TestMessageBuilderWithHeaders()
    {
        var headers = new Dictionary<string, object> { { "operation", "unitTesting" } };
        _messageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessagePayload"));
        Assert.That(resultingMessage.GetHeader("operation"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("operation"), Is.EqualTo("unitTesting"));
    }

    [Test]
    [Platform(Exclude = "MacOsX", Reason = "Only runs on non-Linux/ Unix platforms.")]
    public void TestMessageBuilderWithHeaderTypes()
    {
        
        Console.WriteLine(Environment.OSVersion.Platform);
        var headers = new Dictionary<string, object>
        {
            { "intValue", "{int}:5" },
            { "longValue", "{long}:5" },
            { "floatValue", "{float}:5.0" },
            { "doubleValue", "{double}:5.0" },
            { "boolValue", "{bool}:true" },
            { "shortValue", "{short}:5" },
            { "byteValue", "{byte}:1" },
            { "stringValue", "{string}:5.0" }
        };
        _messageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));
        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessagePayload"));
        Assert.That(resultingMessage.GetHeader("intValue"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("intValue"), Is.EqualTo(5));
        Assert.That(resultingMessage.GetHeader("longValue"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("longValue"), Is.EqualTo(5L));
        Assert.That(resultingMessage.GetHeader("floatValue"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("floatValue"), Is.EqualTo(5.0F));
        Assert.That(resultingMessage.GetHeader("doubleValue"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("doubleValue"), Is.EqualTo(5.0));
        Assert.That(resultingMessage.GetHeader("boolValue"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("boolValue"), Is.True);
        Assert.That(resultingMessage.GetHeader("shortValue"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("shortValue"), Is.EqualTo(5));
        Assert.That(resultingMessage.GetHeader("byteValue"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("byteValue"), Is.EqualTo(1));
        Assert.That(resultingMessage.GetHeader("stringValue"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("stringValue"), Is.EqualTo("5.0"));
    }

    [Test]
    public void TestMessageBuilderWithHeadersVariableSupport()
    {
        var headers = new Dictionary<string, object> { { "operation", "${operation}" } };

        _messageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));

        Context.SetVariable("operation", "unitTesting");

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessagePayload"));
        Assert.That(resultingMessage.GetHeader("operation"), Is.Not.Null);
        Assert.That(resultingMessage.GetHeader("operation"), Is.EqualTo("unitTesting"));
    }

    [Test]
    public void TestMessageBuilderWithHeaderData()
    {
        _messageBuilder.AddHeaderBuilder(new DefaultHeaderDataBuilder("MessageHeaderData"));

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessagePayload"));
        Assert.That(resultingMessage.GetHeaderData().Count, Is.EqualTo(1L));
        Assert.That(resultingMessage.GetHeaderData()[0], Is.EqualTo("MessageHeaderData"));
    }

    [Test]
    public void TestMessageBuilderWithMultipleHeaderData()
    {
        _messageBuilder.AddHeaderBuilder(new DefaultHeaderDataBuilder("MessageHeaderData1"));
        _messageBuilder.AddHeaderBuilder(new DefaultHeaderDataBuilder("MessageHeaderData2"));

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessagePayload"));
        Assert.That(resultingMessage.GetHeaderData().Count, Is.EqualTo(2L));
        Assert.That(resultingMessage.GetHeaderData()[0], Is.EqualTo("MessageHeaderData1"));
        Assert.That(resultingMessage.GetHeaderData()[1], Is.EqualTo("MessageHeaderData2"));
    }

    [Test]
    public void TestMessageBuilderWithHeaderDataVariableSupport()
    {
        _messageBuilder.AddHeaderBuilder(new DefaultHeaderDataBuilder("This ${placeholder} contains variables!"));
        Context.SetVariable("placeholder", "header data");

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessagePayload"));
        Assert.That(resultingMessage.GetHeaderData().Count, Is.EqualTo(1L));
        Assert.That(resultingMessage.GetHeaderData()[0], Is.EqualTo("This header data contains variables!"));
    }

    [Test]
    public void TestMessageBuilderWithHeaderResource()
    {
        var headerResource = $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}" +
                             $".ResourcesTest.validation.builder/header-data-resource.txt";
        _messageBuilder.AddHeaderBuilder(new FileResourceHeaderDataBuilder(headerResource));

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessagePayload"));
        Assert.That(resultingMessage.GetHeaderData().Count, Is.EqualTo(1L));
        Assert.That(resultingMessage.GetHeaderData()[0], Is.EqualTo("MessageHeaderData"));
    }

    [Test]
    public void TestMessageBuilderWithHeaderResourceVariableSupport()
    {
        var headerResource = $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}" +
                             $".ResourcesTest.validation.builder/variable-data-resource.txt";
        _messageBuilder.AddHeaderBuilder(new FileResourceHeaderDataBuilder(headerResource));
        Context.SetVariable("placeholder", "header data");

        var resultingMessage = _messageBuilder.Build(Context, CoreSettings.DefaultMessageType());

        Assert.That(resultingMessage.Payload, Is.EqualTo("TestMessagePayload"));
        Assert.That(resultingMessage.GetHeaderData().Count, Is.EqualTo(1L));
        Assert.That(resultingMessage.GetHeaderData()[0], Is.EqualTo("This header data contains variables!"));
    }
}