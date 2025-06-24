
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Validation.Json.Variable.Dictionary.Json;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Variable.Dictionary.Json;

public class JsonPathMappingDataDictionaryTest : AbstractNUnitSetUp
{
    [Test]
    public void TestTranslateExactMatchStrategy()
    {
        var message = new DefaultMessage("{\"TestMessage\":{\"Text\":\"Hello World!\",\"OtherText\":\"No changes\", \"OtherNumber\": 10}}");

        var mappings = new Dictionary<string, string>
        {
            { "$.Something.Else", "NotFound" },
            { "$.TestMessage.Text", "Hello!" }
        };

        var dictionary = new JsonPathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);
        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Text\":\"Hello!\",\"OtherText\":\"No changes\",\"OtherNumber\":10}}"));
    }

    [Test]
    public void TestTranslateMultipleNodes()
    {
        var message = new DefaultMessage("[" +
                    "{\"TestMessage\":{\"Text\":\"Hello World!\",\"OtherText\":\"No changes\", \"OtherNumber\": 10}}, " +
                    "{\"TestMessage\":{\"Text\":\"Hello World!\",\"OtherText\":\"No changes\", \"OtherNumber\": 10}}" +
                "]");

        var mappings = new Dictionary<string, string>
        {
            { "$.Something.Else", "NotFound" },
            { "$..Text", "Hello!" }
        };

        var dictionary = new JsonPathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);
        Assert.That(message.GetPayload<string>(), Is.EqualTo("[" +
                    "{\"TestMessage\":{\"Text\":\"Hello!\",\"OtherText\":\"No changes\",\"OtherNumber\":10}}," +
                    "{\"TestMessage\":{\"Text\":\"Hello!\",\"OtherText\":\"No changes\",\"OtherNumber\":10}}" +
                "]"));
    }

    [Test]
    public void TestTranslateWithVariables()
    {
        var message = new DefaultMessage("{\"TestMessage\":{\"Text\":\"Hello World!\",\"OtherText\":\"No changes\"}}");

        var mappings = new Dictionary<string, string>
        {
            { "$.TestMessage.Text", "${helloText}" }
        };

        var dictionary = new JsonPathMappingDataDictionary { Mappings = mappings };

        Context.SetVariable("helloText", "Hello!");

        dictionary.ProcessMessage(message, Context);
        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Text\":\"Hello!\",\"OtherText\":\"No changes\"}}"));
    }

    [Test]
    public void TestTranslateWithArrays()
    {
        var message = new DefaultMessage("{\"TestMessage\":{\"Text\":[\"Hello World!\",\"Hello Galaxy!\"],\"OtherText\":\"No changes\"}}");

        var mappings = new Dictionary<string, string>
        {
            { "$.TestMessage.Text[0]", "Hello!" },
            { "$.TestMessage.Text[1]", "Hello Universe!" }
        };

        var dictionary = new JsonPathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);
        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Text\":[\"Hello!\",\"Hello Universe!\"],\"OtherText\":\"No changes\"}}"));
    }

    [Test]
    public void TestTranslateWithArraysAndObjects()
    {
        var message = new DefaultMessage("{\"TestMessage\":{\"Greetings\":[{\"Text\":\"Hello World!\"},{\"Text\":\"Hello Galaxy!\"}],\"OtherText\":\"No changes\"}}");

        var mappings = new Dictionary<string, string>
        {
            { "$.TestMessage.Greetings[0].Text", "Hello!" },
            { "$.TestMessage.Greetings[1].Text", "Hello Universe!" }
        };

        var dictionary = new JsonPathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);
        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Greetings\":[{\"Text\":\"Hello!\"},{\"Text\":\"Hello Universe!\"}],\"OtherText\":\"No changes\"}}"));
    }

    [Test]
    public void TestTranslateFromMappingFile()
    {
        var message = new DefaultMessage("{\"TestMessage\":{\"Text\":\"Hello World!\",\"OtherText\":\"No changes\"}}");

        var dictionary = new JsonPathMappingDataDictionary
        {
            MappingFileResource = FileUtils.GetFileResource(
            "assembly://Agenix.Validation.Json.Tests/Agenix.Validation.Json.Tests.Resources.Variable.Dictionary/jsonmapping.properties")
        };
        dictionary.Initialize();

        dictionary.ProcessMessage(message, Context);
        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Text\":\"Hello!\",\"OtherText\":\"No changes\"}}"));
    }

    [Test]
    public void TestTranslateWithNullValues()
    {
        var message = new DefaultMessage("{\"TestMessage\":{\"Text\":null,\"OtherText\":null}}");

        var mappings = new Dictionary<string, string>
        {
            { "$.TestMessage.Text", "Hello!" }
        };

        var dictionary = new JsonPathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);
        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Text\":\"Hello!\",\"OtherText\":null}}"));
    }

    [Test]
    public void TestTranslateWithNumberValues()
    {
        var message = new DefaultMessage("{\"TestMessage\":{\"Number\":0,\"OtherNumber\":100}}");

        var mappings = new Dictionary<string, string>
        {
            { "$.TestMessage.Number", "99" }
        };

        var dictionary = new JsonPathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);
        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Number\":99,\"OtherNumber\":100}}"));
    }

    [Test]
    public void TestTranslateNoResult()
    {
        var message = new DefaultMessage("{\"TestMessage\":{\"Text\":\"Hello World!\",\"OtherText\":\"No changes\"}}");

        var mappings = new Dictionary<string, string>
        {
            { "$.Something.Else", "NotFound" }
        };

        var dictionary = new JsonPathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);
        Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"TestMessage\":{\"Text\":\"Hello World!\",\"OtherText\":\"No changes\"}}"));
    }
}
