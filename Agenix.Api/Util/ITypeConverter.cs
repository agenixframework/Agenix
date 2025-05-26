using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Util;

/// <summary>
/// Represents a service that provides type conversion operations.
/// Allows objects to be converted to the desired type and supports lookup for converters.
/// </summary>
public interface ITypeConverter
{
    const string Default = "default";
    static readonly Dictionary<string, ITypeConverter> Converters = [];
    
    /// Logger
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ITypeConverter));

    /// <summary>
    ///     Resolves all available converters from the resource path lookup. Scans classpath for converter meta-information and
    ///     instantiates those converters.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, ITypeConverter> Lookup()
    {
        if (Converters.Count == 0)
        {
            /*converters = new ResourcePathTypeResolver().resolveAll(RESOURCE_PATH).ToDictionary(entry => entry.Key,
                                           entry => entry.Value);*/
            if (Converters.Count == 0) Converters.Add(Default, DefaultTypeConverter.Instance);
            Converters.ToList()
                .ForEach(x => Log.LogDebug("Found type converter '{ObjKey}' as {Type}", x.Key, x.Value.GetType()));
        }

        return Converters;
    }

    /// <summary>
    ///     Converts a target object to the required type if necessary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    T ConvertIfNecessary<T>(object target, Type type);


    /// <summary>
    ///     Converts String value object to given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    T ConvertStringToType<T>(string value, Type type);

    /// <summary>
    ///     Lookup default type converter specified by the resource path lookup and/ or environment settings.
    ///     In case only a single type converter is loaded via resource path, lookup this converter is used regardless of any
    ///     environment settings.
    ///     If there are multiple converter implementations on the classpath, the environment settings must specify the default.
    ///     If no converter implementation is given via resource path lookup the default implementation is returned.
    /// </summary>
    /// <returns>the type converter to use by default.</returns>
    public static ITypeConverter LookupDefault()
    {
        return LookupDefault(DefaultTypeConverter.Instance);
    }

    /// <summary>
    ///     Lookup default type converter specified by the resource path lookup and/ or environment settings.
    ///     In case only a single type converter is loaded via a resource path, lookup this converter is used regardless of any
    ///     environment settings.
    ///     If there are multiple converter implementations on the classpath, the environment settings must specify the default.
    ///     If no converter implementation is given via resource path lookup the default implementation is returned.
    /// </summary>
    /// <returns>the type converter to use by default.</returns>
    public static ITypeConverter LookupDefault(ITypeConverter defaultTypeConverter)
    {
        var name = AgenixSettings.GetTypeConverter();

        var converters = Lookup();
        if (converters.Count == 1)
        {
            var converterEntry = converters.First();
            return converterEntry.Value;
        }

        if (converters.TryGetValue(name, out var value)) return value;

        if (!AgenixSettings.TypeConverterDefault.Equals(name))
            Console.WriteLine($"Missing type converter for name '{name}' - using default type converter");
        return defaultTypeConverter;
    }
}