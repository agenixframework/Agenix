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

using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Util;

/// <summary>
/// Provides a default implementation of the <see cref="ITypeConverter"/> interface,
/// allowing for type conversion operations and handling common scenarios related
/// to encoding and normalization.
/// </summary>
public class DefaultTypeConverter(string encodingName) : ITypeConverter
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultTypeConverter));

    /// <summary>
    /// A static instance of the <see cref="DefaultTypeConverter"/> class,
    /// initialized with the default file encoding defined by the application settings.
    /// </summary>
    /// <remarks>
    /// This instance provides a global, shared type conversion utility for handling
    /// various type transformations within the application context. The default
    /// configuration ensures consistency across the application when converting between
    /// types or handling encoded file operations.
    /// </remarks>
    public static readonly DefaultTypeConverter Instance = new(AgenixSettings.AgenixFileEncoding());

    /// <summary>
    /// Converts the given object to the specified type if necessary.
    /// </summary>
    /// <param name="target">The object to be converted.</param>
    /// <param name="type">The target type to which the object should be converted.</param>
    /// <typeparam name="T">The generic type to which the target object will be converted.</typeparam>
    /// <returns>The converted object of the specified type.</returns>
    public T ConvertIfNecessary<T>(object target, Type type)
    {
        if (type.IsInstanceOfType(target))
        {
            return (T)target;
        }

        var result = ConvertBefore<T>(target, type);
        if (result.IsPresent)
        {
            return result.Value;
        }

        if (typeof(IXmlSerializable).IsAssignableFrom(type))
        {
            if (target.GetType().IsAssignableFrom(typeof(string)))
            {
                return (T)(object)new XDocument(new XElement("root", target.ToString()));
            }

            if (target.GetType().IsAssignableFrom(typeof(XmlNode)))
            {
                return (T)(object)new XmlNodeReader((XmlNode)target);
            }
        }

        if (type.IsGenericType && typeof(Dictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()))
        {
            var mapString = target.ToString();

            var nameValueCollection = new NameValueCollection();
            try
            {
                var adjustedString = Regex.Replace(mapString.Substring(1, mapString.Length - 2), ",\\s*", "\n");
                using StringReader stringReader = new(adjustedString);
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    var keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        nameValueCollection.Add(keyValue[0].Trim(), keyValue[1].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to reconstruct object of type IDictionary", ex);
            }

            var map = new Dictionary<string, object>();
            foreach (var key in nameValueCollection.AllKeys)
            {
                map.Add(key, nameValueCollection[key]);
            }

            return (T)(object)map;
        }

        if (type.IsArray && type.GetElementType() == typeof(string))
        {
            var arrayString = AsNormalizedArrayString(target);
            return (T)(object)arrayString.Split(",");
        }

        if (type.IsGenericType && typeof(List<>).IsAssignableFrom(type.GetGenericTypeDefinition()) &&
            type.GetGenericArguments()[0] == typeof(string))
        {
            var arrayString = AsNormalizedArrayString(target);
            return (T)(object)arrayString.Split(",").ToList();
        }

        if (type == typeof(byte[]))
        {
            switch (target)
            {
                case string strTarget:
                    try
                    {
                        return (T)(object)Encoding.GetEncoding(encodingName).GetBytes(strTarget);
                    }
                    catch (EncoderFallbackException)
                    {
                        return (T)(object)Encoding.Default.GetBytes(strTarget);
                    }

                case ArraySegment<byte> byteBuffer:
                    return (T)(object)byteBuffer.ToArray();
                case MemoryStream memoryStream:
                    return (T)(object)memoryStream.ToArray();
            }
        }

        if (typeof(Stream).IsAssignableFrom(type))
        {
            switch (target)
            {
                case Stream streamTarget:
                    return (T)(object)streamTarget;
                case byte[] byteArray:
                    return (T)(object)new MemoryStream(byteArray);
                case string strTarget1:
                    try
                    {
                        var bytes = Encoding.GetEncoding(encodingName).GetBytes(strTarget1);
                        return (T)(object)new MemoryStream(bytes);
                    }
                    catch (EncoderFallbackException)
                    {
                        var fallbackBytes = Encoding.Default.GetBytes(strTarget1);
                        return (T)(object)new MemoryStream(fallbackBytes);
                    }

                default:
                    try
                    {
                        var bytes = Encoding.GetEncoding(encodingName).GetBytes(target.ToString());
                        return (T)(object)new MemoryStream(bytes);
                    }
                    catch (EncoderFallbackException)
                    {
                        var fallbackBytes = Encoding.Default.GetBytes(target.ToString());
                        return (T)(object)new MemoryStream(fallbackBytes);
                    }
            }
        }

        if (type == typeof(string))
        {
            switch (target)
            {
                case null:
                    return (T)(object)"null";
                case byte[] bytes:
                    return (T)(object)BitConverter.ToString(bytes);
            }

            switch (target)
            {
                case byte[] bytes:
                    return (T)(object)BitConverter.ToString(bytes);
                case short[] shorts:
                    return (T)(object)("[" + string.Join(", ", shorts) + "]");
                case int[] ints:
                    return (T)(object)("[" + string.Join(", ", ints) + "]");
                case long[] longs:
                    return (T)(object)("[" + string.Join(", ", longs) + "]");
                case float[] floats:
                    return (T)(object)("[" + string.Join(", ", floats) + "]");
                case double[] doubles:
                    return (T)(object)("[" + string.Join(", ", doubles) + "]");
                case char[] chars:
                    return (T)(object)new string(chars);
                case bool[] bools:
                    return (T)(object)string.Join(", ", bools.Select(b => b.ToString()).ToArray());
                case string[] strings:
                    return (T)(object)("[" + string.Join(", ", strings) + "]");
                case object[] objects:
                    return (T)(object)("[" + string.Join(", ", objects) + "]");
                default:
                    switch (target)
                    {
                        case IList:
                            return (T)(object)("[" + string.Join(", ",
                                ((List<string>)target).Select(x => x.ToString()).ToList()) + "]");
                        case IDictionary:
                            // Convert the dictionary to a string in the format "{key1=value1, key2=value2}"
                            var elements = (from kvp in (Dictionary<object, object>)target
                                            select $"{kvp.Key}={kvp.Value}").ToList();

                            return (T)(object)("{" + string.Join(", ", elements) + "}");
                    }

                    break;
            }
        }

        if (target is string strTarget2)
        {
            try
            {
                return ConvertStringToType<T>(strTarget2, type);
            }
            catch (AgenixSystemException e)
            {
                Log.LogWarning(
                    "WARN: Unable to convert String object to type '{TypeFullName}' - try fallback strategies. Exception: {EMessage}",
                    type.FullName, e.Message);
            }
        }

        if (target is IConvertible convertibleTarget)
        {
            if (type == typeof(int))
            {
                return (T)(object)convertibleTarget.ToInt32(null);
            }

            if (type == typeof(short))
            {
                return (T)(object)convertibleTarget.ToInt16(null);
            }

            if (type == typeof(byte))
            {
                return (T)(object)convertibleTarget.ToByte(null);
            }

            if (type == typeof(long))
            {
                return (T)(object)convertibleTarget.ToInt64(null);
            }

            if (type == typeof(float))
            {
                return (T)(object)convertibleTarget.ToSingle(null);
            }

            if (type == typeof(double))
            {
                return (T)(object)convertibleTarget.ToDouble(null);
            }
        }

        try
        {
            return ConvertAfter<T>(target, type);
        }
        catch (Exception e)
        {
            if (type == typeof(string))
            {
                Log.LogWarning(
                    "WARN: Using default toString representation because object type conversion failed with: {EMessage}"
                    , e.Message);
                return (T)(object)target.ToString();
            }

            throw;
        }
    }

    public T ConvertStringToType<T>(string value, Type type)
    {
        if (type == typeof(string))
        {
            return (T)(object)value;
        }

        if (type == typeof(int))
        {
            return (T)(object)int.Parse(value);
        }

        if (type == typeof(short))
        {
            return (T)(object)short.Parse(value);
        }

        if (type == typeof(byte))
        {
            return (T)(object)byte.Parse(value);
        }

        if (type == typeof(long))
        {
            return (T)(object)long.Parse(value);
        }

        if (type == typeof(bool))
        {
            return (T)(object)bool.Parse(value);
        }

        if (type == typeof(float))
        {
            return (T)(object)float.Parse(value);
        }

        if (type == typeof(double))
        {
            return (T)(object)double.Parse(value);
        }

        throw new AgenixSystemException($"Unable to convert '{value}' to required type '{type.FullName}'");
    }

    private static string FromListToString(List<string> list)
    {
        return "[" + string.Join(", ", list) + "]";
    }

    /// <summary>
    ///     Normalized Array[] in String
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public string AsNormalizedArrayString(object target)
    {
        var stringValue = ConvertIfNecessary<string>(target, typeof(string));
        return stringValue.Replace("[", "").Replace("]", "").Replace(", ", ",");
    }

    protected Optional<T> ConvertBefore<T>(object target, Type type)
    {
        return Optional<T>.Empty;
    }

    /// <summary>
    ///     Subclasses may add additional conversion logic in this method. This is only consulted as a fallback if none of the
    ///     default conversion strategies did succeed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected T ConvertAfter<T>(object target, Type type)
    {
        if (type == typeof(string))
        {
            Log.LogWarning("Using default ToString() representation for object type {0}", target.GetType());
            return (T)(object)target.ToString();
        }

        throw new AgenixSystemException(
            $"Unable to convert object '{target?.GetType().Name ?? "null"}' to target type '{type}'");
    }
}
