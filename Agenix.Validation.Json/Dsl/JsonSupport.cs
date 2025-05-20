using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Message.Builder;
using Newtonsoft.Json;

namespace Agenix.Validation.Json.Dsl;

/// <summary>
/// Provides static utility methods related to JSON validation and serialization,
/// enabling DSL-style operations for working with JSON objects, validation,
/// or serialization tasks.
/// </summary>
public static class JsonSupport
{
    /// <summary>
    ///     Static entrance for all JSON-related C# DSL functionalities.
    /// </summary>
    /// <returns></returns>
    public static JsonMessageValidationContext.Builder Json()
    {
        return new JsonMessageValidationContext.Builder();
    }

    /// <summary>
    ///     Static entrance for JSON mapping validation that uses JSON serializer to perform JSON object validation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static JsonMappingValidationProcessor<T>.Builder<T> Validate<T>()
    {
        return JsonMappingValidationProcessor<T>.Validate();
    }

    /// <summary>
    ///     Static builder method constructing a mapping payload builder.
    /// </summary>
    /// <param name="payload">The object to be serialized into a JSON payload.</param>
    /// <returns>A new instance of <see cref="JsonSerializerPayloadBuilder" /> initialized with the provided payload.</returns>
    public static JsonSerializerPayloadBuilder Serialize(object payload)
    {
        return new JsonSerializerPayloadBuilder(payload);
    }


    /// <summary>
    ///     Static builder method constructing a mapping payload builder.
    /// </summary>
    /// <param name="payload">The object to be serialized into JSON format.</param>
    /// <param name="mapper">The JsonSerializer instance to be used for serialization.</param>
    /// <returns>Returns an instance of JsonSerializerPayloadBuilder for building the JSON payload.</returns>
    public static JsonSerializerPayloadBuilder Serialize(object payload, JsonSerializer mapper)
    {
        return new JsonSerializerPayloadBuilder(payload, mapper);
    }
}