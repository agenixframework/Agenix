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


using Agenix.Api.Context;
using Agenix.Api.Xml.Namespace;
using Agenix.Validation.Xml.Schema;

namespace Agenix.Validation.Xml;

/// <summary>
///     Helper class for XML validation operations.
///     Provides utility methods for resolving schema repositories and namespace context builders.
/// </summary>
public static class XmlValidationHelper
{
    private static readonly NamespaceContextBuilder DefaultNamespaceContextBuilder = new();

    /// <summary>
    ///     Consult reference resolver in given test context and resolve all available beans of type XsdSchemaRepository.
    /// </summary>
    /// <param name="context">The test context</param>
    /// <returns>List of XsdSchemaRepository instances</returns>
    public static List<XsdSchemaRepository> GetSchemaRepositories(TestContext context)
    {
        if (context.ReferenceResolver != null &&
            context.ReferenceResolver.IsResolvable(typeof(XsdSchemaRepository)))
        {
            return context.ReferenceResolver.ResolveAll<XsdSchemaRepository>().Values.ToList();
        }

        return [];
    }

    /// <summary>
    ///     Consult reference resolver in given test context and resolve bean of type NamespaceContextBuilder.
    /// </summary>
    /// <param name="context">The current test context</param>
    /// <returns>Resolved namespace context builder instance</returns>
    public static NamespaceContextBuilder GetNamespaceContextBuilder(TestContext context)
    {
        if (context.ReferenceResolver != null &&
            context.ReferenceResolver.IsResolvable(typeof(NamespaceContextBuilder)))
        {
            return context.ReferenceResolver.Resolve<NamespaceContextBuilder>();
        }

        return DefaultNamespaceContextBuilder;
    }
}
