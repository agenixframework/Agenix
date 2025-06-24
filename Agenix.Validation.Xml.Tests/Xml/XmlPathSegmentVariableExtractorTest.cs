#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
