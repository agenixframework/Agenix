using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Validation.Xml.Functions;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Functions;

public class XmlFunctionsTest : AbstractNUnitSetUp
{
    [Test]
    public void TestCreateCDataSection()
    {
        Assert.That(XmlFunctions.CreateCDataSection("<Test><Message>Some Text</Message></Test>", Context),
            Is.EqualTo("<![CDATA[<Test><Message>Some Text</Message></Test>]]>"));
    }

    [Test]
    public void TestEscapeXml()
    {
        Assert.That(XmlFunctions.EscapeXml("<Test><Message>Some Text</Message></Test>", Context),
            Is.EqualTo("&lt;Test&gt;&lt;Message&gt;Some Text&lt;/Message&gt;&lt;/Test&gt;"));
    }

    [Test]
    public void TestXpath()
    {
        Assert.That(XmlFunctions.XPath("<Test><Message>Some Text</Message></Test>", "/Test/Message", Context),
            Is.EqualTo("Some Text"));
    }

    [Test]
    public void TestFunctionUtils()
    {
        Context.FunctionRegistry = (new DefaultFunctionRegistry());

        Assert.That(FunctionUtils.ResolveFunction("core:EscapeXml('<Message>Hello Yes, I like Agenix!</Message>')", Context),
            Is.EqualTo("&lt;Message&gt;Hello Yes, I like Agenix!&lt;/Message&gt;"));

        Assert.That(FunctionUtils.ResolveFunction("core:escapeXml('<Message>Hello Yes , I like Agenix!</Message>')", Context),
            Is.EqualTo("&lt;Message&gt;Hello Yes , I like Agenix!&lt;/Message&gt;"));

        Assert.That(FunctionUtils.ResolveFunction("core:escapeXml('<Message>Hello Yes,I like Agenix, and this is great!</Message>')", Context),
            Is.EqualTo("&lt;Message&gt;Hello Yes,I like Agenix, and this is great!&lt;/Message&gt;"));

        Assert.That(FunctionUtils.ResolveFunction("core:cdataSection('<Message>Hello Agenix!</Message>')", Context),
            Is.EqualTo("<![CDATA[<Message>Hello Agenix!</Message>]]>"));

        Assert.That(FunctionUtils.ResolveFunction("core:Xpath('<Message>Hello Agenix!</Message>', '/Message')", Context),
            Is.EqualTo("Hello Agenix!"));
    }
}

