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
