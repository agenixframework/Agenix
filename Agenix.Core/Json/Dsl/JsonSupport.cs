using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Json;
using Newtonsoft.Json;

namespace Agenix.Core.Json.Dsl;

public class JsonSupport
{
    /// <summary>
    ///     Static entrance for all Json related C# DSL functionalities.
    /// </summary>
    /// <returns></returns>
    public static JsonMessageValidationContext.Builder Json()
    {
        return new JsonMessageValidationContext.Builder();
    }

    /// <summary>
    ///     Static entrance for Json mapping validation that uses json serializer to perform Json object validation.
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
    public static JsonSerializerPayloadBuilder Marshal(object payload)
    {
        return new JsonSerializerPayloadBuilder(payload);
    }


    /// <summary>
    ///     Static builder method constructing a mapping payload builder.
    /// </summary>
    /// <param name="payload">The object to be serialized into JSON format.</param>
    /// <param name="mapper">The JsonSerializer instance to be used for serialization.</param>
    /// <returns>Returns an instance of JsonSerializerPayloadBuilder for building the JSON payload.</returns>
    public static JsonSerializerPayloadBuilder Marshal(object payload, JsonSerializer mapper)
    {
        return new JsonSerializerPayloadBuilder(payload, mapper);
    }
}