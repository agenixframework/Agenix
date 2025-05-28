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

using System.Reflection;
using System.Security.Permissions;
using Agenix.Api.Reflection.Dynamic;
using Agenix.Api.TypeResolution;
using Agenix.Api.Util;

namespace Agenix.Api.IO;

/// <summary>
///     Registry class that allows users to register and retrieve protocol handlers.
/// </summary>
/// <remarks>
///     <para>
///         Resource handler is an implementation of <see cref="IResource" /> interface
///         that should be used to process resources with the specified protocol.
///     </para>
///     <para>
///         They are used throughout the framework to access resources from various
///         sources. For example, application context loads object definitions from the resources
///         that are processed using one of the registered resource handlers.
///     </para>
///     <para>Following resource handlers are registered by default:</para>
///     <list type="table">
///         <listheader>
///             <term>Protocol</term>
///             <term>Handler Type</term>
///             <term>Description</term>
///         </listheader>
///         <item>
///             <description>config</description>
///             <description>
///                 <see cref="ConfigSectionResource" />
///             </description>
///             <description>
///                 Resolves the resources by loading specified configuration section from the standard .NET
///                 config file.
///             </description>
///         </item>
///         <item>
///             <description>file</description>
///             <description>
///                 <see cref="FileSystemResource" />
///             </description>
///             <description>Resolves filesystem resources.</description>
///         </item>
///         <item>
///             <description>http</description>
///             <description>
///                 <see cref="UrlResource" />
///             </description>
///             <description>Resolves remote web resources.</description>
///         </item>
///         <item>
///             <description>https</description>
///             <description>
///                 <see cref="UrlResource" />
///             </description>
///             <description>Resolves remote web resources via HTTPS.</description>
///         </item>
///         <item>
///             <description>ftp</description>
///             <description>
///                 <see cref="UrlResource" />
///             </description>
///             <description>Resolves ftp resources.</description>
///         </item>
///         <item>
///             <description>assembly</description>
///             <description>
///                 <see cref="AssemblyResource" />
///             </description>
///             <description>Resolves resources that are embedded into an assembly.</description>
///         </item>
///         <item>
///             <description>web</description>
///             <description><c>Agenix.Core.IO.WebResource, Agenix.Web</c>*</description>
///             <description>Resolves resources relative to the web application's virtual directory.</description>
///         </item>
///     </list>
///     * only available in web applications.
///     <para>
///         Users can create and register their own protocol handlers by implementing <see cref="IResource" /> interface
///         and mapping custom protocol name to that implementation. See <see cref="ResourceHandlersSectionHandler" /> for
///         details
///         on how to register custom protocol handler.
///     </para>
/// </remarks>
public class ResourceHandlerRegistry
{
    /// <summary>
    ///     Name of the .Net config section that contains definitions
    ///     for custom resource handlers.
    /// </summary>
    private const string ResourcesSectionName = "agenix/resourceHandlers";

    private static readonly object SyncRoot = new();

    private static readonly IDictionary<string, IDynamicConstructor> resourceHandlers =
        new Dictionary<string, IDynamicConstructor>();

    /// <summary>
    ///     Registers standard and user-configured resource handlers.
    /// </summary>
    static ResourceHandlerRegistry()
    {
        lock (SyncRoot)
        {
            RegisterResourceHandler("config", typeof(ConfigSectionResource));
            RegisterResourceHandler("file", typeof(FileSystemResource));
            RegisterResourceHandler("http", typeof(UrlResource));
            RegisterResourceHandler("https", typeof(UrlResource));
            RegisterResourceHandler("ftp", typeof(UrlResource));
            RegisterResourceHandler("assembly", typeof(AssemblyResource));

            // register custom resource handlers
            ConfigurationUtils.GetSection(ResourcesSectionName);
        }
    }

    /// <summary>
    ///     Returns resource handler for the specified protocol name.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method returns <see cref="ConstructorInfo" /> object that should be used
    ///         to create an instance of the <see cref="IResource" />-derived type by passing
    ///         resource location as a parameter.
    ///     </para>
    /// </remarks>
    /// <param name="protocolName">Name of the protocol to get the handler for.</param>
    /// <returns>Resource handler constructor for the specified protocol name.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="protocolName" /> is <c>null</c>.</exception>
    public static IDynamicConstructor GetResourceHandler(string protocolName)
    {
        AssertUtils.ArgumentNotNull(protocolName, "protocolName");
        IDynamicConstructor constructor;
        resourceHandlers.TryGetValue(protocolName, out constructor);
        return constructor;
    }

    /// <summary>
    ///     Returns <c>true</c> if a handler is registered for the specified protocol,
    ///     <c>false</c> otherwise.
    /// </summary>
    /// <param name="protocolName">Name of the protocol.</param>
    /// <returns>
    ///     <c>true</c> if a handler is registered for the specified protocol, <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">If <paramref name="protocolName" /> is <c>null</c>.</exception>
    public static bool IsHandlerRegistered(string protocolName)
    {
        return resourceHandlers.ContainsKey(protocolName);
    }

    /// <summary>
    ///     Registers resource handler and maps it to the specified protocol name.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         If the mapping already exists, the existing mapping will be
    ///         silently overwritten with the new mapping.
    ///     </p>
    /// </remarks>
    /// <param name="protocolName">
    ///     The protocol to add (or override).
    /// </param>
    /// <param name="handlerTypeName">
    ///     The type name of the concrete implementation of the
    ///     <see cref="IResource" /> interface that will handle
    ///     the specified protocol.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="protocolName" /> is
    ///     <see langword="null" /> or contains only whitespace character(s); or
    ///     if the supplied <paramref name="handlerTypeName" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     If the supplied <paramref name="handlerTypeName" /> is not a
    ///     <see cref="Type" /> that derives from the
    ///     <see cref="IResource" /> interface; or (having passed
    ///     this first check), the supplied <paramref name="handlerTypeName" />
    ///     does not expose a constructor that takes a single
    ///     <see cref="System.String" /> parameter.
    /// </exception>
    public static void RegisterResourceHandler(string protocolName, string handlerTypeName)
    {
        AssertUtils.ArgumentHasText(protocolName, "protocolName");
        AssertUtils.ArgumentHasText(handlerTypeName, "handlerTypeName");

        var handlerType = TypeResolutionUtils.ResolveType(handlerTypeName);
        RegisterResourceHandler(protocolName, handlerType);
    }

    /// <summary>
    ///     Registers resource handler and maps it to the specified protocol name.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         If the mapping already exists, the existing mapping will be
    ///         silently overwritten with the new mapping.
    ///     </p>
    /// </remarks>
    /// <param name="protocolName">
    ///     The protocol to add (or override).
    /// </param>
    /// <param name="handlerType">
    ///     The concrete implementation of the
    ///     <see cref="IResource" /> interface that will handle
    ///     the specified protocol.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="protocolName" /> is
    ///     <see langword="null" /> or contains only whitespace character(s); or
    ///     if the supplied <paramref name="handlerType" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     If the supplied <paramref name="handlerType" /> is not a
    ///     <see cref="Type" /> that derives from the
    ///     <see cref="IResource" /> interface; or (having passed
    ///     this first check), the supplied <paramref name="handlerType" />
    ///     does not expose a constructor that takes a single
    ///     <see cref="System.String" /> parameter.
    /// </exception>
    public static void RegisterResourceHandler(string protocolName, Type handlerType)
    {
        #region Sanity Checks

        AssertUtils.ArgumentHasText(protocolName, "protocolName");
        AssertUtils.ArgumentNotNull(handlerType, "handlerType");
        if (!typeof(IResource).IsAssignableFrom(handlerType))
            throw new ArgumentException(
                string.Format("[{0}] does not implement [{1}] interface (it must).", handlerType.FullName,
                    typeof(IResource).FullName));

        #endregion

        lock (SyncRoot)
        {
            var callback = () =>
            {
                // register generic uri parser for this scheme
                if (!UriParser.IsKnownScheme(protocolName))
                    UriParser.Register(new TolerantUriParser(), protocolName, 0);
            };
#if NETSTANDARD
                callback();
#else
            SecurityCritical.ExecutePrivileged(new SecurityPermission(SecurityPermissionFlag.Infrastructure), callback);
#endif
            var ctor = GetResourceConstructor(handlerType);
            resourceHandlers[protocolName] = ctor;
        }
    }

    private static IDynamicConstructor GetResourceConstructor(Type handlerType)
    {
        var ctor = handlerType.GetConstructor([typeof(string)]);
        if (ctor == null)
            throw new ArgumentException(
                $"[{handlerType.FullName}] does not have a constructor that takes a single string as an argument (it must).");
        return new SafeConstructor(ctor);
    }

    /// <summary>
    ///     Allows to create any arbitrary Url format
    /// </summary>
    private class TolerantUriParser : GenericUriParser
    {
        private const GenericUriParserOptions DefaultOptions = GenericUriParserOptions.Default
                                                               | GenericUriParserOptions.GenericAuthority
                                                               | GenericUriParserOptions.AllowEmptyAuthority;

        public TolerantUriParser()
            : base(DefaultOptions)
        {
        }
    }
}
