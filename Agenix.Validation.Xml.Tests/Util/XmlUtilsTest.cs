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


using System.Xml;
using Agenix.Api.Xml.Namespace;
using Agenix.Validation.Xml.Util;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Util;

public class XmlUtilsTest
{
    [Test]
    public void TestFindNodeByName()
    {
        var doc = XmlUtils.ParseMessagePayload(
            "<testRequest><message id=\"1\">Hello</message></testRequest>");

        var result = XmlUtils.FindNodeByName(doc, "testRequest");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.LocalName, Is.EqualTo("testRequest"));
        Assert.That(result.NodeType, Is.EqualTo(XmlNodeType.Element));

        result = XmlUtils.FindNodeByName(doc, "testRequest.message");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.LocalName, Is.EqualTo("message"));
        Assert.That(result.NodeType, Is.EqualTo(XmlNodeType.Element));

        result = XmlUtils.FindNodeByName(doc, "testRequest.message.id");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.LocalName, Is.EqualTo("id"));
        Assert.That(result.NodeType, Is.EqualTo(XmlNodeType.Attribute));

        result = XmlUtils.FindNodeByName(doc, "testRequest.wrongElement");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TestStripWhitespaceNodes_SimpleCase()
    {
        // Create XML with whitespace text nodes
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root>   <child>Text content</child>   </root>");

        var rootElement = xmlDoc.DocumentElement;

        // Count initial nodes (should include whitespace text nodes)
        var initialCount = rootElement.ChildNodes.Count;
        Assert.That(initialCount, Is.EqualTo(1), "Should have whitespace nodes initially");

        // Call the method under test
        XmlUtils.StripWhitespaceNodes(rootElement);

        // After stripping, should only have the child element
        Assert.That(rootElement.ChildNodes.Count, Is.EqualTo(1),
            "Should have only one child after stripping whitespace");
        Assert.That(rootElement.FirstChild.NodeType, Is.EqualTo(XmlNodeType.Element));
        Assert.That(rootElement.FirstChild.LocalName, Is.EqualTo("child"));
    }

    [Test]
    public void TestStripWhitespaceNodes_ComplexStructure()
    {
        // Create a more complex XML structure with multiple levels of whitespace
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(@"
            <root>
                <level1>
                    <level2>Content</level2>

                </level1>

                <level1>

                    <level2>More content</level2>
                </level1>
            </root>");

        var rootElement = xmlDoc.DocumentElement;

        // Call the method under test
        XmlUtils.StripWhitespaceNodes(rootElement);

        // Verify structure is preserved but whitespace is removed
        Assert.That(rootElement.ChildNodes.Count, Is.EqualTo(2), "Should have 2 level1 elements");

        foreach (XmlNode level1Node in rootElement.ChildNodes)
        {
            Assert.That(level1Node.NodeType, Is.EqualTo(XmlNodeType.Element));
            Assert.That(level1Node.LocalName, Is.EqualTo("level1"));

            // Each level1 should have exactly one level2 child (whitespace removed)
            Assert.That(level1Node.ChildNodes.Count, Is.EqualTo(1),
                "Each level1 should have only one child after whitespace removal");
            Assert.That(level1Node.FirstChild.LocalName, Is.EqualTo("level2"));
        }
    }

    [Test]
    public void TestStripWhitespaceNodes_MixedContent()
    {
        // Test with mixed content (elements and meaningful text)
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root>  <child>Text</child>Important text<other>More</other>  </root>");

        var rootElement = xmlDoc.DocumentElement;

        // Call the method under test
        XmlUtils.StripWhitespaceNodes(rootElement);

        // Should remove leading/trailing whitespace but keep meaningful text
        var meaningfulNodes = 0;
        foreach (XmlNode node in rootElement.ChildNodes)
        {
            if (node.NodeType == XmlNodeType.Element ||
                (node.NodeType == XmlNodeType.Text && !string.IsNullOrWhiteSpace(node.Value)))
            {
                meaningfulNodes++;
            }
        }

        Assert.That(meaningfulNodes, Is.EqualTo(3), "Should have 2 elements + 1 meaningful text node");
    }

    [Test]
    public void TestStripWhitespaceNodes_OnlyWhitespace()
    {
        // Test element that contains only whitespace
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root>   \n   \t   </root>");

        var rootElement = xmlDoc.DocumentElement;
        var initialCount = rootElement.ChildNodes.Count;

        // Call the method under test
        XmlUtils.StripWhitespaceNodes(rootElement);

        // All whitespace should be removed
        Assert.That(rootElement.ChildNodes.Count, Is.EqualTo(0), "All whitespace nodes should be removed");
    }

    [Test]
    public void TestStripWhitespaceNodes_NoWhitespace()
    {
        // Test with no whitespace nodes
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root><child>Text</child><other>More text</other></root>");

        var rootElement = xmlDoc.DocumentElement;
        var initialCount = rootElement.ChildNodes.Count;

        // Call the method under test
        XmlUtils.StripWhitespaceNodes(rootElement);

        // Structure should remain unchanged
        Assert.That(rootElement.ChildNodes.Count, Is.EqualTo(initialCount),
            "No nodes should be removed when no whitespace present");
        Assert.That(rootElement.ChildNodes.Count, Is.EqualTo(2));
    }

    [Test]
    public void TestStripWhitespaceNodes_EmptyElement()
    {
        // Test with empty element
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root></root>");

        var rootElement = xmlDoc.DocumentElement;

        // Call the method under test
        XmlUtils.StripWhitespaceNodes(rootElement);

        // Should remain empty
        Assert.That(rootElement.ChildNodes.Count, Is.EqualTo(0), "Empty element should remain empty");
    }

    [Test]
    public void TestStripWhitespaceNodes_SingleTextNode()
    {
        // Test element with single meaningful text node
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root>Important content</root>");

        var rootElement = xmlDoc.DocumentElement;

        // Call the method under test
        XmlUtils.StripWhitespaceNodes(rootElement);

        // Meaningful text should be preserved
        Assert.That(rootElement.ChildNodes.Count, Is.EqualTo(1));
        Assert.That(rootElement.FirstChild.NodeType, Is.EqualTo(XmlNodeType.Text));
        Assert.That(rootElement.FirstChild.Value, Is.EqualTo("Important content"));
    }

    [Test]
    public void TestStripWhitespaceNodes_NestedWhitespace()
    {
        // Test deeply nested structure with whitespace at various levels
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(@"
            <root>
                <level1>
                    <level2>
                        <level3>Deep content</level3>
                    </level2>
                </level1>
            </root>");

        var rootElement = xmlDoc.DocumentElement;

        // Call the method under test
        XmlUtils.StripWhitespaceNodes(rootElement);

        // Navigate through the structure to verify whitespace was removed at all levels
        Assert.That(rootElement.ChildNodes.Count, Is.EqualTo(1), "Root should have one child");

        var level1 = rootElement.FirstChild;
        Assert.That(level1.LocalName, Is.EqualTo("level1"));
        Assert.That(level1.ChildNodes.Count, Is.EqualTo(1), "Level1 should have one child");

        var level2 = level1.FirstChild;
        Assert.That(level2.LocalName, Is.EqualTo("level2"));
        Assert.That(level2.ChildNodes.Count, Is.EqualTo(1), "Level2 should have one child");

        var level3 = level2.FirstChild;
        Assert.That(level3.LocalName, Is.EqualTo("level3"));
        Assert.That(level3.InnerText, Is.EqualTo("Deep content"));
    }

    [Test]
    public void TestGetNodesPathName_RootElement()
    {
        // Test with root element
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root><child>content</child></root>");

        var rootElement = xmlDoc.DocumentElement;
        var pathName = XmlUtils.GetNodesPathName(rootElement);

        Assert.That(pathName, Is.EqualTo("root"), "Root element should return just its name");
    }

    [Test]
    public void TestGetNodesPathName_NestedElement()
    {
        // Test with nested elements
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root><level1><level2><level3>content</level3></level2></level1></root>");

        var level3Element = xmlDoc.SelectSingleNode("//level3");
        var pathName = XmlUtils.GetNodesPathName(level3Element);

        Assert.That(pathName, Is.EqualTo("root.level1.level2.level3"),
            "Nested element should return full dot-separated path");
    }

    [Test]
    public void TestGetNodesPathName_SimpleAttribute()
    {
        // Test with attribute on root element
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root id=\"123\"><child>content</child></root>");

        var idAttribute = xmlDoc.DocumentElement.Attributes["id"];
        var pathName = XmlUtils.GetNodesPathName(idAttribute);

        Assert.That(pathName, Is.EqualTo("root.id"), "Attribute should return element path plus attribute name");
    }

    [Test]
    public void TestGetNodesPathName_NestedElementAttribute()
    {
        // Test with attribute on nested element
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root><level1><level2 attr=\"value\">content</level2></level1></root>");

        var level2Element = xmlDoc.SelectSingleNode("//level2") as XmlElement;
        var attribute = level2Element.Attributes["attr"];
        var pathName = XmlUtils.GetNodesPathName(attribute);

        Assert.That(pathName, Is.EqualTo("root.level1.level2.attr"),
            "Nested attribute should return full path including attribute");
    }

    [Test]
    public void TestGetNodesPathName_MultipleAttributes()
    {
        // Test with multiple attributes on same element
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root><element id=\"1\" name=\"test\" value=\"data\">content</element></root>");

        var element = xmlDoc.SelectSingleNode("//element") as XmlElement;

        var idPathName = XmlUtils.GetNodesPathName(element.Attributes["id"]);
        var namePathName = XmlUtils.GetNodesPathName(element.Attributes["name"]);
        var valuePathName = XmlUtils.GetNodesPathName(element.Attributes["value"]);

        Assert.That(idPathName, Is.EqualTo("root.element.id"));
        Assert.That(namePathName, Is.EqualTo("root.element.name"));
        Assert.That(valuePathName, Is.EqualTo("root.element.value"));
    }

    [Test]
    public void TestGetNodesPathName_TextNode()
    {
        // Test with text node
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root><child>text content</child></root>");

        var childElement = xmlDoc.SelectSingleNode("//child");
        var textNode = childElement.FirstChild; // This is the text node
        var pathName = XmlUtils.GetNodesPathName(textNode);

        Assert.That(pathName, Is.EqualTo("root.child.#text"), "Text node should return path with #text");
    }

    [Test]
    public void TestGetNodesPathName_ElementWithNamespace()
    {
        // Test with namespaced elements
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<ns:root xmlns:ns=\"http://example.com\"><ns:child>content</ns:child></ns:root>");

        var rootElement = xmlDoc.DocumentElement;
        var childElement = xmlDoc.SelectSingleNode("//ns:child", CreateNamespaceManager(xmlDoc));

        var rootPathName = XmlUtils.GetNodesPathName(rootElement);
        var childPathName = XmlUtils.GetNodesPathName(childElement);

        // LocalName should be used, not the prefixed name
        Assert.That(rootPathName, Is.EqualTo("root"));
        Assert.That(childPathName, Is.EqualTo("root.child"));
    }

    [Test]
    public void TestGetNodesPathName_AttributeWithNamespace()
    {
        // Test with namespaced attribute
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<root xmlns:ns=\"http://example.com\" ns:attr=\"value\">content</root>");

        var rootElement = xmlDoc.DocumentElement;
        var namespacedAttr = rootElement.Attributes["attr", "http://example.com"];
        var pathName = XmlUtils.GetNodesPathName(namespacedAttr);

        Assert.That(pathName, Is.EqualTo("root.attr"), "Namespaced attribute should use LocalName");
    }

    [Test]
    public void TestGetNodesPathName_DeepNesting()
    {
        // Test with very deep nesting
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(@"
        <root>
            <level1>
                <level2>
                    <level3>
                        <level4>
                            <level5 deepAttr=""value"">
                                <level6>deep content</level6>
                            </level5>
                        </level4>
                    </level3>
                </level2>
            </level1>
        </root>");

        var level6Element = xmlDoc.SelectSingleNode("//level6");
        var level5Element = xmlDoc.SelectSingleNode("//level5") as XmlElement;
        var deepAttribute = level5Element.Attributes["deepAttr"];

        var level6PathName = XmlUtils.GetNodesPathName(level6Element);
        var deepAttrPathName = XmlUtils.GetNodesPathName(deepAttribute);

        Assert.That(level6PathName, Is.EqualTo("root.level1.level2.level3.level4.level5.level6"));
        Assert.That(deepAttrPathName, Is.EqualTo("root.level1.level2.level3.level4.level5.deepAttr"));
    }

    [Test]
    public void TestGetNodesPathName_SingleElementWithAttribute()
    {
        // Test simple case with single element and attribute
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<element attr=\"value\">content</element>");

        var element = xmlDoc.DocumentElement;
        var attribute = element.Attributes["attr"];

        var elementPathName = XmlUtils.GetNodesPathName(element);
        var attributePathName = XmlUtils.GetNodesPathName(attribute);

        Assert.That(elementPathName, Is.EqualTo("element"));
        Assert.That(attributePathName, Is.EqualTo("element.attr"));
    }

    [Test]
    public void TestGetNodesPathName_SiblingsWithSameName()
    {
        // Test elements with same name but different paths
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(@"
        <root>
            <container>
                <item id=""1"">First</item>
            </container>
            <container>
                <item id=""2"">Second</item>
            </container>
        </root>");

        var items = xmlDoc.SelectNodes("//item");
        var firstItem = items[0] as XmlElement;
        var secondItem = items[1] as XmlElement;

        var firstItemPathName = XmlUtils.GetNodesPathName(firstItem);
        var secondItemPathName = XmlUtils.GetNodesPathName(secondItem);
        var firstIdPathName = XmlUtils.GetNodesPathName(firstItem.Attributes["id"]);
        var secondIdPathName = XmlUtils.GetNodesPathName(secondItem.Attributes["id"]);

        // Both should have the same path name since they have the same structure
        Assert.That(firstItemPathName, Is.EqualTo("root.container.item"));
        Assert.That(secondItemPathName, Is.EqualTo("root.container.item"));
        Assert.That(firstIdPathName, Is.EqualTo("root.container.item.id"));
        Assert.That(secondIdPathName, Is.EqualTo("root.container.item.id"));
    }

    [Test]
    public void TestGetNodesPathName_AttributeWithoutOwnerElement()
    {
        // Test edge case - this shouldn't normally happen but let's test defensive coding
        var xmlDoc = new XmlDocument();
        var attribute = xmlDoc.CreateAttribute("orphanAttr");
        attribute.Value = "value";

        // This attribute has no owner element
        var pathName = XmlUtils.GetNodesPathName(attribute);

        Assert.That(pathName, Is.EqualTo(".orphanAttr"), "Orphaned attribute should just return dot + attribute name");
    }

    [Test]
    public void TestPrettyPrint_NullInput()
    {
        // Test with null input
        var result = XmlUtils.PrettyPrint(null);

        Assert.That(result, Is.Null, "Null input should return null");
    }

    [Test]
    public void TestPrettyPrint_EmptyInput()
    {
        // Test with empty string
        var result = XmlUtils.PrettyPrint("");

        Assert.That(result, Is.EqualTo(""), "Empty string should return empty string");
    }

    [Test]
    public void TestPrettyPrint_WhitespaceOnlyInput()
    {
        // Test with whitespace-only input
        var result = XmlUtils.PrettyPrint("   ");

        Assert.That(result, Is.EqualTo("   "), "Whitespace-only input should return original string");
    }

    [Test]
    public void TestPrettyPrint_SimpleXml()
    {
        // Test with simple, unformatted XML
        var input = "<root><child>value</child></root>";
        var result = XmlUtils.PrettyPrint(input);

        // Should be formatted with proper indentation
        Assert.That(result, Does.Contain("<?xml version=\"1.0\" encoding=\"utf-8\"?>"),
            "Should include XML declaration");
        Assert.That(result, Does.Contain("<root>"), "Should contain root element");
        Assert.That(result, Does.Contain("<child>value</child>"), "Should contain child element");
        Assert.That(result, Does.Contain("</root>"), "Should contain closing root tag");

        // Check that it's actually formatted (contains newlines and indentation)
        Assert.That(result, Does.Contain("\n"), "Should contain newlines for formatting");
    }

    [Test]
    public void TestPrettyPrint_AlreadyFormattedXml()
    {
        // Test with already well-formatted XML
        var input = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <child>value</child>
</root>";

        var result = XmlUtils.PrettyPrint(input);

        // Should still be valid and formatted
        Assert.That(result, Does.Contain("<?xml"), "Should contain XML declaration");
        Assert.That(result, Does.Contain("<root>"), "Should contain root element");
        Assert.That(result, Does.Contain("<child>value</child>"), "Should contain child element");
    }

    [Test]
    public void TestPrettyPrint_ComplexXml()
    {
        // Test with complex XML structure
        var input =
            "<root><level1><level2 attr=\"value\"><level3>content</level3><level3>more content</level3></level2></level1><sibling>data</sibling></root>";
        var result = XmlUtils.PrettyPrint(input);

        // Should be properly formatted
        Assert.That(result, Does.Contain("<?xml"), "Should include XML declaration");
        Assert.That(result, Does.Contain("level1"), "Should contain level1 element");
        Assert.That(result, Does.Contain("level2"), "Should contain level2 element");
        Assert.That(result, Does.Contain("level3"), "Should contain level3 elements");
        Assert.That(result, Does.Contain("attr=\"value\""), "Should preserve attributes");
        Assert.That(result, Does.Contain("sibling"), "Should contain sibling element");

        // Should be formatted with proper structure
        Assert.That(result, Does.Contain("\n"), "Should contain newlines");
    }

    [Test]
    public void TestPrettyPrint_XmlWithAttributes()
    {
        // Test XML with various attributes
        var input = "<root id=\"1\" name=\"test\"><child attr1=\"value1\" attr2=\"value2\">content</child></root>";
        var result = XmlUtils.PrettyPrint(input);

        // Should preserve all attributes
        Assert.That(result, Does.Contain("id=\"1\""), "Should preserve id attribute");
        Assert.That(result, Does.Contain("name=\"test\""), "Should preserve name attribute");
        Assert.That(result, Does.Contain("attr1=\"value1\""), "Should preserve attr1");
        Assert.That(result, Does.Contain("attr2=\"value2\""), "Should preserve attr2");
        Assert.That(result, Does.Contain("content"), "Should preserve element content");
    }

    [Test]
    public void TestPrettyPrint_XmlWithNamespaces()
    {
        // Test XML with namespaces
        var input = "<ns:root xmlns:ns=\"http://example.com\"><ns:child>value</ns:child></ns:root>";
        var result = XmlUtils.PrettyPrint(input);

        // Should preserve namespace declarations and prefixes
        Assert.That(result, Does.Contain("xmlns:ns=\"http://example.com\""), "Should preserve namespace declaration");
        Assert.That(result, Does.Contain("ns:root"), "Should preserve namespace prefix on root");
        Assert.That(result, Does.Contain("ns:child"), "Should preserve namespace prefix on child");
    }

    [Test]
    public void TestPrettyPrint_XmlWithCData()
    {
        // Test XML with CDATA sections
        var input = "<root><![CDATA[Some <special> content & more]]></root>";
        var result = XmlUtils.PrettyPrint(input);

        // Should preserve CDATA content
        Assert.That(result, Does.Contain("<![CDATA[Some <special> content & more]]>"), "Should preserve CDATA section");
    }

    [Test]
    public void TestPrettyPrint_XmlWithSpecialCharacters()
    {
        // Test XML with special characters and entities
        var input = "<root><child>Content with &lt; &gt; &amp; &quot; &apos;</child></root>";
        var result = XmlUtils.PrettyPrint(input);

        // Should preserve escaped entities
        Assert.That(result, Does.Contain("&lt;"), "Should preserve less-than entity");
        Assert.That(result, Does.Contain("&gt;"), "Should preserve greater-than entity");
        Assert.That(result, Does.Contain("&amp;"), "Should preserve ampersand entity");
    }

    [Test]
    public void TestPrettyPrint_InvalidXml_ReturnsOriginal()
    {
        // Test with malformed XML - should return original string
        var input = "<root><unclosed>content</root>";
        var result = XmlUtils.PrettyPrint(input);

        Assert.That(result, Is.EqualTo(input), "Invalid XML should return original string unchanged");
    }

    [Test]
    public void TestPrettyPrint_NotXmlAtAll_ReturnsOriginal()
    {
        // Test with completely non-XML content
        var input = "This is just plain text, not XML at all!";
        var result = XmlUtils.PrettyPrint(input);

        Assert.That(result, Is.EqualTo(input), "Non-XML content should return original string unchanged");
    }

    [Test]
    public void TestPrettyPrint_PartiallyInvalidXml_ReturnsOriginal()
    {
        // Test with XML that starts correctly but becomes invalid
        var input = "<root><child>valid start but then invalid content < > without proper escaping</child>";
        var result = XmlUtils.PrettyPrint(input);

        Assert.That(result, Is.EqualTo(input), "Partially invalid XML should return original string unchanged");
    }

    [Test]
    public void TestPrettyPrint_XmlWithLeadingWhitespace()
    {
        // Test XML with leading/trailing whitespace (should be trimmed)
        var input = "   <root><child>content</child></root>   ";
        var result = XmlUtils.PrettyPrint(input);

        // Should be processed successfully (trimmed before parsing)
        Assert.That(result, Does.Contain("<?xml"), "Should include XML declaration");
        Assert.That(result, Does.Contain("<root>"), "Should contain root element");
        Assert.That(result, Does.Contain("<child>content</child>"), "Should contain child content");
    }

    [Test]
    public void TestPrettyPrint_XmlDeclarationOnly()
    {
        // Test with just XML declaration
        var input = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
        var result = XmlUtils.PrettyPrint(input);

        // Should return original since there's no root element to format
        Assert.That(result, Is.EqualTo(input), "XML declaration only should return original");
    }

    [Test]
    public void TestPrettyPrint_SingleSelfClosingElement()
    {
        // Test with single self-closing element
        var input = "<root/>";
        var result = XmlUtils.PrettyPrint(input);

        // Should be formatted properly
        Assert.That(result, Does.Contain("<?xml"), "Should include XML declaration");
        Assert.That(result, Does.Contain("<root"), "Should contain root element");
    }

    [Test]
    public void TestPrettyPrint_XmlWithComments()
    {
        // Test XML with comments
        var input = "<root><!-- This is a comment --><child>content</child><!-- Another comment --></root>";
        var result = XmlUtils.PrettyPrint(input);

        // Should preserve comments
        Assert.That(result, Does.Contain("<!-- This is a comment -->"), "Should preserve first comment");
        Assert.That(result, Does.Contain("<!-- Another comment -->"), "Should preserve second comment");
        Assert.That(result, Does.Contain("<child>content</child>"), "Should preserve element content");
    }


    // Helper method for namespace testing
    private XmlNamespaceManager CreateNamespaceManager(XmlDocument doc)
    {
        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("ns", "http://example.com");
        return nsmgr;
    }


    [Test]
    public void TestLookupNamespacesInXmlFragment()
    {
        var namespaces = NamespaceContextBuilder.LookupNamespaces(
            "<ns1:testRequest xmlns:ns1=\"http://agenix.org/test\" xmlns:ns2=\"http://agenix.org/test2\"></ns1:testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/test2"));

        namespaces = NamespaceContextBuilder.LookupNamespaces(
            "<ns1:testRequest xmlns:ns1=\"http://agenix.org/xmlns/test\" xmlns:ns2=\"http://agenix.org/xmlns/test2\"></ns1:testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/xmlns/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/xmlns/test2"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload(
                "<ns1:testRequest xmlns:ns1=\"http://agenix.org/test\" xmlns:ns2=\"http://agenix.org/test2\"></ns1:testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/test2"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload(
                "<ns1:testRequest xmlns:ns1=\"http://agenix.org/xmlns/test\" xmlns:ns2=\"http://agenix.org/xmlns/test2\"></ns1:testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/xmlns/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/xmlns/test2"));
    }

    [Test]
    public void TestLookupNamespacesInXmlFragmentSingleQuotes()
    {
        var namespaces = NamespaceContextBuilder.LookupNamespaces(
            "<ns1:testRequest xmlns:ns1='http://agenix.org/test' xmlns:ns2='http://agenix.org/test2'></ns1:testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/test2"));

        namespaces = NamespaceContextBuilder.LookupNamespaces(
            "<ns1:testRequest xmlns:ns1=\"http://agenix.org/test\" xmlns:ns2='http://agenix.org/test2'></ns1:testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/test2"));

        namespaces = NamespaceContextBuilder.LookupNamespaces(
            "<ns1:testRequest xmlns:ns1='http://agenix.org/xmlns/test' xmlns:ns2='http://agenix.org/xmlns/test2'></ns1:testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/xmlns/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/xmlns/test2"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload(
                "<ns1:testRequest xmlns:ns1='http://agenix.org/test' xmlns:ns2='http://agenix.org/test2'></ns1:testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/test2"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload(
                "<ns1:testRequest xmlns:ns1=\"http://agenix.org/test\" xmlns:ns2='http://agenix.org/test2'></ns1:testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/test2"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload(
                "<ns1:testRequest xmlns:ns1='http://agenix.org/xmlns/test' xmlns:ns2='http://agenix.org/xmlns/test2'></ns1:testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/xmlns/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/xmlns/test2"));
    }

    [Test]
    public void TestLookupNamespacesInXmlFragmentWithAttributes()
    {
        Dictionary<string, string> namespaces;

        namespaces = NamespaceContextBuilder.LookupNamespaces(
            "<ns1:testRequest xmlns:ns1=\"http://agenix.org/test\" id=\"123456789\" xmlns:ns2=\"http://agenix.org/test2\"></ns1:testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/test2"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload(
                "<ns1:testRequest xmlns:ns1=\"http://agenix.org/test\" id=\"123456789\" xmlns:ns2=\"http://agenix.org/test2\"></ns1:testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));
        Assert.That(namespaces["ns2"], Is.EqualTo("http://agenix.org/test2"));
    }

    [Test]
    public void TestLookupNamespacesInXmlFragmentDefaultNamespaces()
    {
        Dictionary<string, string> namespaces;

        namespaces =
            NamespaceContextBuilder.LookupNamespaces(
                "<testRequest xmlns=\"http://agenix.org/test-default\"></testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(1));
        Assert.That(namespaces[string.Empty], Is.EqualTo("http://agenix.org/test-default"));

        namespaces =
            NamespaceContextBuilder.LookupNamespaces(
                "<testRequest xmlns='http://agenix.org/test-default'></testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(1));
        Assert.That(namespaces[string.Empty], Is.EqualTo("http://agenix.org/test-default"));

        namespaces = NamespaceContextBuilder.LookupNamespaces(
            "<testRequest xmlns=\"http://agenix.org/test-default\" xmlns:ns1=\"http://agenix.org/test\"></testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces[string.Empty], Is.EqualTo("http://agenix.org/test-default"));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));

        namespaces = NamespaceContextBuilder.LookupNamespaces(
            "<testRequest xmlns=\"http://agenix.org/xmlns/test-default\" xmlns:ns1=\"http://agenix.org/xmlns/test\"></testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces[string.Empty], Is.EqualTo("http://agenix.org/xmlns/test-default"));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/xmlns/test"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload("<testRequest xmlns=\"http://agenix.org/test-default\"></testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(1));
        Assert.That(namespaces[string.Empty], Is.EqualTo("http://agenix.org/test-default"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload("<testRequest xmlns='http://agenix.org/test-default'></testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(1));
        Assert.That(namespaces[string.Empty], Is.EqualTo("http://agenix.org/test-default"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload(
                "<testRequest xmlns=\"http://agenix.org/test-default\" xmlns:ns1=\"http://agenix.org/test\"></testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces[string.Empty], Is.EqualTo("http://agenix.org/test-default"));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/test"));

        namespaces = XmlUtils.LookupNamespaces(
            XmlUtils.ParseMessagePayload(
                "<testRequest xmlns=\"http://agenix.org/xmlns/test-default\" xmlns:ns1=\"http://agenix.org/xmlns/test\"></testRequest>"));

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces[string.Empty], Is.EqualTo("http://agenix.org/xmlns/test-default"));
        Assert.That(namespaces["ns1"], Is.EqualTo("http://agenix.org/xmlns/test"));
    }

    [Test]
    public void TestLookupNamespacesInXmlFragmentNoNamespacesFound()
    {
        var namespaces = NamespaceContextBuilder.LookupNamespaces("<testRequest id=\"123456789\"></testRequest>");

        Assert.That(namespaces.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestParseEncodingCharset()
    {
        var doc = XmlUtils.ParseMessagePayload("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                               "<testRequest xmlns=\"http://agenix.org/test-default\"></testRequest>");
        Assert.That(doc, Is.Not.Null);

        doc = XmlUtils.ParseMessagePayload("<?xml version='1.0' encoding='UTF-8'?>" +
                                           "<testRequest xmlns='http://agenix.org/test-default'></testRequest>");
        Assert.That(doc, Is.Not.Null);

        doc = XmlUtils.ParseMessagePayload("<?xml version='1.0' encoding = 'ISO-8859-1' standalone=\"yes\"?>" +
                                           "<testRequest xmlns='http://agenix.org/test-default'></testRequest>");
        Assert.That(doc, Is.Not.Null);

        doc = XmlUtils.ParseMessagePayload("<?xml version='1.0'?>" +
                                           "<testRequest xmlns='http://agenix.org/test-default'></testRequest>");
        Assert.That(doc, Is.Not.Null);

        doc = XmlUtils.ParseMessagePayload("<?xml version='1.0'?>" +
                                           "<testRequest xmlns='http://agenix.org/test-default'>encoding</testRequest>");
        Assert.That(doc, Is.Not.Null);

        doc = XmlUtils.ParseMessagePayload("<?xml version='1.0' encoding='UTF-8'?>" +
                                           "<testRequest xmlns='http://agenix.org/test-default'><![CDATA[<?xml version='1.0' encoding='some unknown encoding'?><message>Nested</message>]]></testRequest>");
        Assert.That(doc, Is.Not.Null);

        doc = XmlUtils.ParseMessagePayload("<?xml version='1.0'?>" +
                                           "<testRequest xmlns='http://agenix.org/test-default'><![CDATA[<?xml version='1.0' encoding='some unknown encoding'?><message>Nested</message>]]></testRequest>");
        Assert.That(doc, Is.Not.Null);
    }

    [Test]
    public void TestEncodingRoundTrip()
    {
        var payload = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                      "<testRequest xmlns=\"http://agenix.org/test-default\">ÄäÖöÜü</testRequest>";

        var doc = XmlUtils.ParseMessagePayload(payload);

        Assert.That(XmlUtils.Serialize(doc), Is.EqualTo(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
            "<testRequest xmlns=\"http://agenix.org/test-default\">ÄäÖöÜü</testRequest>"));
    }

    [Test]
    public void TestOmitXmlDeclaration()
    {
        var payload = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                      " <testRequest xmlns=\"http://agenix.org/test-default\">Test</testRequest>";

        Assert.That(XmlUtils.OmitXmlDeclaration(payload),
            Is.EqualTo("<testRequest xmlns=\"http://agenix.org/test-default\">Test</testRequest>"));

        Assert.That(
            XmlUtils.OmitXmlDeclaration(
                "<testRequest xmlns=\"http://agenix.org/test-default\">Test</testRequest>"),
            Is.EqualTo("<testRequest xmlns=\"http://agenix.org/test-default\">Test</testRequest>"));

        Assert.That(XmlUtils.OmitXmlDeclaration(""), Is.EqualTo(""));

        Assert.That(XmlUtils.OmitXmlDeclaration("Test"), Is.EqualTo("Test"));
    }
}
