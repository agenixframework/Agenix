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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;

namespace Agenix.Api.Util;

/// <summary>
///     TypeConversionUtils provides utility methods for converting objects or strings to specified types.
///     This class contains functionality for handling type conversions that may be used across various components.
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
