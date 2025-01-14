using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Agenix.Core.Exceptions;
using log4net;

namespace Agenix.Core.Util;

public class DefaultTypeConverter(string encodingName) : ITypeConverter
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(DefaultTypeConverter));

    public static DefaultTypeConverter INSTANCE = new(CoreSettings.AgenixFileEncoding());

    public T ConvertIfNecessary<T>(object target, Type type)
    {
        if (type.IsInstanceOfType(target)) return (T)target;

        var result = ConvertBefore<T>(target, type);
        if (result.IsPresent) return result.Value;

        if (typeof(IXmlSerializable).IsAssignableFrom(type))
        {
            if (target.GetType().IsAssignableFrom(typeof(string)))
                return (T)(object)new XDocument(new XElement("root", target.ToString()));
            if (target.GetType().IsAssignableFrom(typeof(XmlNode)))
                return (T)(object)new XmlNodeReader((XmlNode)target);
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
                    if (keyValue.Length == 2) nameValueCollection.Add(keyValue[0].Trim(), keyValue[1].Trim());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to reconstruct object of type IDictionary", ex);
            }

            var map = new Dictionary<string, object>();
            foreach (var key in nameValueCollection.AllKeys) map.Add(key, nameValueCollection[key]);

            return (T)(object)map;
        }

        if (type.IsArray && type.GetElementType() == typeof(string))
        {
            var arrayString = AsNormalizedArrayString(target);
            return (T)(object)arrayString.Split(",").ToArray();
        }

        if (type.IsGenericType && typeof(List<>).IsAssignableFrom(type.GetGenericTypeDefinition()) &&
            type.GetGenericArguments()[0] == typeof(string))
        {
            var arrayString = AsNormalizedArrayString(target);
            return (T)(object)arrayString.Split(",").ToList();
        }

        if (type == typeof(byte[]))
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

        if (typeof(Stream).IsAssignableFrom(type))
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
            try
            {
                return ConvertStringToType<T>(strTarget2, type);
            }
            catch (CoreSystemException e)
            {
                _log.Warn(
                    $"WARN: Unable to convert String object to type '{type.FullName}' - try fallback strategies. Exception: {e.Message}");
            }

        if (target is IConvertible convertibleTarget)
        {
            if (type == typeof(int))
                return (T)(object)convertibleTarget.ToInt32(null);
            if (type == typeof(short))
                return (T)(object)convertibleTarget.ToInt16(null);
            if (type == typeof(byte))
                return (T)(object)convertibleTarget.ToByte(null);
            if (type == typeof(long))
                return (T)(object)convertibleTarget.ToInt64(null);
            if (type == typeof(float))
                return (T)(object)convertibleTarget.ToSingle(null);
            if (type == typeof(double)) return (T)(object)convertibleTarget.ToDouble(null);
        }

        try
        {
            return ConvertAfter<T>(target, type);
        }
        catch (Exception e)
        {
            if (type == typeof(string))
            {
                _log.Warn(
                    $"WARN: Using default toString representation because object type conversion failed with: {e.Message}");
                return (T)(object)target.ToString();
            }

            throw;
        }
    }

    public T ConvertStringToType<T>(string value, Type type)
    {
        if (type == typeof(string))
            return (T)(object)value;
        if (type == typeof(int))
            return (T)(object)int.Parse(value);
        if (type == typeof(short))
            return (T)(object)short.Parse(value);
        if (type == typeof(byte))
            return (T)(object)byte.Parse(value);
        if (type == typeof(long))
            return (T)(object)long.Parse(value);
        if (type == typeof(bool))
            return (T)(object)bool.Parse(value);
        if (type == typeof(float))
            return (T)(object)float.Parse(value);
        if (type == typeof(double))
            return (T)(object)double.Parse(value);

        throw new InvalidOperationException($"Unable to convert '{value}' to required type '{type.FullName}'");
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
            _log.WarnFormat("Using default ToString() representation for object type {0}", target.GetType());
            return (T)(object)target.ToString();
        }

        throw new InvalidOperationException(
            $"Unable to convert object '{target?.GetType().Name ?? "null"}' to target type '{type}'");
    }
}