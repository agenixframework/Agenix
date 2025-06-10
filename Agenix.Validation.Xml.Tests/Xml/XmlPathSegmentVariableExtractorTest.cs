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


using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Agenix.Api.Variable;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Xml;

public class XmlPathSegmentVariableExtractorTest : AbstractNUnitSetUp
{
    private static readonly string XmlFixture = "<person><name>Peter</name></person>";

    private readonly XpathSegmentVariableExtractor _extractor = new();

    [Test]
    public void TestExtractFromXml()
    {
        const string xpath = "//person/name";
        var matcher = MatchSegmentExpressionMatcher(xpath);

        Assert.That(_extractor.CanExtract(Context, XmlFixture, matcher), Is.True);
        Assert.That(_extractor.ExtractValue(Context, XmlFixture, matcher), Is.EqualTo("Peter"));

        // Assert that an XML document was cached
        var documentCacheKey = GenerateDocumentCacheKey(XmlFixture);
        var cachedXmlDocument = Context.GetVariableObject(documentCacheKey);
        Assert.That(cachedXmlDocument, Is.InstanceOf<XmlDocument>());

        // Assert that another match can be matched
        matcher = MatchSegmentExpressionMatcher(xpath);
        Assert.That(_extractor.CanExtract(Context, XmlFixture, matcher), Is.True);
        Assert.That(_extractor.ExtractValue(Context, XmlFixture, matcher), Is.EqualTo("Peter"));

        // Assert that an XML document can be matched
        matcher = MatchSegmentExpressionMatcher(xpath);
        Assert.That(_extractor.CanExtract(Context, cachedXmlDocument, matcher), Is.True);
        Assert.That(_extractor.ExtractValue(Context, cachedXmlDocument, matcher), Is.EqualTo("Peter"));
    }

    [Test]
    public void TestExtractFromInvalidXpathExpression()
    {
        const string invalidXpathPath = "name";
        var matcher = MatchSegmentExpressionMatcher(invalidXpathPath);

        Assert.That(_extractor.CanExtract(Context, XmlFixture, matcher), Is.False);
    }

    [Test]
    public void TestExtractFromXmlExpressionFailure()
    {
        const string invalidXpath = "//$$$";
        var matcher = MatchSegmentExpressionMatcher(invalidXpath);

        Assert.That(_extractor.CanExtract(Context, XmlFixture, matcher), Is.True);
        Assert.That(() => _extractor.ExtractValue(Context, XmlFixture, matcher), Throws.Exception);
    }



    private VariableExpressionSegmentMatcher MatchSegmentExpressionMatcher(string xpath)
    {
        var variableExpression = $"xpath({xpath})";
        var matcher = new VariableExpressionSegmentMatcher(variableExpression);
        Assert.That(matcher.NextMatch(), Is.True);
        return matcher;
    }

    private static string GenerateDocumentCacheKey(string xmlContent)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(xmlContent));
        return Convert.ToBase64String(hashBytes);
    }


}
