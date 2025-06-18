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

using System.Collections;
using System.Collections.Specialized;
using Agenix.Api.Util;

#endregion

namespace Agenix.Api.TypeResolution;

/// <summary>
///     Resolves (instantiates) a <see cref="System.Type" /> by it's (possibly
///     assembly qualified) name, and caches the <see cref="System.Type" />
///     instance against the type name.
/// </summary>
public class CachedTypeResolver : ITypeResolver
{
    /// <summary>
    ///     The cache, mapping type names (<see cref="System.String" /> instances) against
    ///     <see cref="System.Type" /> instances.
    /// </summary>
    private readonly IDictionary typeCache = new HybridDictionary();

    private readonly ITypeResolver typeResolver;

    /// <summary>
    ///     Creates a new instance of the <see cref="CachedTypeResolver" /> class.
    /// </summary>
    /// <param name="typeResolver">
    ///     The <see cref="Type" /> that this instance will delegate
    ///     actual <see cref="System" /> resolution to if a <see cref="System" />
    ///     cannot be found in this instance's <see cref="System" /> cache.
    /// </param>
    /// <exception cref="System">
    ///     If the supplied <paramref name="typeResolver" /> is <see langword="null" />.
    /// </exception>
    public CachedTypeResolver(ITypeResolver typeResolver)
    {
        AssertUtils.ArgumentNotNull(typeResolver, "typeResolver");
        this.typeResolver = typeResolver;
    }

    /// <summary>
    ///     Resolves the supplied <paramref name="typeName" /> to a
    ///     <see cref="System.Type" />
    ///     instance.
    /// </summary>
    /// <param name="typeName">
    ///     The (possibly partially assembly qualified) name of a
    ///     <see cref="System.Type" />.
    /// </param>
    /// <returns>
    ///     A resolved <see cref="System.Type" /> instance.
    /// </returns>
    /// <exception cref="System.TypeLoadException">
    ///     If the supplied <paramref name="typeName" /> could not be resolved
    ///     to a <see cref="System.Type" />.
    /// </exception>
    public Type Resolve(string typeName)
    {
        if (StringUtils.IsNullOrEmpty(typeName))
        {
            throw BuildTypeLoadException(typeName);
        }

        Type type;
        try
        {
            lock (typeCache.SyncRoot)
            {
                type = typeCache[typeName] as Type;
                if (type == null)
                {
                    type = typeResolver.Resolve(typeName);
                    typeCache[typeName] = type;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is TypeLoadException)
            {
                throw;
            }

            throw BuildTypeLoadException(typeName, ex);
        }

        return type;
    }

    private static TypeLoadException BuildTypeLoadException(string typeName)
    {
        return new TypeLoadException("Could not load type from string value '" + typeName + "'.");
    }

    private static TypeLoadException BuildTypeLoadException(string typeName, Exception ex)
    {
        return new TypeLoadException("Could not load type from string value '" + typeName + "'.", ex);
    }
}
