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

namespace Agenix.Api.Xml.Namespace;

/// <summary>
///     Extension methods for DefaultNamespaceContext integration
/// </summary>
public static class DefaultNamespaceContextExtensions
{
    /// <summary>
    ///     Converts DefaultNamespaceContext to XmlNamespaceManager for XPath operations
    /// </summary>
    /// <param name="context">The namespace context to convert</param>
    /// <returns>XmlNamespaceManager with all namespace mappings</returns>
    public static XmlNamespaceManager ToXmlNamespaceManager(this DefaultNamespaceContext context)
    {
        var nameTable = new NameTable();
        var manager = new XmlNamespaceManager(nameTable);

        foreach (var ns in context.GetNamespaces())
        {
            manager.AddNamespace(ns.Key, ns.Value);
        }

        return manager;
    }

    /// <summary>
    ///     Creates DefaultNamespaceContext from XmlNamespaceManager
    /// </summary>
    /// <param name="manager">The XmlNamespaceManager to convert</param>
    /// <returns>DefaultNamespaceContext with all namespace mappings</returns>
    public static DefaultNamespaceContext ToDefaultNamespaceContext(this XmlNamespaceManager manager)
    {
        var context = new DefaultNamespaceContext();

        foreach (string prefix in manager)
        {
            var uri = manager.LookupNamespace(prefix);
            if (!string.IsNullOrEmpty(uri))
            {
                context.AddNamespace(prefix, uri);
            }
        }

        return context;
    }
}
