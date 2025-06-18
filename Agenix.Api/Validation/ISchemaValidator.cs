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
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation.Context;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Validation;

/// <summary>
///     Defines an interface for schema validation, enabling the validation of messages
///     against specific schemas and providing support for different message types and validation
///     contexts.
/// </summary>
/// <typeparam name="T">
///     The context type that implements <see cref="ISchemaValidationContext" /> and
///     provides additional data or configuration required for schema validation.
/// </typeparam>
public interface ISchemaValidator<in T> where T : ISchemaValidationContext
{
    /// <summary>
    ///     Schema validator resource lookup path
    /// </summary>
    private const string ResourcePath = "Extension/agenix/message/schemaValidator";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ISchemaValidator<T>).Name);

    /// <summary>
    ///     Provides resolution of types by using a specified resource path for
    ///     runtime class reference mapping, typically used in schema validation processes.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Resolves all available validators from the defined resource path.
    ///     Scans assemblies for schema validator metadata and creates instances
    ///     of the associated types.
    /// </summary>
    /// <returns>
    ///     A dictionary containing the instantiated schema validators,
    ///     keyed by their identifiers.
    /// </returns>
    static IDictionary<string, ISchemaValidator<T>> Lookup()
    {
        var resolvedSchemas =
            TypeResolver.ResolveAll<ISchemaValidator<T>>("", ITypeResolver.DEFAULT_TYPE_PROPERTY, "name");

        foreach (var kvp in resolvedSchemas)
        {
            resolvedSchemas[kvp.Key] = kvp.Value;
        }

        if (!Log.IsEnabled(LogLevel.Debug))
        {
            return resolvedSchemas;
        }

        {
            foreach (var kvp in resolvedSchemas)
            {
                Log.LogDebug("Found schema validator '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());
            }
        }
        return resolvedSchemas;
    }

    /// <summary>
    ///     Resolves all available schema validators from the defined resource path lookup.
    ///     Scans assemblies for validator meta-information and returns instantiated validators as a collection.
    /// </summary>
    /// <returns>A dictionary containing the registered schema validators.</returns>
    public static Optional<ISchemaValidator<T>> Lookup(string validator)
    {
        try
        {
            var instance = TypeResolver.Resolve<ISchemaValidator<T>>(validator, ITypeResolver.DEFAULT_TYPE_PROPERTY);
            return Optional<ISchemaValidator<T>>.Of(instance);
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning($"Failed to resolve validator from resource '{validator}'");
        }

        return Optional<ISchemaValidator<T>>.Empty;
    }

    /// <summary>
    ///     Performs validation of the specified message within the given test and schema validation contexts.
    /// </summary>
    /// <param name="message">The instance of <see cref="IMessage" /> to be validated.</param>
    /// <param name="context">The context of the current test execution, providing runtime variables and utilities.</param>
    /// <param name="validationContext">The schema validation context specific to the validation environment.</param>
    void Validate(IMessage message, TestContext context, T validationContext);

    /// <summary>
    ///     Determines whether the specified message type is supported by this schema validator.
    ///     This is typically used to check if a specific message type and its associated message
    ///     can be validated with the current schema validator implementation.
    /// </summary>
    /// <param name="messageType">The type of the message to validate support for.</param>
    /// <param name="message">The message instance to validate support against.</param>
    /// <returns>True if the specified message type and message are supported; otherwise, false.</returns>
    bool SupportsMessageType(string messageType, IMessage message);

    /// <summary>
    ///     Determines if the provided message can be validated, considering the schema validation
    ///     configuration and message context.
    /// </summary>
    /// <param name="message">The message to be evaluated for validation.</param>
    /// <param name="schemaValidationEnabled">A flag indicating whether schema validation is enabled.</param>
    /// <returns>True if the message can be validated; otherwise, false.</returns>
    bool CanValidate(IMessage message, bool schemaValidationEnabled);

    /// <summary>
    ///     Validates a given message against the specified schema and context.
    ///     Ensures that the message adheres to the defined validation rules
    ///     and schema requirements.
    /// </summary>
    /// <param name="message">The message to validate.</param>
    /// <param name="context">The test context providing state and configuration for the validation process.</param>
    /// <param name="schemaRepository">The identifier for the repository containing the relevant schemas.</param>
    /// <param name="schema">The specific schema to validate the message against.</param>
    void Validate(IMessage message, TestContext context, string schemaRepository, string schema);
}
