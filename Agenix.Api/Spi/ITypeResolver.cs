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