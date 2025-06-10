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
