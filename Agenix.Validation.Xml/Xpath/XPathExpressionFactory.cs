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

using System.Xml;
using System.Xml.XPath;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Xpath;

/// <summary>
///     Factory for creating compiled XPath expressions.
/// </summary>
public abstract class XPathExpressionFactory
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(XPathExpressionFactory));

    /// <summary>
    ///     Create a compiled XPath expression using the given string.
    /// </summary>
    /// <param name="expression">the XPath expression</param>
    /// <returns>the compiled XPath expression</returns>
    /// <exception cref="InvalidOperationException">if XPath compilation fails</exception>
    /// <exception cref="XPathException">if the given expression cannot be parsed</exception>
    public static XPathExpression CreateXPathExpression(string expression)
    {
        return CreateXPathExpression(expression, new Dictionary<string, string>());
    }

    /// <summary>
    ///     Create a compiled XPath expression using the given string and namespaces.
    ///     The namespace map should consist of string prefixes mapped to string namespaces.
    /// </summary>
    /// <param name="expression">the XPath expression</param>
    /// <param name="namespaces">a map that binds string prefixes to string namespaces</param>
    /// <returns>the compiled XPath expression</returns>
    /// <exception cref="InvalidOperationException">if XPath compilation fails</exception>
    /// <exception cref="XPathException">if the given expression cannot be parsed</exception>
    public static XPathExpression CreateXPathExpression(string expression, IDictionary<string, string>? namespaces)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("expression is empty", nameof(expression));
        }

        namespaces ??= new Dictionary<string, string>();

        Log.LogTrace("Creating XPathExpression");
        return DotNetXPathExpressionFactory.CreateXPathExpression(expression, namespaces);
    }
}

/// <summary>
///     .NET implementation of XPath expression factory using System.Xml.XPath.
/// </summary>
internal static class DotNetXPathExpressionFactory
{
    /// <summary>
    ///     Creates an XPath expression using .NET's built-in XPath support.
    /// </summary>
    /// <param name="expression">the XPath expression string</param>
    /// <param name="namespaces">namespace prefix to URI mappings</param>
    /// <returns>compiled XPath expression</returns>
    public static XPathExpression CreateXPathExpression(string expression, IDictionary<string, string> namespaces)
    {
        try
        {
            // Create a temporary document for compilation
            var doc = new XmlDocument();
            var navigator = doc.CreateNavigator();

            // Compile the expression - navigator.Compile() only takes the expression string
            var compiledExpression = navigator.Compile(expression);

            // Set namespace context if namespaces are provided
            if (namespaces.Count > 0)
            {
                var namespaceManager = new XmlNamespaceManager(doc.NameTable);
                foreach (var ns in namespaces)
                {
                    namespaceManager.AddNamespace(ns.Key, ns.Value);
                }

                // Set the namespace context on the compiled expression
                compiledExpression.SetContext(namespaceManager);
            }

            return compiledExpression;
        }
        catch (XPathException ex)
        {
            throw new XPathException($"Failed to parse XPath expression '{expression}'", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create XPath expression '{expression}'", ex);
        }
    }
}
