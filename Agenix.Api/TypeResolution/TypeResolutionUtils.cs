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

using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Agenix.Api.Util;

#endregion

namespace Agenix.Api.TypeResolution;

/// <summary>
///     Helper methods with regard to type resolution.
/// </summary>
/// <remarks>
///     <p>
///         Not intended to be used directly by applications.
///     </p>
/// </remarks>
public sealed class TypeResolutionUtils
{
    #region Fields

    private static readonly ITypeResolver internalTypeResolver
        = new CachedTypeResolver(new GenericTypeResolver());

    #endregion

    #region Constructor (s) / Destructor

    // CLOVER:OFF

    /// <summary>
    ///     Creates a new instance of the <see cref="TypeResolutionUtils" /> class.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         This is a utility class, and as such exposes no public constructors.
    ///     </p>
    /// </remarks>
    private TypeResolutionUtils()
    {
    }

    // CLOVER:ON

    #endregion

    #region Methods

    /// <summary>
    ///     Resolves the supplied type name into a <see cref="System.Type" />
    ///     instance.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         If you require special <see cref="System.Type" /> resolution, do
    ///         <b>not</b> use this method, but rather instantiate
    ///         your own <see cref="TypeResolver" />.
    ///     </p>
    /// </remarks>
    /// <param name="typeName">
    ///     The (possibly partially assembly qualified) name of a
    ///     <see cref="System.Type" />.
    /// </param>
    /// <returns>
    ///     A resolved <see cref="System.Type" /> instance.
    /// </returns>
    /// <exception cref="System.TypeLoadException">
    ///     If the type cannot be resolved.
    /// </exception>
    public static Type ResolveType(string typeName)
    {
        var type = TypeRegistry.ResolveType(typeName);
        if (type == null) type = internalTypeResolver.Resolve(typeName);
        return type;
    }

    /// <summary>
    ///     Resolves a string array of interface names to
    ///     a <see cref="System.Type" /> array.
    /// </summary>
    /// <param name="interfaceNames">
    ///     An array of valid interface names. Each name must include the full
    ///     interface and assembly name.
    /// </param>
    /// <returns>An array of interface <see cref="System.Type" />s.</returns>
    /// <exception cref="System.TypeLoadException">
    ///     If any of the interfaces can't be loaded.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     If any of the <see cref="System.Type" />s specified is not an interface.
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     If <paramref name="interfaceNames" /> (or any of its elements ) is
    ///     <see langword="null" />.
    /// </exception>
    public static IList<Type> ResolveInterfaceArray(string[] interfaceNames)
    {
        AssertUtils.ArgumentNotNull(interfaceNames, "interfaceNames");

        var interfaces = new List<Type>();
        for (var i = 0; i < interfaceNames.Length; i++)
        {
            var interfaceName = interfaceNames[i];
            AssertUtils.ArgumentNotNull(interfaceName,
                string.Format(CultureInfo.InvariantCulture, "interfaceNames[{0}]", i));
            var resolvedInterface = ResolveType(interfaceName);
            if (!resolvedInterface.IsInterface)
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture,
                        "[{0}] is a class.",
                        resolvedInterface.FullName));
            interfaces.Add(resolvedInterface);
            interfaces.AddRange(resolvedInterface.GetInterfaces());
        }

        return interfaces;
    }

    #region MethodMatch

    // TODO : Use the future Pointcut expression language instead

    private static readonly Regex methodMatchRegex = new(
        @"(?<methodName>([\w]+\.)*[\w\*]+)(?<parameters>(\((?<parameterTypes>[\w\.]+(,[\w\.]+)*)*\))?)",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    /// <summary>
    ///     Match a method against the given pattern.
    /// </summary>
    /// <param name="pattern">the pattern to match against.</param>
    /// <param name="method">the method to match.</param>
    /// <returns>
    ///     <see lang="true" /> if the method matches the given pattern; otherwise <see lang="false" />.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    ///     If the supplied <paramref name="pattern" /> is invalid.
    /// </exception>
    public static bool MethodMatch(string pattern, MethodInfo method)
    {
        var m = methodMatchRegex.Match(pattern);

        if (!m.Success)
            throw new ArgumentException(string.Format("The pattern [{0}] is not well-formed.", pattern));

        // Check method name
        var methodNamePattern = m.Groups["methodName"].Value;
        if (!PatternMatchUtils.SimpleMatch(methodNamePattern, method.Name))
            return false;

        if (m.Groups["parameters"].Value.Length > 0)
        {
            // Check parameter types
            var parameters = m.Groups["parameterTypes"].Value;
            var paramTypes =
                parameters.Length == 0
                    ? []
                    : StringUtils.DelimitedListToStringArray(parameters, ",");
            var paramInfos = method.GetParameters();

            // Verify parameter count
            if (paramTypes.Length != paramInfos.Length)
                return false;

            // Match parameter types
            return !paramInfos.Where((t, i) => t.ParameterType != ResolveType(paramTypes[i])).Any();
        }

        return true;
    }

    #endregion

    #endregion
}
