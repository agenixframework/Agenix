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

using System.Reflection;
using Agenix.Api.Util;

#endregion

namespace Agenix.Api.TypeResolution;

/// <summary>
///     Resolves a <see cref="System.Type" /> by name.
/// </summary>
public class TypeResolver : ITypeResolver
{
    /// <summary>
    ///     Resolves the supplied <paramref name="typeName" /> to a
    ///     <see cref="System.Type" /> instance.
    /// </summary>
    /// <param name="typeName">
    ///     The unresolved (possibly partially assembly qualified) name
    ///     of a <see cref="System.Type" />.
    /// </param>
    /// <returns>
    ///     A resolved <see cref="System.Type" /> instance.
    /// </returns>
    /// <exception cref="System.TypeLoadException">
    ///     If the supplied <paramref name="typeName" /> could not be resolved
    ///     to a <see cref="System.Type" />.
    /// </exception>
    public virtual Type Resolve(string typeName)
    {
        if (StringUtils.IsNullOrEmpty(typeName))
        {
            throw BuildTypeLoadException(typeName);
        }

        var typeInfo = new TypeAssemblyHolder(typeName);
        Type type;
        try
        {
            type = typeInfo.IsAssemblyQualified
                ? LoadTypeDirectlyFromAssembly(typeInfo)
                : LoadTypeByIteratingOverAllLoadedAssemblies(typeInfo);
        }
        catch (Exception ex)
        {
            if (ex is TypeLoadException)
            {
                throw;
            }

            throw BuildTypeLoadException(typeName, ex);
        }

        if (type == null)
        {
            throw BuildTypeLoadException(typeName);
        }

        return type;
    }

    /// <summary>
    ///     Uses <see cref="System.Reflection.Assembly.LoadWithPartialName(string)" />
    ///     to load an <see cref="System.Reflection.Assembly" /> and then the attendant
    ///     <see cref="System.Type" /> referred to by the <paramref name="typeInfo" />
    ///     parameter.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         <see cref="System.Reflection.Assembly.LoadWithPartialName(string)" /> is
    ///         deprecated in .NET 2.0, but is still used here (even when this class is
    ///         compiled for .NET 2.0);
    ///         <see cref="System.Reflection.Assembly.LoadWithPartialName(string)" /> will
    ///         still resolve (non-.NET Framework) local assemblies when given only the
    ///         display name of an assembly (the behaviour for .NET Framework assemblies
    ///         and strongly named assemblies is documented in the docs for the
    ///         <see cref="System.Reflection.Assembly.LoadWithPartialName(string)" /> method).
    ///     </p>
    /// </remarks>
    /// <param name="typeInfo">
    ///     The assembly and type to be loaded.
    /// </param>
    /// <returns>
    ///     A <see cref="System.Type" />, or <see lang="null" />.
    /// </returns>
    /// <exception cref="System.Exception">
    ///     <see cref="System.Reflection.Assembly.LoadWithPartialName(string)" />
    /// </exception>
    private static Type LoadTypeDirectlyFromAssembly(TypeAssemblyHolder typeInfo)
    {
        Type type = null;
#if MONO_2_0
            Assembly assembly = Assembly.Load(typeInfo.AssemblyName);
#else
        var assembly = Assembly.LoadWithPartialName(typeInfo.AssemblyName);
#endif
        if (assembly != null)
        {
            type = assembly.GetType(typeInfo.TypeName, true, true);
        }

        return type;
    }

    /// <summary>
    ///     Uses <see cref="M:System.AppDomain.CurrentDomain.GetAssemblies()" />
    ///     to load the attendant <see cref="System.Type" /> referred to by
    ///     the <paramref name="typeInfo" /> parameter.
    /// </summary>
    /// <param name="typeInfo">
    ///     The type to be loaded.
    /// </param>
    /// <returns>
    ///     A <see cref="System.Type" />, or <see lang="null" />.
    /// </returns>
    private static Type LoadTypeByIteratingOverAllLoadedAssemblies(TypeAssemblyHolder typeInfo)
    {
        Type type = null;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            type = assembly.GetType(typeInfo.TypeName, false, false);
            if (type != null)
            {
                break;
            }
        }

        return type;
    }

    /// <summary>
    ///     Creates a new <see cref="TypeLoadException" /> instance
    ///     from the given <paramref name="typeName" />
    /// </summary>
    protected static TypeLoadException BuildTypeLoadException(string typeName)
    {
        return new TypeLoadException("Could not load type from string value '" + typeName + "'.");
    }

    /// <summary>
    ///     Creates a new <see cref="TypeLoadException" /> instance
    ///     from the given <paramref name="typeName" /> with the given inner <see cref="Exception" />
    /// </summary>
    protected static TypeLoadException BuildTypeLoadException(string typeName, Exception ex)
    {
        return new TypeLoadException("Could not load type from string value '" + typeName + "'.", ex);
    }
}
