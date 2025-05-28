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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Core.Message.Builder;
using Newtonsoft.Json;

namespace Agenix.Validation.Json.Message.Builder;

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
    ///     Determines the appropriate JSON serializer based on the provided context and internal state
    ///     and uses it to serialize the header data.
    /// </summary>
    /// <param name="context">The context used for resolving references and performing operations.</param>
    /// <returns>Returns the constructed header data object.</returns>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when a JSON serializer cannot be resolved or when multiple serializers are found in the reference resolver.
    /// </exception>
    public override string BuildHeaderData(TestContext context)
    {
        if (GetHeaderData() == null || GetHeaderData() is string) return base.BuildHeaderData(context);

        if (_jsonSerializer != null) return BuildHeaderData(_jsonSerializer, GetHeaderData(), context);

        if (_serializerName != null)
        {
            if (!context.ReferenceResolver.IsResolvable(_serializerName))
                throw new AgenixSystemException($"Unable to find proper json serializer for name '{_serializerName}'");
            var jsonSerializer = context.ReferenceResolver.Resolve<JsonSerializer>(_serializerName);
            return BuildHeaderData(jsonSerializer, GetHeaderData(), context);
        }

        var jsonSerializers = context.ReferenceResolver.ResolveAll<JsonSerializer>();
        if (jsonSerializers.Count == 1)
        {
            var jsonSerializer = new List<JsonSerializer>(jsonSerializers.Values)[0];
            return BuildHeaderData(jsonSerializer, GetHeaderData(), context);
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
    private static string BuildHeaderData(JsonSerializer jsonSerializer, object model, TestContext context)
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
