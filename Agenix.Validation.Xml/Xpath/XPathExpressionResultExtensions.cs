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

namespace Agenix.Validation.Xml.Xpath;

/// <summary>
///     Enumeration representing the possible result types for XPath expression evaluation. In Agenix
///     XPath expressions a prefix may determine the result type like this:
///     string://MyExpressionString/Value
///     number://MyExpressionString/Value
///     boolean://MyExpressionString/Value
///     The result type prefix is supposed to be stripped off before expression evaluation
///     and determines the evaluation result.
/// </summary>
public enum XPathExpressionResult
{
    Node,
    NodeSet,
    String,
    Boolean,
    Number,
    Integer
}

/// <summary>
///     Extension methods for XPathExpressionResult enum.
/// </summary>
public static class XPathExpressionResultExtensions
{
    /// <summary>
    ///     Prefix for XPath expressions in Agenix determining the result type
    /// </summary>
    private const string StringPrefix = "string:";

    private const string NumberPrefix = "number:";
    private const string IntegerPrefix = "integer:";
    private const string NodePrefix = "node:";
    private const string NodeSetPrefix = "node-set:";
    private const string BooleanPrefix = "boolean:";

    /// <summary>
    ///     Get the enumeration value from an expression string. According to the leading
    ///     prefix and a default result type the enumeration value is returned.
    /// </summary>
    /// <param name="value">The expression string to parse</param>
    /// <param name="defaultResult">The default result type if no prefix is found</param>
    /// <returns>The corresponding XPathExpressionResult</returns>
    public static XPathExpressionResult FromString(string value, XPathExpressionResult defaultResult)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultResult;
        }

        if (value.StartsWith(StringPrefix, StringComparison.Ordinal))
        {
            return XPathExpressionResult.String;
        }

        if (value.StartsWith(NodePrefix, StringComparison.Ordinal))
        {
            return XPathExpressionResult.Node;
        }

        if (value.StartsWith(NodeSetPrefix, StringComparison.Ordinal))
        {
            return XPathExpressionResult.NodeSet;
        }

        if (value.StartsWith(BooleanPrefix, StringComparison.Ordinal))
        {
            return XPathExpressionResult.Boolean;
        }

        if (value.StartsWith(NumberPrefix, StringComparison.Ordinal))
        {
            return XPathExpressionResult.Number;
        }

        return value.StartsWith(IntegerPrefix, StringComparison.Ordinal)
            ? XPathExpressionResult.Integer
            : defaultResult;
    }

    /// <summary>
    ///     Get a constant XPathResultType from this enumeration value.
    /// </summary>
    /// <param name="result">The XPathExpressionResult enum value</param>
    /// <returns>The corresponding XPathResultType</returns>
    public static XPathResultType GetAsXPathResultType(this XPathExpressionResult result)
    {
        return result switch
        {
            XPathExpressionResult.String => XPathResultType.String,
            XPathExpressionResult.Node => XPathResultType.Navigator,
            XPathExpressionResult.NodeSet => XPathResultType.NodeSet,
            XPathExpressionResult.Boolean => XPathResultType.Boolean,
            XPathExpressionResult.Number or XPathExpressionResult.Integer => XPathResultType.Number,
            _ => XPathResultType.Navigator
        };
    }

    /// <summary>
    ///     Get a constant XmlQualifiedName instance from this enumeration value.
    /// </summary>
    /// <param name="result">The XPathExpressionResult enum value</param>
    /// <returns>The corresponding XmlQualifiedName</returns>
    public static XmlQualifiedName GetAsQName(this XPathExpressionResult result)
    {
        return result switch
        {
            XPathExpressionResult.String => new XmlQualifiedName("string", "http://www.w3.org/1999/XSL/Transform"),
            XPathExpressionResult.Node => new XmlQualifiedName("node", "http://www.w3.org/1999/XSL/Transform"),
            XPathExpressionResult.NodeSet => new XmlQualifiedName("node-set", "http://www.w3.org/1999/XSL/Transform"),
            XPathExpressionResult.Boolean => new XmlQualifiedName("boolean", "http://www.w3.org/1999/XSL/Transform"),
            XPathExpressionResult.Number or XPathExpressionResult.Integer => new XmlQualifiedName("number",
                "http://www.w3.org/1999/XSL/Transform"),
            _ => new XmlQualifiedName("node", "http://www.w3.org/1999/XSL/Transform")
        };
    }

    /// <summary>
    ///     Cut off the leading result type prefix in a XPath expression string.
    /// </summary>
    /// <param name="expression">The expression string</param>
    /// <returns>The expression string without the prefix</returns>
    public static string CutOffPrefix(string expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return expression;
        }

        if (expression.StartsWith(StringPrefix, StringComparison.Ordinal))
        {
            return expression[StringPrefix.Length..];
        }

        if (expression.StartsWith(NodePrefix, StringComparison.Ordinal))
        {
            return expression[NodePrefix.Length..];
        }

        if (expression.StartsWith(NodeSetPrefix, StringComparison.Ordinal))
        {
            return expression[NodeSetPrefix.Length..];
        }

        if (expression.StartsWith(BooleanPrefix, StringComparison.Ordinal))
        {
            return expression[BooleanPrefix.Length..];
        }

        if (expression.StartsWith(NumberPrefix, StringComparison.Ordinal))
        {
            return expression[NumberPrefix.Length..];
        }

        return expression.StartsWith(IntegerPrefix, StringComparison.Ordinal)
            ? expression[IntegerPrefix.Length..]
            : expression;
    }
}
