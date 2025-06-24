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

namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
///     Provides a registry of default type converters.
/// </summary>
public class DefaultConverters
{
    private static readonly Dictionary<Type, IConverter<object>> DefaultConverterMap;

    static DefaultConverters()
    {
        DefaultConverterMap = new Dictionary<Type, IConverter<object>>
        {
            { typeof(string), new ConverterWrapper<string>(new StringConverter()) },
            { typeof(bool), new ConverterWrapper<bool>(new BooleanConverter()) },
            { typeof(DateTime), new ConverterWrapper<DateTime>(new DateTimeConverter()) },
            { typeof(float), new ConverterWrapper<float>(new FloatConverter()) },
            { typeof(double), new ConverterWrapper<double>(new DoubleConverter()) },
            { typeof(int), new ConverterWrapper<int>(new IntegerConverter()) },
            { typeof(long), new ConverterWrapper<long>(new LongConverter()) },
            { typeof(decimal), new ConverterWrapper<decimal>(new DecimalConverter()) }
        };
    }

    /// <summary>
    ///     Gets the appropriate converter for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert to</typeparam>
    /// <param name="type">The type to get a converter for</param>
    /// <returns>The converter for the specified type</returns>
    /// <exception cref="InvalidOperationException">Thrown when no converter exists for the specified type</exception>
    public static IConverter<T> ConverterFor<T>(Type type)
    {
        if (!DefaultConverterMap.TryGetValue(type, out var value))
        {
            throw new InvalidOperationException($"No converter found for {type}");
        }

        return (IConverter<T>)value;
    }

    /// <summary>
    ///     Wrapper to bridge specific converters to object converter
    /// </summary>
    private class ConverterWrapper<T>(IConverter<T> innerConverter) : IConverter<object>
    {
        public object Convert(object value)
        {
            return innerConverter.Convert(value);
        }
    }
}
