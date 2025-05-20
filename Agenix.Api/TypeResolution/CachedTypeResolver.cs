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
        if (StringUtils.IsNullOrEmpty(typeName)) throw BuildTypeLoadException(typeName);
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
            if (ex is TypeLoadException) throw;
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