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
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Endpoint;

/// <summary>
///     Endpoint component registers with bean name in Spring application context and is then responsible to create proper
///     endpoints dynamically from endpoint uri values. Creates an endpoint instance by parsing the dynamic endpoint uri
///     with
///     special properties and parameters. Creates a proper endpoint configuration instance on the fly.
/// </summary>
public interface IEndpointComponent
{
    public static string EndpointName = "endpointName";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IEndpointComponent));

    /// <summary>
    ///     Path used to locate resources associated with the endpoint component.
    /// </summary>
    private static readonly string ResourcePath = "Extension/agenix/endpoint/component";

    /// <summary>
    ///     Resolver for dynamically loading and resolving resources based on a specified resource path.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Creates an endpoint instance by parsing the given dynamic endpoint URI with specific properties and parameters.
    /// </summary>
    /// <param name="endpointUri">The URI of the endpoint to be created.</param>
    /// <param name="context">The context in which the endpoint is to be created.</param>
    /// <returns>A new instance of an IEndpoint configured according to the specified URI and context.</returns>
    IEndpoint CreateEndpoint(string endpointUri, TestContext context);

    /// <summary>
    ///     Gets the name of this endpoint component.
    /// </summary>
    /// <returns>The name of the endpoint component.</returns>
    string GetName();

    /// <summary>
    ///     Constructs endpoint parameters from the endpoint URI.
    /// </summary>
    /// <param name="endpointUri">The endpoint URI.</param>
    /// <returns>A dictionary of parameters extracted from the endpoint URI.</returns>
    IDictionary<string, string> GetParameters(string endpointUri);

    /// <summary>
    ///     Provides a lookup of available endpoint components, indexed by their respective types.
    /// </summary>
    /// <returns>
    ///     A dictionary where the key is a string representing the endpoint type and the value is an instance of
    ///     IEndpointComponent.
    /// </returns>
    public static IDictionary<string, IEndpointComponent> Lookup()
    {
        var components = TypeResolver.ResolveAll<IEndpointComponent>();

        if (!Log.IsEnabled(LogLevel.Debug)) return components;
        foreach (var kvp in components)
            Log.LogDebug("Found endpoint component '{KvpKey}' as {Name}", kvp.Key, kvp.Value.GetType().Name);

        return components;
    }

    /// <summary>
    ///     Retrieves an <see cref="IEndpointComponent" /> instance based on the specified validator string.
    ///     If the validator is recognized, a corresponding <see cref="IEndpointComponent" /> instance is instantiated and
    ///     returned.
    ///     Otherwise, an empty <see cref="Optional{T}" /> is returned.
    /// </summary>
    /// <param name="validator">The validator string used to look up the appropriate <see cref="Optional{T}" /> instance.</param>
    /// <returns>
    ///     An <see cref="Optional{T}" /> containing the <see cref="IEndpointComponent" /> instance if found, otherwise an
    ///     empty <see cref="Optional{T}" />.
    /// </returns>
    public static Optional<IEndpointComponent> Lookup(string validator)
    {
        try
        {
            return Optional<IEndpointComponent>.Of(TypeResolver.Resolve<IEndpointComponent>(validator));
        }
        catch (TypeLoadException)
        {
            Log.LogWarning("Failed to resolve annotation config parser from resource '{Validator}'", validator);
        }

        return Optional<IEndpointComponent>.Empty;
    }
}
