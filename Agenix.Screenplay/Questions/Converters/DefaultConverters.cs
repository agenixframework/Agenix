namespace Agenix.Screenplay.Questions.Converters;

/// <summary>
/// Provides a registry of default type converters.
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
    /// Gets the appropriate converter for the specified type.
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
    /// Wrapper to bridge specific converters to object converter
    /// </summary>
    private class ConverterWrapper<T>(IConverter<T> innerConverter) : IConverter<object>
    {
        public object Convert(object value)
        {
            return innerConverter.Convert(value);
        }
    }


}
