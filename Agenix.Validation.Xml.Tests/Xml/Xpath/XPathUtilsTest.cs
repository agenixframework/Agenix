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
using System.Xml.XPath;
using Agenix.Api.Xml.Namespace;
using Agenix.Validation.Xml.Xpath;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Xml.Xpath;

public class XPathUtilsTest
{
    [Test]
    public void TestDynamicNamespaceCreation()
    {
        Dictionary<string, string> namespaces;

        // Test 1: Single namespace with duplicates
        namespaces = XpathUtils.GetDynamicNamespaces("//{http://agenix.org/foo}Foo/{http://agenix.org/foo}bar");

        Assert.That(namespaces.Count, Is.EqualTo(1));
        Assert.That(namespaces["dns1"], Is.EqualTo("http://agenix.org/foo"));

        // Test 2: Two different namespaces with one duplicate
        namespaces =
            XpathUtils.GetDynamicNamespaces(
                "//{http://agenix.org/foo}Foo/{http://agenix.org/bar}bar/{http://agenix.org/foo}value");

        Assert.That(namespaces.Count, Is.EqualTo(2));
        Assert.That(namespaces["dns1"], Is.EqualTo("http://agenix.org/foo"));
        Assert.That(namespaces["dns2"], Is.EqualTo("http://agenix.org/bar"));

        // Test 3: Three different namespaces including URN
        namespaces =
            XpathUtils.GetDynamicNamespaces(
                "//{http://agenix.org/foo}Foo/{http://agenix.org/bar}bar/{urn:agenix}value");

        Assert.That(namespaces.Count, Is.EqualTo(3));
        Assert.That(namespaces["dns1"], Is.EqualTo("http://agenix.org/foo"));
        Assert.That(namespaces["dns2"], Is.EqualTo("http://agenix.org/bar"));
        Assert.That(namespaces["dns3"], Is.EqualTo("urn:agenix"));
    }

    [Test]
    public void TestDynamicNamespaceReplacement()
    {
        var namespaces = new Dictionary<string, string>
        {
            { "ns1", "http://agenix.org/foo" },
            { "ns2", "http://agenix.org/bar" },
            { "ns3", "http://agenix.org/foo-bar" }
        };

        Assert.That(
            XpathUtils.ReplaceDynamicNamespaces("//{http://agenix.org/foo}Foo/{http://agenix.org/foo}bar", namespaces),
            Is.EqualTo("//ns1:Foo/ns1:bar"));

        Assert.That(
            XpathUtils.ReplaceDynamicNamespaces("//{http://agenix.org/foo}Foo/{http://agenix.org/bar}bar", namespaces),
            Is.EqualTo("//ns1:Foo/ns2:bar"));

        Assert.That(
            XpathUtils.ReplaceDynamicNamespaces("//{http://agenix.org/foo-bar}Foo/bar", namespaces),
            Is.EqualTo("//ns3:Foo/bar"));

        Assert.That(
            XpathUtils.ReplaceDynamicNamespaces("//{http://agenix.org/unkown}Foo/{http://agenix.org/unknown}bar",
                namespaces),
            Is.EqualTo("//{http://agenix.org/unkown}Foo/{http://agenix.org/unknown}bar"));
    }

    [Test]
    public void TestEvaluate()
    {
        // Arrange
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(@"<person status=""single"">
                                <name>foo</name>
                                <age>23</age>
                              </person>");

        var namespaceContext = new DefaultNamespaceContext();

        // Assert - String evaluation
        Assert.That(
            XpathUtils.Evaluate(xmlDocument, "/person/name", namespaceContext, XPathExpressionResult.String),
            Is.EqualTo("foo"));

        // Assert - Number evaluation
        Assert.That(
            XpathUtils.Evaluate(xmlDocument, "/person/age", namespaceContext, XPathExpressionResult.Number),
            Is.EqualTo(23.0));

        // Assert - Integer evaluation
        Assert.That(
            XpathUtils.Evaluate(xmlDocument, "/person/age", namespaceContext, XPathExpressionResult.Integer),
            Is.EqualTo(23));

        // Assert - Node evaluation
        Assert.That(
            XpathUtils.EvaluateAsNode(xmlDocument, "/person/name", namespaceContext).FirstChild.Value,
            Is.EqualTo("foo"));

        // Assert - NodeList evaluation
        Assert.That(
            XpathUtils.EvaluateAsNodeList(xmlDocument, "/person/name", namespaceContext)[0].FirstChild.Value,
            Is.EqualTo("foo"));

        // Assert - NodeList count for unknown elements
        Assert.That(
            XpathUtils.EvaluateAsNodeList(xmlDocument, "/person/unknown", namespaceContext).Count,
            Is.EqualTo(0));

        // Assert - Boolean evaluation (existing element)
        Assert.That(
            XpathUtils.EvaluateAsBoolean(xmlDocument, "/person/name", namespaceContext),
            Is.True);

        // Assert - Boolean evaluation (non-existing element)
        Assert.That(
            XpathUtils.EvaluateAsBoolean(xmlDocument, "/person/unknown", namespaceContext),
            Is.False);

        // Assert - String evaluation (direct method)
        Assert.That(
            XpathUtils.EvaluateAsString(xmlDocument, "/person/name", namespaceContext),
            Is.EqualTo("foo"));

        // Assert - Object evaluation with QName
        Assert.That(
            XpathUtils.EvaluateAsObject(xmlDocument, "/person/name", namespaceContext, XPathResultType.String),
            Is.EqualTo("foo"));

        // Assert - Number evaluation (direct method)
        Assert.That(
            XpathUtils.EvaluateAsNumber(xmlDocument, "/person/age", namespaceContext),
            Is.EqualTo(23.0));

        // Assert - Attribute evaluation
        Assert.That(
            XpathUtils.EvaluateAsString(xmlDocument, "/person/@status", namespaceContext),
            Is.EqualTo("single"));
    }
}
