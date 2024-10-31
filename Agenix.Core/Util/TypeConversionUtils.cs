using System;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Util;

public abstract class TypeConversionUtils
{
    /// <summary>
    ///     Type converter delegate used to convert target objects to required type
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
    ///     Converts target object to required type if necessary.
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
    ///     Convert value string to required type or read bean of type from application context.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="context"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="CoreSystemException"></exception>
    public static T ConvertStringToType<T>(string value, Type type, TestContext context)
    {
        try
        {
            return ConvertStringToType<T>(value, type);
        }
        catch (CoreSystemException e)
        {
            //TODO: Implement referenc resolver
            /*if ((context.GetReferenceResolver() != null) && context.GetReferenceResolver().IsResolvable(value))
            {
                object bean = context.GetReferenceResolver().resolve(value, typeof(T));
                if (typeof(T).IsAssignableFrom(bean.GetType()))
                {
                    return (T)bean;
                }
            }*/

            throw new CoreSystemException(
                $"Unable to convert '{value}' to required type '{typeof(T).Name}' - also no bean of required type available in application context",
                e.InnerException);
        }
    }
}