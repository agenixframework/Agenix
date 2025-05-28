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

#region Imports

using Agenix.Api.Util;

#endregion

namespace Agenix.Api.IO;

/// <summary>
///     Configurable implementation of the
///     <see cref="IResourceLoader" /> interface.
/// </summary>
/// <remarks>
///     <p>
///         This <see cref="IResourceLoader" /> implementation
///         supports the configuration of resource access protocols and the
///         corresponding .NET types that know how to handle those protocols.
///     </p>
///     <p>
///         Basic protocol-to-resource type mappings are also defined by this class,
///         while others can be added either internally, by application contexts
///         extending this class, or externally, by the end user configuring the
///         context.
///     </p>
///     <p>
///         Only one resource type can be defined for each protocol, but multiple
///         protocols can map to the same resource type (for example, the
///         <c>"http"</c> and <c>"ftp"</c> protocols both map to the
///         <see cref="UrlResource" /> type. The protocols that are
///         mapped by default can be found in the following list.
///     </p>
///     <p>
///         <list type="bullet">
///             <item>
///                 <description>assembly</description>
///             </item>
///             <item>
///                 <description>config</description>
///             </item>
///             <item>
///                 <description>file</description>
///             </item>
///             <item>
///                 <description>http</description>
///             </item>
///             <item>
///                 <description>https</description>
///             </item>
///         </list>
///     </p>
/// </remarks>
/// <seealso cref="IResource" />
/// <seealso cref="Agenix.Core.IO.ResourceConverter" />
/// <seealso cref="Agenix.Context.IApplicationContext" />
public class ConfigurableResourceLoader : IResourceLoader
{
    /// <summary>
    ///     The separator between the protocol name and the resource name.
    /// </summary>
    public const string ProtocolSeparator = "://";

    #region Fields

    #endregion

    /// <summary>
    ///     Creates a new instance of the
    ///     <see cref="ConfigurableResourceLoader" /> class.
    /// </summary>
    public ConfigurableResourceLoader()
    {
    }

    /// <summary>
    ///     Creates a new instance of the
    ///     <see cref="ConfigurableResourceLoader" /> class using the specified default protocol for unqualified resources.
    /// </summary>
    public ConfigurableResourceLoader(string defaultProtocol)
    {
        AssertUtils.ArgumentNotNull(defaultProtocol, "defaultProtocol");
        DefaultResourceProtocol = defaultProtocol;
    }

    /// <summary>
    ///     The default protocol to use for unqualified resources.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         The initial value is <c>"file"</c>.
    ///     </p>
    /// </remarks>
    public string DefaultResourceProtocol { get; set; } = "file";

    /// <summary>
    ///     Returns a <see cref="IResource" /> that has been
    ///     mapped to the protocol of the supplied <paramref name="resourceName" />.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>
    ///     A new <see cref="IResource" /> instance for the
    ///     supplied <paramref name="resourceName" />.
    /// </returns>
    /// <exception cref="System.UriFormatException">
    ///     If a <see cref="IResource" /> <see cref="System.Type" />
    ///     mapping does not exist for the supplied <paramref name="resourceName" />.
    /// </exception>
    /// <exception cref="System.Exception">
    ///     In the case of any errors arising from the instantiation of the
    ///     returned <see cref="IResource" /> instance.
    /// </exception>
    /// <seealso cref="ResourceHandlerRegistry.RegisterResourceHandler(string, Type)" />
    public IResource GetResource(string resourceName)
    {
        var protocol = GetProtocol(resourceName);
        if (protocol == null)
        {
            protocol = DefaultResourceProtocol;
            resourceName = protocol + ProtocolSeparator + resourceName;
        }

        var handler = ResourceHandlerRegistry.GetResourceHandler(protocol);
        if (handler == null)
            throw new UriFormatException("Resource handler for the '" + protocol + "' protocol is not defined.");

        return (IResource)handler.Invoke([resourceName]);
    }

    /// <summary>
    ///     Checks that the supplied <paramref name="resourceName" /> starts
    ///     with one of the protocol names currently mapped by this
    ///     <see cref="ConfigurableResourceLoader" /> instance.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>
    ///     <see langword="true" /> if the supplied
    ///     <paramref name="resourceName" /> starts with one of the known
    ///     protocols; <see langword="false" /> if not, or if the supplied
    ///     <paramref name="resourceName" /> is itself <see langword="null" />.
    /// </returns>
    public static bool HasProtocol(string resourceName)
    {
        var protocol = GetProtocol(resourceName);
        return protocol != null && ResourceHandlerRegistry.IsHandlerRegistered(protocol);
    }

    /// <summary>
    ///     Extracts the protocol name from the supplied
    ///     <paramref name="resourceName" />.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>
    ///     The extracted protocol name or <see langword="null" /> if the
    ///     supplied <paramref name="resourceName" /> is unqualified (or
    ///     is itself <see langword="null" />).
    /// </returns>
    internal static string GetProtocol(string resourceName)
    {
        if (resourceName == null) return null;
        var pos = resourceName.IndexOf(ProtocolSeparator, StringComparison.Ordinal);
        return pos == -1 ? null : resourceName[..pos];
    }
}
