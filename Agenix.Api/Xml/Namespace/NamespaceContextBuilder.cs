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

using System.Text.RegularExpressions;
using System.Xml;
using Agenix.Api.Message;

namespace Agenix.Api.Xml.Namespace;

/// <summary>
///     Builds a namespace context for XPath expression evaluations. Builder supports default mappings
///     as well as dynamic mappings from a received message.
///     Namespace mappings are defined as key value pairs where key is defined as a namespace prefix and value is the
///     actual namespace uri.
/// </summary>
public class NamespaceContextBuilder
{
    /// <summary>The default bean id in DI container</summary>
    public const string DefaultBeanId = "namespaceContextBuilder";

    /// <summary>
    ///     Default namespace mappings for all tests
    /// </summary>
    public Dictionary<string, string> NamespaceMappings { get; set; } = new();

    /// <summary>
    ///     Construct a basic namespace context from the received message and explicit namespace mappings.
    /// </summary>
    /// <param name="receivedMessage">the actual message received.</param>
    /// <param name="namespaces">explicit namespace mappings for this construction.</param>
    /// <returns>the constructed namespace context.</returns>
    public DefaultNamespaceContext BuildContext(IMessage receivedMessage, Dictionary<string, string>? namespaces = null)
    {
        var defaultNamespaceContext = new DefaultNamespaceContext();

        // First add default namespace definitions
        if (NamespaceMappings.Count != 0)
        {
            defaultNamespaceContext.AddNamespaces(NamespaceMappings);
        }

        var dynamicBindings = LookupNamespaces(receivedMessage.GetPayload<string>());

        if (namespaces != null && namespaces.Count != 0)
        {
            // Dynamic binding of namespace declarations in a root element of a received message
            foreach (var binding in dynamicBindings.Where(binding => !namespaces.ContainsValue(binding.Value)))
            {
                defaultNamespaceContext.AddNamespace(binding.Key, binding.Value);
            }

            // Add explicit namespace bindings
            defaultNamespaceContext.AddNamespaces(namespaces);
        }
        else
        {
            defaultNamespaceContext.AddNamespaces(dynamicBindings);
        }

        return defaultNamespaceContext;
    }

    /// <summary>
    ///     Construct a basic namespace context from the received message and explicit namespace mappings.
    ///     Returns an XmlNamespaceManager for compatibility with existing XPath operations.
    /// </summary>
    /// <param name="receivedMessage">the actual message received.</param>
    /// <param name="namespaces">explicit namespace mappings for this construction.</param>
    /// <returns>the constructed XmlNamespaceManager.</returns>
    public XmlNamespaceManager BuildXmlNamespaceManager(IMessage receivedMessage,
        Dictionary<string, string> namespaces = null)
    {
        var context = BuildContext(receivedMessage, namespaces);
        return context.ToXmlNamespaceManager();
    }

    /// <summary>
    ///     Look up namespace attribute declarations in the XML fragment and
    ///     store them in a binding map, where the key is the namespace prefix and the value
    ///     is the namespace uri.
    /// </summary>
    /// <param name="xml">XML fragment.</param>
    /// <returns>Dictionary containing namespace prefix - namespace uri pairs.</returns>
    public static Dictionary<string, string> LookupNamespaces(string xml)
    {
        var namespaces = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(xml) || !xml.Contains("xmlns"))
        {
            return namespaces;
        }

        // Regular expression to match xmlns declarations
        // Matches: xmlns="uri", xmlns:prefix="uri", xmlns:prefix='uri'
        const string xmlnsPattern = """xmlns(?::([^=\s]+))?\s*=\s*["']([^"']+)["']""";
        var matches = Regex.Matches(xml, xmlnsPattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var prefix = match.Groups[1].Success ? match.Groups[1].Value : string.Empty;
            var uri = match.Groups[2].Value;

            // Use empty string for the default namespace (like XMLConstants.DEFAULT_NS_PREFIX)
            var nsPrefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix;

            namespaces[nsPrefix] = uri;
        }

        return namespaces;
    }
}
