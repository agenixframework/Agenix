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


using System.Text;
using System.Xml;

namespace Agenix.Validation.Xml.Util;

/// <summary>
///     Convenience methods for working with the DOM API,
///     in particular for working with DOM Nodes and DOM Elements.
/// </summary>
public static class DomUtils
{
    /// <summary>
    ///     Retrieves all child elements of the given DOM element that match any of the given element names.
    ///     Only looks at the direct child level of the given element; do not go into further depth
    ///     (in contrast to the DOM API's GetElementsByTagName method).
    /// </summary>
    /// <param name="element">The DOM element to analyze</param>
    /// <param name="childElementNames">The child element names to look for</param>
    /// <returns>A List of child XmlElement instances</returns>
    public static List<XmlElement> GetChildElementsByTagName(XmlElement element, params string[] childElementNames)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element), "Element must not be null");
        }

        if (childElementNames == null)
        {
            throw new ArgumentNullException(nameof(childElementNames), "Element names collection must not be null");
        }

        var childElementNameList = new List<string>(childElementNames);
        var nodeList = element.ChildNodes;
        var childElements = new List<XmlElement>();

        for (var i = 0; i < nodeList.Count; i++)
        {
            var node = nodeList[i];
            if (node is XmlElement xmlElement && NodeNameMatch(node, childElementNameList))
            {
                childElements.Add(xmlElement);
            }
        }

        return childElements;
    }

    /// <summary>
    ///     Retrieves all child elements of the given DOM element that match the given element name.
    ///     Only look at the direct child level of the given element; do not go into further depth
    ///     (in contrast to the DOM API's GetElementsByTagName method).
    /// </summary>
    /// <param name="element">The DOM element to analyze</param>
    /// <param name="childElementName">The child element name to look for</param>
    /// <returns>A List of child XmlElement instances</returns>
    public static List<XmlElement> GetChildElementsByTagName(XmlElement element, string childElementName)
    {
        return GetChildElementsByTagName(element, [childElementName]);
    }

    /// <summary>
    ///     Utility method that returns the first child element identified by its name.
    /// </summary>
    /// <param name="element">The DOM element to analyze</param>
    /// <param name="childElementName">The child element name to look for</param>
    /// <returns>The XmlElement instance, or null if none found</returns>
    public static XmlElement? GetChildElementByTagName(XmlElement element, string childElementName)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element), "Element must not be null");
        }

        if (childElementName == null)
        {
            throw new ArgumentNullException(nameof(childElementName), "Element name must not be null");
        }

        var nodeList = element.ChildNodes;
        for (var i = 0; i < nodeList.Count; i++)
        {
            var node = nodeList[i];
            if (node is XmlElement xmlElement && NodeNameMatch(node, childElementName))
            {
                return xmlElement;
            }
        }

        return null;
    }

    /// <summary>
    ///     Utility method that returns the first child element value identified by its name.
    /// </summary>
    /// <param name="element">The DOM element to analyze</param>
    /// <param name="childElementName">The child element name to look for</param>
    /// <returns>The extracted text value, or null if no child element found</returns>
    public static string? GetChildElementValueByTagName(XmlElement element, string childElementName)
    {
        var child = GetChildElementByTagName(element, childElementName);
        return child != null ? GetTextValue(child) : null;
    }

    /// <summary>
    ///     Retrieves all child elements of the given DOM element.
    /// </summary>
    /// <param name="element">The DOM element to analyze</param>
    /// <returns>A List of child XmlElement instances</returns>
    public static List<XmlElement> GetChildElements(XmlElement element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element), "Element must not be null");
        }

        var nodeList = element.ChildNodes;
        var childElements = new List<XmlElement>();

        for (var i = 0; i < nodeList.Count; i++)
        {
            var node = nodeList[i];
            if (node is XmlElement xmlElement)
            {
                childElements.Add(xmlElement);
            }
        }

        return childElements;
    }

    /// <summary>
    ///     Extracts the text value from the given DOM element, ignoring XML comments.
    ///     Appends all CharacterData nodes and EntityReference nodes into a single
    ///     String value, excluding Comment nodes. Only exposes actual user-specified
    ///     text, no default values of any kind.
    /// </summary>
    /// <param name="valueElement">The element to extract text from</param>
    /// <returns>The concatenated text content</returns>
    public static string GetTextValue(XmlElement valueElement)
    {
        if (valueElement == null)
        {
            throw new ArgumentNullException(nameof(valueElement), "Element must not be null");
        }

        var sb = new StringBuilder();
        var nodeList = valueElement.ChildNodes;

        for (var i = 0; i < nodeList.Count; i++)
        {
            var item = nodeList[i];
            if ((item is XmlCharacterData && item is not XmlComment) || item is XmlEntityReference)
            {
                sb.Append(item.Value);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Namespace-aware equals comparison. Returns true if either
    ///     LocalName or Name equals desiredName, otherwise returns false.
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <param name="desiredName">The desired name to match</param>
    /// <returns>True if names match, false otherwise</returns>
    public static bool NodeNameEquals(XmlNode node, string desiredName)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node), "Node must not be null");
        }

        if (desiredName == null)
        {
            throw new ArgumentNullException(nameof(desiredName), "Desired name must not be null");
        }

        return NodeNameMatch(node, desiredName);
    }

    /// <summary>
    ///     Matches the given node's name and local name against the given desired name.
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <param name="desiredName">The desired name to match</param>
    /// <returns>True if names match, false otherwise</returns>
    private static bool NodeNameMatch(XmlNode node, string desiredName)
    {
        return desiredName.Equals(node.Name) || desiredName.Equals(node.LocalName);
    }

    /// <summary>
    ///     Matches the given node's name and local name against the given desired names.
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <param name="desiredNames">The collection of desired names to match</param>
    /// <returns>True if any name matches, false otherwise</returns>
    private static bool NodeNameMatch(XmlNode node, ICollection<string> desiredNames)
    {
        return desiredNames.Contains(node.Name) || desiredNames.Contains(node.LocalName);
    }
}
