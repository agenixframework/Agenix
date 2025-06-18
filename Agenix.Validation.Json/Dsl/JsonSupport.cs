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

using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Message.Builder;
using Newtonsoft.Json;

namespace Agenix.Validation.Json.Dsl;

/// <summary>
///     Provides static utility methods related to JSON validation and serialization,
///     enabling DSL-style operations for working with JSON objects, validation,
///     or serialization tasks.
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
