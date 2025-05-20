using Agenix.Api.Context;
using Agenix.Api.Exceptions;

namespace Agenix.Api.Util;

/// <summary>
/// TypeConversionUtils provides utility methods for converting objects or strings to specified types.
/// This class contains functionality for handling type conversions that may be used across various components.
/// </summary>
public abstract class TypeConversionUtils
{
    /// <summary>
    ///     Type converter delegate used to convert target objects to the required type
    /// </summary>
    private static ITypeConverter _typeConverter = ITypeConverter.LookupDefault();

    /// <summary>
    ///     Prevent instantiation.
    /// </summary>
    private TypeConversionUtils()
    {
    }

    /// <summary>
    ///     Reload default type converter.
    /// </summary>
    public static void LoadDefaultConverter()
    {
        _typeConverter = ITypeConverter.LookupDefault();
    }

    /// <summary>
    ///     Converts a target object to the required type if necessary.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="type"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ConvertIfNecessary<T>(object target, Type type)
    {
        return _typeConverter.ConvertIfNecessary<T>(target, type);
    }

    /// <summary>
    ///     Convert value string to required type.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ConvertStringToType<T>(string value, Type type)
    {
        return _typeConverter.ConvertStringToType<T>(value, type);
    }

    /// <summary>
    ///     Convert value string to required type or read bean of a type from the application context.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="context"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="AgenixSystemException"></exception>
    public static T ConvertStringToType<T>(string value, Type type, TestContext context)
    {
        try
        {
            return ConvertStringToType<T>(value, type);
        }
        catch (AgenixSystemException e)
        {
            if (context.ReferenceResolver != null && context.ReferenceResolver.IsResolvable(value))
            {
                object bean = context.ReferenceResolver.Resolve<T>(value);
                if (typeof(T).IsAssignableFrom(bean.GetType())) return (T)bean;
            }

            throw new AgenixSystemException(
                $"Unable to convert '{value}' to required type '{typeof(T).Name}' - also no bean of required type available in application context",
                e.InnerException);
        }
    }
}