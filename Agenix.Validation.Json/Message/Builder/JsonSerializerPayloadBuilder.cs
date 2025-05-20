using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Core.Message.Builder;
using Newtonsoft.Json;

namespace Agenix.Validation.Json.Message.Builder;

/// <summary>
///     Provides functionality for building message payloads using object mapping,
///     allowing customization with a specified JSON serializer or a serializer name.
/// </summary>
///
[MessagePayload(MessageType.JSON)]
public class JsonSerializerPayloadBuilder : DefaultPayloadBuilder
{
    private readonly JsonSerializer _jsonSerializer;
    private readonly string _serializerName;

    /// <summary>
    ///     Provides functionality for building message payloads using object mapping,
    ///     allowing customization with a specified JSON serializer or a serializer name.
    /// </summary>
    public JsonSerializerPayloadBuilder(object model) : base(model)
    {
        _serializerName = null;
        _jsonSerializer = null;
    }

    /// <summary>
    ///     Provides functionality for building message payloads using object mapping,
    ///     allowing customization with a specified JSON serializer or a serializer name.
    /// </summary>
    public JsonSerializerPayloadBuilder(object model, string serializerName) : base(model)
    {
        _serializerName = serializerName;
        _jsonSerializer = null;
    }

    /// <summary>
    ///     Provides functionality for building message payloads using object mapping,
    ///     allowing customization with a specified JSON serializer or a serializerName name.
    /// </summary>
    public JsonSerializerPayloadBuilder(object model, JsonSerializer jsonSerializer) : base(model)
    {
        _jsonSerializer = jsonSerializer;
        _serializerName = null;
    }

    public override object BuildPayload(TestContext context)
    {
        if (GetPayload() == null || GetPayload() is string) return base.BuildPayload(context);

        if (_jsonSerializer != null) return BuildPayload(_jsonSerializer, GetPayload(), context);

        if (_serializerName != null)
        {
            if (!context.ReferenceResolver.IsResolvable(_serializerName))
                throw new AgenixSystemException($"Unable to find proper json serializer for name '{_serializerName}'");
            var jsonSerializer = context.ReferenceResolver.Resolve<JsonSerializer>(_serializerName);
            return BuildPayload(jsonSerializer, GetPayload(), context);
        }

        var jsonSerializers = context.ReferenceResolver.ResolveAll<JsonSerializer>();
        if (jsonSerializers.Count == 1)
        {
            var jsonSerializer = new List<JsonSerializer>(jsonSerializers.Values)[0];
            return BuildPayload(jsonSerializer, GetPayload(), context);
        }

        throw new AgenixSystemException(
            $"Unable to auto detect json serializer - found {jsonSerializers.Count} matching serializer instances in reference resolver");
    }

    /// <summary>
    ///     Builds the JSON payload for a given object and context, using the specified JSON serializer.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer to serialize the model.</param>
    /// <param name="model">The object model to be serialized into JSON.</param>
    /// <param name="context">The context in which dynamic content replacements are performed.</param>
    /// <returns>A JSON string with dynamic content replaced based on the given context.</returns>
    /// <exception cref="AgenixSystemException">Thrown when the object graph mapping for the message payload fails.</exception>
    private object BuildPayload(JsonSerializer jsonSerializer, object model, TestContext context)
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
            throw new AgenixSystemException("Failed to map object graph for message payload", e);
        }
    }
}