using Agenix.Api.Message;
using Agenix.Api.Xml.Namespace;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Validation.Xml.Variable.Dictionary.Xml;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Dictionary;

public class XpathMappingDataDictionaryTest : AbstractNUnitSetUp
{

    private readonly string htmlPayload =
        "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">" +
        "<html>" +
        "<head>" +
        "<title>?</title>" +
        "</head>" +
        "<body>" +
        "<h1>?</h1>" +
        "<hr />" +
        "<form action=\"/\">" +
        "<input name=\"foo\" type=\"text\" />" +
        "</form>" +
        "</body>" +
        "</html>";


    private readonly string payload =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestMessage><Text>Hello World!</Text><OtherText name=\"foo\">No changes</OtherText></TestMessage>";


    [Test]
    public void TestTranslate()
    {
        var message = new DefaultMessage(payload);

        var mappings = new Dictionary<string, string>
        {
            { "//TestMessage/Text", "Hello!" }, { "//@name", "bar" }, { "//something/else", "not_found" }
        };

        var dictionary = new XpathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                            <?xml version="1.0" encoding="utf-8"?>
                                                                            <TestMessage>
                                                                              <Text>Hello!</Text>
                                                                              <OtherText name="bar">No changes</OtherText>
                                                                            </TestMessage>
                                                                            """));
    }

    [Test]
    public void TestTranslateMultipleNodes()
    {
        var message = new DefaultMessage(payload);

        var mappings = new Dictionary<string, string>
        {
            { "//*[string-length(normalize-space(text())) > 0]", "Hello!" },
            { "//@*", "bar" }
        };

        var dictionary = new XpathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <TestMessage>
                                                                      <Text>Hello!</Text>
                                                                      <OtherText name="bar">Hello!</OtherText>
                                                                    </TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateWithNamespaceLookup()
    {
        var namespacePayload = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ns1:TestMessage xmlns:ns1=\"http://www.foo.bar\"><ns1:Text>Hello World!</ns1:Text><ns1:OtherText name=\"foo\">No changes</ns1:OtherText></ns1:TestMessage>";
        var message = new DefaultMessage(namespacePayload);

        var mappings = new Dictionary<string, string>
        {
            { "//ns1:TestMessage/ns1:Text", "Hello!" },
            { "//@name", "bar" }
        };

        var dictionary = new XpathMappingDataDictionary { Mappings = mappings };

        dictionary.ProcessMessage(message, Context);

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo("""
                                                                    <?xml version="1.0" encoding="utf-8"?>
                                                                    <ns1:TestMessage xmlns:ns1="http://www.foo.bar">
                                                                      <ns1:Text>Hello!</ns1:Text>
                                                                      <ns1:OtherText name="bar">No changes</ns1:OtherText>
                                                                    </ns1:TestMessage>
                                                                    """));
    }

    [Test]
    public void TestTranslateWithNamespaceBuilder()
    {
        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?><ns1:TestMessage xmlns:ns1=\"http://www.foo.bar\"><ns1:Text>Hello World!</ns1:Text><ns1:OtherText name=\"foo\">No changes</ns1:OtherText></ns1:TestMessage>");

        var mappings = new Dictionary<string, string>
        {
            { "//foo:TestMessage/foo:Text", "Hello!" },
            { "//@name", "bar" }
        };

        var dictionary = new XpathMappingDataDictionary { Mappings = mappings };

        var namespaceContextBuilder = new NamespaceContextBuilder();
        var namespaces = new Dictionary<string, string>
        {
            { "foo", "http://www.foo.bar" }
        };
        namespaceContextBuilder.NamespaceMappings = namespaces;
        dictionary.NamespaceContextBuilder = namespaceContextBuilder;

        dictionary.ProcessMessage(message, Context);

        const string expected = """
                                <?xml version="1.0" encoding="utf-8"?>
                                <ns1:TestMessage xmlns:ns1="http://www.foo.bar">
                                  <ns1:Text>Hello!</ns1:Text>
                                  <ns1:OtherText name="bar">No changes</ns1:OtherText>
                                </ns1:TestMessage>
                                """;

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo(expected));
    }

    [Test]
    public void TestTranslateWithVariables()
    {
        var message = new DefaultMessage(payload);

        var mappings = new Dictionary<string, string>
        {
            { "//TestMessage/Text", "${hello}" },
            { "//@name", "bar" }
        };

        Context.SetVariable("hello", "Hello!");

        var dictionary = new XpathMappingDataDictionary { Mappings = mappings };


        dictionary.ProcessMessage(message, Context);

        var expected = """
                       <?xml version="1.0" encoding="utf-8"?>
                       <TestMessage>
                         <Text>Hello!</Text>
                         <OtherText name="bar">No changes</OtherText>
                       </TestMessage>
                       """;

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo(expected));
    }

    [Test]
    public void TestTranslateFromMappingFile()
    {
        var message = new DefaultMessage(payload);

        var dictionary = new XpathMappingDataDictionary { MappingFileResource =
            FileUtils.GetFileResource("assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Variables.Dictionary/xpathmapping.properties") };
        dictionary.Initialize();

        dictionary.ProcessMessage(message, Context);

        const string expected = """
                                <?xml version="1.0" encoding="utf-8"?>
                                <TestMessage>
                                  <Text>Hello!</Text>
                                  <OtherText name="bar">GoodBye!</OtherText>
                                </TestMessage>
                                """;

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo(expected));
    }

    [Test]
    public void TestTranslateNoResult()
    {
        var message = new DefaultMessage(payload);

        var mappings = new Dictionary<string, string>
        {
            { "//TestMessage/Unknown", "Hello!" },
            { "//@name", "bar" }
        };

        var dictionary = new XpathMappingDataDictionary { Mappings = (mappings) };

        dictionary.ProcessMessage(message, Context);

        const string expected = """
                                <?xml version="1.0" encoding="utf-8"?>
                                <TestMessage>
                                  <Text>Hello World!</Text>
                                  <OtherText name="bar">No changes</OtherText>
                                </TestMessage>
                                """;

        Assert.That(message.GetPayload<string>().Trim(), Is.EqualTo(expected));
    }

    [Test]
    public void TestTranslateXhtml()
    {
        var message = new DefaultMessage(htmlPayload);
        message.SetType(nameof(MessageType.XHTML));

        var mappings = new Dictionary<string, string>
        {
            { "/xh:html/xh:head/xh:title", "Hello" },
            { "//xh:h1", "Hello Agenix!" }
        };

        var dictionary = new XpathMappingDataDictionary { Mappings = mappings };

        var namespaceContextBuilder = new NamespaceContextBuilder();
        namespaceContextBuilder.NamespaceMappings.Add("xh", "http://www.w3.org/1999/xhtml");
        dictionary.NamespaceContextBuilder = namespaceContextBuilder;

        dictionary.ProcessMessage(message, Context);

        var actual = message.GetPayload<string>().Trim();
        Assert.That(actual, Does.Contain("<title>Hello</title>"));
        Assert.That(actual, Does.Contain("<h1>Hello Agenix!</h1>"));
        Assert.That(actual, Does.Contain("<hr />"));
    }
}
