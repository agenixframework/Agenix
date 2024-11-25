using System.Collections.Generic;
using System.IO;
using Agenix.Core.Exceptions;
using Newtonsoft.Json;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Provides functionality for building message header data using object mapping,
///     allowing customization with a specified JSON serializer or a serializer name.
/// </summary>
public class JsonSerializerHeaderDataBuilder : DefaultHeaderDataBuilder
{
    private readonly JsonSerializer _jsonSerializer;
    private readonly string _serializerName;

    /// <summary>
    ///     Provides functionality for building message header data using object mapping,
    ///     allowing customization with a specified JSON serializer or a serializer name.
    /// </summary>
    public JsonSerializerHeaderDataBuilder(object model) : base(model)
    {
        _serializerName = null;
        _jsonSerializer = null;
    }

    /// <summary>
    ///     Provides functionality for building message header data using object mapping,
    ///     allowing customization with a specified JSON serializer or a serializer name.
    /// </summary>
    public JsonSerializerHeaderDataBuilder(object model, string serializerName) : base(model)
    {
        _serializerName = serializerName;
        _jsonSerializer = null;
    }

    /// <summary>
    ///     Provides functionality for building message header data using object mapping,
    ///     allowing customization with a specified JSON serializer or a serializerName name.
    /// </summary>
    public JsonSerializerHeaderDataBuilder(object model, JsonSerializer jsonSerializer) : base(model)
    {
        _jsonSerializer = jsonSerializer;
        _serializerName = null;
    }

    /// <summary>
    ///     Builds the header data using the specified context and returns the constructed header object.
    ///     Determines the appropriate JSON serializer based on the provided context and internal state,
    ///     and uses it to serialize the header data.
    /// </summary>
    /// <param name="context">The context used for resolving references and performing operations.</param>
    /// <returns>Returns the constructed header data object.</returns>
    /// <exception cref="CoreSystemException">
    ///     Thrown when a JSON serializer cannot be resolved or when multiple serializers are found in the reference resolver.
    /// </exception>
    public new object BuildHeaderData(TestContext context)
    {
        if (GetHeaderData() == null || GetHeaderData() is string) return base.BuildHeaderData(context);

        if (_jsonSerializer != null) return BuildHeaderData(_jsonSerializer, GetHeaderData(), context);

        if (_serializerName != null)
        {
            if (!context.GetReferenceResolver().IsResolvable(_serializerName))
                throw new CoreSystemException($"Unable to find proper json serializer for name '{_serializerName}'");
            var jsonSerializer = context.GetReferenceResolver().Resolve<JsonSerializer>(_serializerName);
            return BuildHeaderData(jsonSerializer, GetHeaderData(), context);
        }

        var jsonSerializers = context.GetReferenceResolver().ResolveAll<JsonSerializer>();
        if (jsonSerializers.Count == 1)
        {
            var jsonSerializer = new List<JsonSerializer>(jsonSerializers.Values)[0];
            return BuildHeaderData(jsonSerializer, GetHeaderData(), context);
        }

        throw new CoreSystemException(
            $"Unable to auto detect json serializer - found {jsonSerializers.Count} matching serializer instances in reference resolver");
    }

    /// <summary>
    ///     Builds the JSON payload for a given object and context, using the specified JSON serializer.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer to serialize the model.</param>
    /// <param name="model">The object model to be serialized into JSON.</param>
    /// <param name="context">The context in which dynamic content replacements are performed.</param>
    /// <returns>A JSON string with dynamic content replaced based on the given context.</returns>
    /// <exception cref="CoreSystemException">Thrown when the object graph mapping for the message payload fails.</exception>
    private object BuildHeaderData(JsonSerializer jsonSerializer, object model, TestContext context)
    {
        try
        {
            // Serialize the model to JSON string            
            string jsonString;
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonSerializer.Serialize(jsonWriter, model);
                jsonString = stringWriter.ToString();
            }

            // Replace dynamic content in the JSON string
            return context.ReplaceDynamicContentInString(jsonString);
        }
        catch (JsonException e)
        {
            throw new CoreSystemException("Failed to map object graph for message payload", e);
        }
    }
}