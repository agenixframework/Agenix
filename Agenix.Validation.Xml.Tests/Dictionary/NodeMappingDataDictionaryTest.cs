using Agenix.Api.Variable.Dictionary;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Validation.Xml.Variable.Dictionary.Xml;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Dictionary;

public class NodeMappingDataDictionaryTest : AbstractNUnitSetUp
{
    [Test]
    public void TestTranslateExactMatchStrategy()
    {
        var message =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text>Hello World!</Text><OtherText>No changes</OtherText></TestMessage>");

        var mappings = new Dictionary<string, string>
        {
            { "Something.Else", "NotFound!" }, { "TestMessage.Text", "Hello!" }
        };

        var dictionary = new NodeMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text>Hello!</Text>
                                                                      <OtherText>No changes</OtherText>
                                                                    </TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateStartsWithStrategy()
    {
        var message =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text>Hello World!</Text><OtherText>Good Bye!</OtherText></TestMessage>");

        var mappings = new Dictionary<string, string>
        {
            { "TestMessage.Text", "Hello!" }, { "TestMessage.Other", "Bye!" }
        };

        var dictionary = new NodeMappingDataDictionary
        {
            Mappings = mappings,
            PathMappingStrategy = PathMappingStrategy.STARTS_WITH
        };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text>Hello!</Text>
                                                                      <OtherText>Bye!</OtherText>
                                                                    </TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateEndsWithStrategy()
    {
        var message =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text>Hello World!</Text><OtherText>Good Bye!</OtherText></TestMessage>");

        var mappings = new Dictionary<string, string> { { "Text", "Hello!" } };

        var dictionary = new NodeMappingDataDictionary
        {
            Mappings = mappings,
            PathMappingStrategy = PathMappingStrategy.ENDS_WITH
        };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text>Hello!</Text>
                                                                      <OtherText>Hello!</OtherText>
                                                                    </TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateAttributes()
    {
        var message =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text name=\"helloText\">Hello World!</Text><OtherText name=\"goodbyeText\">No changes</OtherText></TestMessage>");

        var mappings = new Dictionary<string, string>
        {
            { "TestMessage.Text", "Hello!" }, { "TestMessage.Text.name", "newName" }
        };

        var dictionary = new NodeMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text name="newName">Hello!</Text>
                                                                      <OtherText name="goodbyeText">No changes</OtherText>
                                                                    </TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateMultipleAttributes()
    {
        var message =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text name=\"helloText\">Hello World!</Text><OtherText name=\"goodbyeText\">No changes</OtherText></TestMessage>");

        var mappings = new Dictionary<string, string> { { "name", "newName" } };

        var dictionary = new NodeMappingDataDictionary
        {
            Mappings = mappings,
            PathMappingStrategy = PathMappingStrategy.ENDS_WITH
        };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text name="newName">Hello World!</Text>
                                                                      <OtherText name="newName">No changes</OtherText>
                                                                    </TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateWithVariables()
    {
        var message =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text name=\"\">Hello World!</Text><OtherText>No changes</OtherText></TestMessage>");

        var mappings = new Dictionary<string, string>
        {
            { "TestMessage.Text", "${newText}" }, { "TestMessage.Text.name", "agenix:upperCase('text')" }
        };

        Context.SetVariable("newText", "Hello!");

        var dictionary = new NodeMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text name="TEXT">Hello!</Text>
                                                                      <OtherText>No changes</OtherText>
                                                                    </TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateWithNestedAndEmptyElements()
    {
        var message =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text><value></value></Text><OtherText></OtherText></TestMessage>");

        var mappings = new Dictionary<string, string> { { "TestMessage.Text.value", "Hello!" } };

        var dictionary = new NodeMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text>
                                                                        <value>Hello!</value>
                                                                      </Text>
                                                                      <OtherText />
                                                                    </TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateNoResult()
    {
        var message =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text>Hello World!</Text><OtherText>No changes</OtherText></TestMessage>");

        var mappings = new Dictionary<string, string> { { "Something.Else", "NotFound!" } };

        var dictionary = new NodeMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text>Hello World!</Text>
                                                                      <OtherText>No changes</OtherText>
                                                                    </TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateFromMappingFile()
    {
        var message =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text name=\"\">Hello World!</Text><OtherText>No changes</OtherText></TestMessage>");

        Context.SetVariable("newText", "Hello!");

        var dictionary = new NodeMappingDataDictionary
        {
            MappingFileResource = FileUtils.GetFileResource(
                "assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Variables.Dictionary/mapping.properties")
        };
        dictionary.Initialize();

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text name="newName">Hello!</Text>
                                                                      <OtherText>No changes</OtherText>
                                                                    </TestMessage>
                                                                    """));
    }
}
