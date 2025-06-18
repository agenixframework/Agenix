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


using System.Collections;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Xpath;

/// <summary>
///     Provides utility methods for working with XPath expressions and evaluating them against XML nodes.
/// </summary>
public abstract class XpathUtils
{
    /// <summary>Dynamic namespace prefix suffix</summary>
    public const string DynamicNsStart = "{";

    public const string DynamicNsEnd = "}";

    /// <summary>Dynamic namespace prefix</summary>
    private const string DynamicNsPrefix = "dns";

    /// Logger
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(XpathUtils));

    /// <summary>
    ///     Creates a new XPath factory which is not thread safe per definition.
    /// </summary>
    /// <returns>A new XPathNavigator factory delegate</returns>
    private static readonly object XpathFactoryLock = new();

    /// <summary>
    ///     Extracts dynamic namespaces that are inline inside an XPath expression. Example:
    ///     <code>/{http://sample.org/foo}foo/{http://sample.org/bar}bar</code>
    /// </summary>
    /// <param name="expression">The XPath expression containing dynamic namespaces</param>
    /// <returns>Dictionary mapping namespace prefixes to namespace URIs</returns>
    public static Dictionary<string, string> GetDynamicNamespaces(string expression)
    {
        var namespaces = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(expression) || !HasDynamicNamespaces(expression))
        {
            return namespaces;
        }

        var tokens = expression.Split(DynamicNsStart, StringSplitOptions.RemoveEmptyEntries);

        for (var i = 1; i < tokens.Length; i++)
        {
            var endIndex = tokens[i].IndexOf(DynamicNsEnd, StringComparison.Ordinal);
            if (endIndex == -1)
            {
                continue; // Skip malformed namespace declarations
            }

            var namespaceUri = tokens[i][..endIndex];

            if (!namespaces.ContainsValue(namespaceUri))
            {
                namespaces[DynamicNsPrefix + i] = namespaceUri;
            }
        }

        return namespaces;
    }

    /// <summary>
    ///     Replaces all dynamic namespaces in an XPath expression with respective prefixes
    ///     in a namespace map.
    ///     XPath: <code>/{http://sample.org/foo}foo/{http://sample.org/bar}bar</code>
    ///     results in <code>/ns1:foo/ns2:bar</code> where the namespace map contains ns1 and ns2.
    /// </summary>
    /// <param name="expression">The XPath expression with dynamic namespaces</param>
    /// <param name="namespaces">Dictionary mapping namespace prefixes to URIs</param>
    /// <returns>The XPath expression with namespace prefixes</returns>
    public static string ReplaceDynamicNamespaces(string expression, Dictionary<string, string> namespaces)
    {
        if (string.IsNullOrEmpty(expression) || namespaces == null || namespaces.Count == 0)
        {
            return expression;
        }

        var expressionResult = expression;

        foreach (var namespaceEntry in namespaces)
        {
            var dynamicNamespace = DynamicNsStart + namespaceEntry.Value + DynamicNsEnd;

            if (expressionResult.Contains(dynamicNamespace))
            {
                // Escape special regex characters in the namespace URI
                var escapedNamespace = Regex.Escape(namespaceEntry.Value);
                var pattern = Regex.Escape(DynamicNsStart) + escapedNamespace + Regex.Escape(DynamicNsEnd);

                expressionResult = Regex.Replace(expressionResult, pattern, namespaceEntry.Key + ":");
            }
        }

        return expressionResult;
    }

    /// <summary>
    ///     Searches for dynamic namespaces in expression.
    /// </summary>
    /// <param name="expression">The XPath expression to check</param>
    /// <returns>True if the expression contains dynamic namespaces, false otherwise</returns>
    public static bool HasDynamicNamespaces(string expression)
    {
        return !string.IsNullOrEmpty(expression) &&
               expression.Contains(DynamicNsStart) &&
               expression.Contains(DynamicNsEnd);
    }

    private static Func<XPathNavigator> CreateXPathFactory()
    {
        lock (XpathFactoryLock)
        {
            Func<XPathNavigator> factory = null;
            const string defaultPropertyName = "javax.xml.xpath.XPathFactory";

            // Read system properties/environment variables and see if there is a factory set
            var environmentVariables = Environment.GetEnvironmentVariables();
            var appSettings = ConfigurationManager.AppSettings;

            // Check environment variables first
            foreach (DictionaryEntry envVar in environmentVariables)
            {
                var key = envVar.Key?.ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith(defaultPropertyName, StringComparison.Ordinal))
                {
                    var uri = ExtractUriFromKey(key);
                    if (!string.IsNullOrEmpty(uri))
                    {
                        factory = CreateFactoryWithUri(uri, key);
                        if (factory != null)
                        {
                            break;
                        }
                    }
                }
            }

            // Check app.config settings if no environment variable found
            if (factory == null && appSettings != null)
            {
                foreach (var key in appSettings.AllKeys)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith(defaultPropertyName, StringComparison.Ordinal))
                    {
                        var uri = ExtractUriFromKey(key);
                        if (string.IsNullOrEmpty(uri))
                        {
                            continue;
                        }

                        factory = CreateFactoryWithUri(uri, key);
                        if (factory != null)
                        {
                            break;
                        }
                    }
                }
            }

            // Create default factory if none found
            if (factory == null)
            {
                factory = CreateDefaultFactory();
            }

            return factory;
        }
    }

    private static string? ExtractUriFromKey(string key)
    {
        var colonIndex = key.IndexOf(':');
        return colonIndex > 0 ? key[(colonIndex + 1)..] : null;
    }

    /// <summary>
    ///     Construct an XPath expression instance with a given expression string and namespace context.
    ///     If the namespace context is not specified, a default context is built from the XML node
    ///     that is evaluated against.
    /// </summary>
    /// <param name="xPathExpression">The XPath expression string</param>
    /// <param name="nsContext">The namespace context (optional)</param>
    /// <returns>A compiled XPath expression</returns>
    /// <exception cref="XPathException">Thrown when XPath compilation fails</exception>
    private static XPathExpression BuildExpression(string xPathExpression, IXmlNamespaceResolver? nsContext = null)
    {
        try
        {
            // Compile the XPath expression
            var compiledExpression = nsContext != null
                ? XPathExpression.Compile(xPathExpression, nsContext)
                : XPathExpression.Compile(xPathExpression);

            return compiledExpression;
        }
        catch (Exception ex) when (ex is XPathException or ArgumentException)
        {
            throw new XPathException($"Failed to compile XPath expression: {xPathExpression}", ex);
        }
    }

    /// <summary>
    ///     Evaluate XPath expression with result type Node.
    /// </summary>
    /// <param name="node">The XML node to evaluate against</param>
    /// <param name="xPathExpression">The XPath expression</param>
    /// <param name="nsContext">The namespace context (optional)</param>
    /// <returns>The first matching XML node</returns>
    /// <exception cref="AgenixSystemException">Thrown when no node matches the expression</exception>
    public static XmlNode EvaluateAsNode(XmlNode node, string xPathExpression, IXmlNamespaceResolver? nsContext = null)
    {
        try
        {
            var navigator = node.CreateNavigator();

            // Use Evaluate to get the result directly
            var result = navigator.Evaluate(xPathExpression, nsContext);

            // Handle different result types
            switch (result)
            {
                case XPathNodeIterator nodeIterator:
                    if (!nodeIterator.MoveNext())
                    {
                        throw new AgenixSystemException($"No result for XPath expression: '{xPathExpression}'");
                    }

                    // Get the underlying node from the navigator
                    var resultNavigator = nodeIterator.Current;
                    if (resultNavigator?.UnderlyingObject is XmlNode xmlNode)
                    {
                        return xmlNode;
                    }

                    break;

                case XPathNavigator singleNavigator:
                    // Direct navigator result
                    if (singleNavigator.UnderlyingObject is XmlNode directXmlNode)
                    {
                        return directXmlNode;
                    }

                    break;
            }

            throw new AgenixSystemException($"XPath expression '{xPathExpression}' did not return a node");
        }
        catch (XPathException ex)
        {
            throw new AgenixSystemException($"Cannot evaluate XPath expression '{xPathExpression}' as node", ex);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            throw new AgenixSystemException($"Cannot evaluate XPath expression '{xPathExpression}' as node", ex);
        }
    }


    /// <summary>
    ///     Evaluate XPath expression with the result type NodeList.
    /// </summary>
    /// <param name="node">The XML node to evaluate against</param>
    /// <param name="xPathExpression">The XPath expression</param>
    /// <param name="nsContext">The namespace context (optional)</param>
    /// <returns>The collection of matching XML nodes</returns>
    /// <exception cref="AgenixSystemException">Thrown when no nodes match the expression</exception>
    public static XmlNodeList EvaluateAsNodeList(XmlNode node, string xPathExpression,
        IXmlNamespaceResolver? nsContext = null)
    {
        try
        {
            var expression = BuildExpression(xPathExpression, nsContext);
            var navigator = node.CreateNavigator();

            // Use Select to get a node iterator
            var nodeIterator = navigator.Select(expression);

            // Convert XPathNodeIterator to XmlNodeList
            var nodeList = new List<XmlNode>();
            while (nodeIterator.MoveNext())
            {
                if (nodeIterator.Current?.UnderlyingObject is XmlNode xmlNode)
                {
                    nodeList.Add(xmlNode);
                }
            }

            // Create a simple XmlNodeList implementation
            return new SimpleXmlNodeList(nodeList);
        }
        catch (XPathException ex)
        {
            throw new AgenixSystemException($"Cannot evaluate XPath expression '{xPathExpression}' as node list", ex);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            throw new AgenixSystemException($"Cannot evaluate XPath expression '{xPathExpression}' as node list", ex);
        }
    }


    /// <summary>
    ///     Evaluate XPath expression with result type String.
    /// </summary>
    /// <param name="node">The XML node to evaluate against</param>
    /// <param name="xPathExpression">The XPath expression</param>
    /// <param name="nsContext">The namespace context (optional)</param>
    /// <returns>The string result of the XPath evaluation</returns>
    public static string EvaluateAsString(XmlNode node, string xPathExpression, IXmlNamespaceResolver? nsContext = null)
    {
        var result = (string)EvaluateExpression(node, xPathExpression, nsContext, XPathResultType.String);

        if (string.IsNullOrWhiteSpace(result))
        {
            // Result is empty so check if the expression node really exists
            // If node does not exist an exception is thrown
            EvaluateAsNode(node, xPathExpression, nsContext);
        }

        return result ?? string.Empty;
    }

    /// <summary>
    ///     Evaluate XPath expression with the result type Boolean value.
    /// </summary>
    /// <param name="node">The XML node to evaluate against</param>
    /// <param name="xPathExpression">The XPath expression</param>
    /// <param name="nsContext">The namespace context (optional)</param>
    /// <returns>The boolean result of the XPath evaluation</returns>
    public static bool EvaluateAsBoolean(XmlNode node, string xPathExpression, IXmlNamespaceResolver? nsContext = null)
    {
        return (bool)EvaluateExpression(node, xPathExpression, nsContext, XPathResultType.Boolean);
    }

    /// <summary>
    ///     Evaluate XPath expression with result type Number.
    /// </summary>
    /// <param name="node">The XML node to evaluate against</param>
    /// <param name="xPathExpression">The XPath expression</param>
    /// <param name="nsContext">The namespace context (optional)</param>
    /// <returns>The numeric result of the XPath evaluation</returns>
    public static double EvaluateAsNumber(XmlNode node, string xPathExpression, IXmlNamespaceResolver? nsContext = null)
    {
        return (double)EvaluateExpression(node, xPathExpression, nsContext, XPathResultType.Number);
    }

    /// <summary>
    ///     Evaluate XPath expression with a specified result type.
    /// </summary>
    /// <param name="node">The XML node to evaluate against</param>
    /// <param name="xPathExpression">The XPath expression</param>
    /// <param name="nsContext">The namespace context (optional)</param>
    /// <param name="resultType">The expected result type</param>
    /// <returns>The result of the XPath evaluation</returns>
    public static object EvaluateAsObject(XmlNode node, string xPathExpression,
        IXmlNamespaceResolver? nsContext = null, XPathResultType resultType = XPathResultType.Any)
    {
        return EvaluateExpression(node, xPathExpression, nsContext, resultType);
    }


    /// <summary>
    ///     Method to find out whether an expression is of XPath nature or custom dot notation syntax.
    /// </summary>
    /// <param name="expression">The expression string to check</param>
    /// <returns>True if the expression is an XPath expression, false otherwise</returns>
    public static bool IsXPathExpression(string expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return false;
        }

        return expression.Contains('/') || expression.Contains('(');
    }

    /// <summary>
    ///     Evaluate XPath expression as String result type regardless
    ///     what actual result type the expression will evaluate to.
    /// </summary>
    /// <param name="node">The XML node to evaluate against</param>
    /// <param name="xPathExpression">The XPath expression</param>
    /// <param name="nsContext">The namespace context (optional)</param>
    /// <param name="resultType">The expected result type</param>
    /// <returns>The evaluation result</returns>
    public static object Evaluate(XmlNode node, string xPathExpression,
        IXmlNamespaceResolver nsContext, XPathExpressionResult resultType)
    {
        switch (resultType)
        {
            case XPathExpressionResult.Node:
            {
                var resultNode = EvaluateAsNode(node, xPathExpression, nsContext);
                return ExtractNodeValue(resultNode);
            }

            case XPathExpressionResult.NodeSet:
            {
                var resultNodeList = EvaluateAsNodeList(node, xPathExpression, nsContext);

                return (from XmlNode resultNode in resultNodeList select ExtractNodeValue(resultNode)).ToList();
            }

            case XPathExpressionResult.String:
                return EvaluateAsString(node, xPathExpression, nsContext);

            case XPathExpressionResult.Integer:
            {
                var result = EvaluateAsObject(node, xPathExpression, nsContext, resultType.GetAsXPathResultType());

                if (result == null)
                {
                    throw new AgenixSystemException($"No result for XPath expression: '{xPathExpression}'");
                }

                return (int)Math.Round(Convert.ToDouble(result));
            }

            case XPathExpressionResult.Boolean:
            case XPathExpressionResult.Number:
            default:
            {
                var result = EvaluateAsObject(node, xPathExpression, nsContext, resultType.GetAsXPathResultType());

                if (result == null)
                {
                    throw new AgenixSystemException($"No result for XPath expression: '{xPathExpression}'");
                }

                return result;
            }
        }
    }

    /// <summary>
    ///     Extract the string value from an XML node based on its type.
    /// </summary>
    /// <param name="node">The XML node</param>
    /// <returns>The string value of the node</returns>
    private static string ExtractNodeValue(XmlNode node)
    {
        if (node.NodeType != XmlNodeType.Element)
        {
            return node.Value ?? string.Empty;
        }

        if (node.FirstChild != null)
        {
            return node.FirstChild.Value ?? string.Empty;
        }

        return string.Empty;
    }


    /// <summary>
    ///     Evaluates the XPath expression.
    /// </summary>
    /// <param name="node">The XML node to evaluate against</param>
    /// <param name="xPathExpression">The XPath expression string</param>
    /// <param name="nsContext">The namespace context (optional)</param>
    /// <param name="returnType">The expected return type</param>
    /// <returns>The evaluation result</returns>
    /// <exception cref="AgenixSystemException">Thrown when XPath evaluation fails</exception>
    public static object EvaluateExpression(XmlNode node, string xPathExpression,
        IXmlNamespaceResolver? nsContext = null, XPathResultType returnType = XPathResultType.Any)
    {
        try
        {
            var expression = BuildExpression(xPathExpression, nsContext);
            var navigator = node.CreateNavigator();

            return returnType switch
            {
                XPathResultType.String => GetStringValue(navigator, expression),
                XPathResultType.Number => GetNumberValue(navigator, expression),
                XPathResultType.Boolean => GetBooleanValue(navigator, expression),
                XPathResultType.NodeSet => navigator.Select(expression),
                _ => navigator.Evaluate(expression)
            };
        }
        catch (XPathException ex)
        {
            throw new AgenixSystemException($"Cannot evaluate XPath expression '{xPathExpression}'", ex);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            throw new AgenixSystemException($"Cannot evaluate XPath expression '{xPathExpression}'", ex);
        }
    }

    private static double GetNumberValue(XPathNavigator navigator, XPathExpression expression)
    {
        var result = navigator.Evaluate(expression);

        return result switch
        {
            double number => number,
            XPathNodeIterator iterator when iterator.MoveNext() =>
                double.TryParse(iterator.Current?.Value, out var parsed) ? parsed : 0.0,
            XPathNodeIterator => 0.0,
            string str => double.TryParse(str, out var parsed) ? parsed : 0.0,
            bool boolean => boolean ? 1.0 : 0.0,
            _ when double.TryParse(result?.ToString(), out var parsed) => parsed,
            _ => 0.0
        };
    }

    private static bool GetBooleanValue(XPathNavigator navigator, XPathExpression expression)
    {
        var result = navigator.Evaluate(expression);

        return result switch
        {
            bool boolean => boolean,
            XPathNodeIterator iterator => iterator.MoveNext(), // true if has nodes, false if empty
            double number => number != 0.0,
            string str => !string.IsNullOrEmpty(str),
            _ => result != null
        };
    }


    private static string GetStringValue(XPathNavigator navigator, XPathExpression expression)
    {
        var result = navigator.Evaluate(expression);

        return result switch
        {
            XPathNodeIterator iterator when iterator.MoveNext() => iterator.Current?.Value ?? string.Empty,
            XPathNodeIterator => string.Empty, // Empty iterator
            string str => str,
            double number => number.ToString(),
            bool boolean => boolean.ToString(),
            _ => result?.ToString() ?? string.Empty
        };
    }


    private static Func<XPathNavigator> CreateFactoryWithUri(string? uri, string key)
    {
        try
        {
            // In .NET, we don't have the same factory pattern as Java
            // Instead, we create a factory delegate that can create XPathNavigator instances
            var factory = CreateFactoryForUri(uri);

            if (Log?.IsEnabled(LogLevel.Debug) == true)
            {
                Log.LogDebug("Created xpath factory using system property {Key} with value {Uri}", key, uri);
            }

            return factory;
        }
        catch (Exception ex)
        {
            Log?.LogWarning(ex, "Failed to instantiate xpath factory");
            return CreateDefaultFactory();
        }
    }

    private static Func<XPathNavigator> CreateFactoryForUri(string? uri)
    {
        // .NET doesn't have the same XPath factory URI system as Java
        // This is a simplified implementation - you might need to customize based on your needs
        return () =>
        {
            var document = new XmlDocument();
            return document.CreateNavigator();
        };
    }

    private static Func<XPathNavigator> CreateDefaultFactory()
    {
        var factory = new Func<XPathNavigator>(() =>
        {
            var document = new XmlDocument();
            return document.CreateNavigator();
        });

        if (Log?.IsEnabled(LogLevel.Debug) == true)
        {
            Log.LogDebug("Created default xpath factory");
        }

        return factory;
    }

    /// <summary>
    ///     Simple implementation of XmlNodeList for returning collections of nodes
    /// </summary>
    private class SimpleXmlNodeList(List<XmlNode> nodes) : XmlNodeList
    {
        private readonly List<XmlNode> _nodes = nodes ?? [];

        public override int Count => _nodes.Count;

        public override XmlNode? Item(int index)
        {
            return index >= 0 && index < _nodes.Count ? _nodes[index] : null;
        }

        public override IEnumerator GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }
    }
}
