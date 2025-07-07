#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
                var bean = context.ReferenceResolver.Resolve<T>(value);
                if (type.IsInstanceOfType(bean))
                {
                    return bean;
                }
            }

            throw new AgenixSystemException(
                $"Unable to convert '{value}' to required type '{typeof(T).Name}' - also no bean of required type available in application context",
                e.InnerException);
        }
    }
}
