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
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation.Context;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Validation;

/// <summary>
///     Represents a validator for headers in a given context. Implementations of this interface
///     provide functionality for validating headers based on specific rules or logic.
/// </summary>
public interface IHeaderValidator
{
    /// <summary>
    ///     Logger instance used for capturing and managing logging information
    ///     within the IHeaderValidator interface and its implementations.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IHeaderValidator));

    static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Dictionary to store header validators.
    /// </summary>
    private static readonly IDictionary<string, IHeaderValidator> Validators =
        new Dictionary<string, IHeaderValidator>();

    /// <summary>
    ///     Represents the path used to identify and locate the resource associated with the HeaderValidator implementation.
    /// </summary>
    static string ResourcePath => "Extension/agenix/header/validator";

    /// <summary>
    ///     Validates the provided header information against control values within the context of a test.
    /// </summary>
    /// <param name="name">The name of the header to validate.</param>
    /// <param name="received">The received value of the header for validation.</param>
    /// <param name="control">The control value to validate against.</param>
    /// <param name="context">The context of the current test.</param>
    /// <param name="validationContext">The context of header validation containing additional settings or data.</param>
    void ValidateHeader(string name, object received, object control, TestContext context,
        HeaderValidationContext validationContext);

    /// <summary>
    ///     Filters supported headers by name and value type.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    /// <param name="type">The type of the header value.</param>
    /// <returns>A boolean indicating whether the header is supported.</returns>
    bool Supports(string headerName, Type type);

    /// <summary>
    ///     Resolves all available validators from the resource path lookup.
    ///     Scans assemblies for validator meta-information and instantiates those validators.
    /// </summary>
    /// <returns>A dictionary containing the registered header validators.</returns>
    static IDictionary<string, IHeaderValidator> Lookup()
    {
        if (Validators.Count != 0) return Validators;

        var resolvedValidators =
            TypeResolver.ResolveAll<dynamic>(ResourcePath, ITypeResolver.DEFAULT_TYPE_PROPERTY, "name");

        foreach (var kvp in resolvedValidators) Validators[kvp.Key] = kvp.Value;

        if (!Log.IsEnabled(LogLevel.Debug)) return Validators;
        {
            foreach (var kvp in Validators)
                Log.LogDebug("Found header validator '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());
        }
        return Validators;
    }

    /// <summary>
    ///     Resolves validator from resource path lookup with given validator resource name.
    ///     Scans assemblies for validator meta-information with the given name and returns instance of validator.
    ///     Returns optional instead of throwing an exception when no validator could be found.
    /// </summary>
    /// <param name="validator"></param>
    /// <returns></returns>
    public static Optional<IHeaderValidator> Lookup(string validator)
    {
        try
        {
            var instance = TypeResolver.Resolve<dynamic>(validator);
            return Optional<IHeaderValidator>.Of(instance);
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning("Failed to resolve header validator from resource '{Validator}'", validator);
        }

        return Optional<IHeaderValidator>.Empty;
    }
}
