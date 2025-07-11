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
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Util;
using Agenix.Api.Variable;
using Agenix.Core.Message;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Xpath;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml;

/// <summary>
///     Represents an implementation of <see cref="SegmentVariableExtractorRegistry.AbstractSegmentVariableExtractor" />
///     specifically designed to handle XPath-based variable extraction from XML data.
/// </summary>
public class XpathSegmentVariableExtractor : SegmentVariableExtractorRegistry.AbstractSegmentVariableExtractor
{
    /// Logger
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(XpathSegmentVariableExtractor));

    /// <summary>
    ///     Determines if this extractor can handle the given object and matcher.
    /// </summary>
    /// <param name="testContext">The test context</param>
    /// <param name="obj">The object to extract from</param>
    /// <param name="matcher">The variable expression segment matcher</param>
    /// <returns>True if it can extract, false otherwise</returns>
    public override bool CanExtract(TestContext testContext, object? obj, VariableExpressionSegmentMatcher matcher)
    {
        return obj == null ||
               ((obj is XmlDocument ||
                 (obj is string s && IsXmlPredicate.Instance.Test(s))) &&
                XpathUtils.IsXPathExpression(matcher.SegmentExpression));
    }

    /// <summary>
    ///     Extracts the value using XPath evaluation.
    /// </summary>
    /// <param name="testContext">The test context</param>
    /// <param name="obj">The object to extract from</param>
    /// <param name="matcher">The variable expression segment matcher</param>
    /// <returns>The extracted value</returns>
    protected override object DoExtractValue(TestContext testContext, object obj,
        VariableExpressionSegmentMatcher matcher)
    {
        return obj == null ? null : ExtractXpath(testContext, obj, matcher);
    }

    /// <summary>
    ///     Extracts value using XPath expression evaluation.
    /// </summary>
    /// <param name="testContext">The test context</param>
    /// <param name="xml">The XML object (Document or string)</param>
    /// <param name="matcher">The variable expression segment matcher</param>
    /// <returns>The extracted value</returns>
    private static object ExtractXpath(TestContext testContext, object xml, VariableExpressionSegmentMatcher matcher)
    {
        XmlDocument document = null;

        switch (xml)
        {
            case XmlDocument xmlDoc:
                document = xmlDoc;
                break;
            case string xmlString:
                {
                    var documentCacheKey = GenerateDocumentCacheKey(xmlString);

                    // Try to get a cached document
                    if (testContext.GetVariables().TryGetValue(documentCacheKey, out var cachedDoc) &&
                        cachedDoc is XmlDocument cachedDocument)
                    {
                        document = cachedDocument;
                    }
                    else
                    {
                        // Parse and cache the document
                        document = XmlUtils.ParseMessagePayload(xmlString);
                        testContext.SetVariable(documentCacheKey, document);
                    }

                    break;
                }
        }

        if (document == null)
        {
            throw new AgenixSystemException($"Unable to extract xpath from object of type {xml.GetType().Name}");
        }

        var message = new DefaultMessage { Payload = xml };
        var namespaceContext = XmlValidationHelper
            .GetNamespaceContextBuilder(testContext)
            .BuildContext(message, new Dictionary<string, string>());

        return XpathUtils.Evaluate(document, matcher.SegmentExpression, namespaceContext, XPathExpressionResult.String);
    }

    /// <summary>
    ///     Generates a cache key for the XML document based on its content.
    /// </summary>
    /// <param name="xmlContent">The XML content</param>
    /// <returns>A unique cache key</returns>
    private static string GenerateDocumentCacheKey(string xmlContent)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(xmlContent));
        return Convert.ToBase64String(hashBytes);
    }
}
