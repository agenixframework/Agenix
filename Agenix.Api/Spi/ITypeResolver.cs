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

namespace Agenix.Api.Spi;

/// <summary>
///     Resolves types by searching for assembly resource mapping files in order to resolve class references at runtime.
/// </summary>
public interface ITypeResolver
{
    /**
     * Property name that holds the type information to resolve
     */
    const string DEFAULT_TYPE_PROPERTY = "type";

    /**
     * Property name to mark that multiple types will be present all types are loaded
     */
    const string TYPE_PROPERTY_WILDCARD = "*";

    /// <summary>
    ///     Resolve resource path property file with given name and load given property.
    /// </summary>
    /// <param name="resourcePath"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    string ResolveProperty(string resourcePath, string property);

    /// <summary>
    ///     Load given property from given resource path property file and create new instance of given type. The type
    ///     information
    ///     is read by the given property in the resource file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resourcePath"></param>
    /// <param name="property"></param>
    /// <param name="initargs"></param>
    /// <returns></returns>
    T Resolve<T>(string resourcePath, string property, params object[] initargs);

    /// <summary>
    ///     Load all resources in given resource path and create new instance of given type. The type information is read by
    ///     the given property in the resource file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resourcePath"></param>
    /// <param name="property"></param>
    /// <param name="keyProperty"></param>
    /// <returns></returns>
    IDictionary<string, T> ResolveAll<T>(string resourcePath, string property, string keyProperty);
}
