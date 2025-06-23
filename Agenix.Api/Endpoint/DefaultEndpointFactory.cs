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

using System.Collections.Concurrent;
using System.Reflection;
using Agenix.Api.Annotations;
using Agenix.Api.Config.Annotation;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Endpoint;

/// <summary>
///     Default endpoint factory implementation uses registered endpoint components in Spring application context to create
///     endpoint from given endpoint uri. If the endpoint bean name is given, factory directly creates from the application
///     context.
///     If endpoint uri is given factory tries to find proper endpoint component in application context and in default
///     endpoint component configuration. Default endpoint components are listed in property file reference where key is
///     the component name and value is the fully qualified class name of the implementing endpoint component class.
/// </summary>
public class DefaultEndpointFactory : IEndpointFactory
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultEndpointFactory));

    /// <summary>
    ///     Endpoint cache for endpoint reuse
    /// </summary>
    private readonly ConcurrentDictionary<string, IEndpoint> _endpointCache = new();

    /// <summary>
    ///     Creates an endpoint instance based on the given URI and context.
    /// </summary>
    /// <param name="uri">The URI that identifies the endpoint to be created.</param>
    /// <param name="context">An instance of TestContext, which provides context information and access to necessary services.</param>
    /// <returns>An instance of IEndpoint that matches the specified URI and configuration.</returns>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when the endpoint URI is invalid or an appropriate endpoint component cannot be found or created.
    /// </exception>
    public IEndpoint Create(string uri, TestContext context)
    {
        var endpointUri = context.ReplaceDynamicContentInString(uri);
        if (!endpointUri.Contains(':'))
        {
            return context.ReferenceResolver.Resolve<IEndpoint>(endpointUri);
        }

        var tokens = endpointUri.Split(':');
        if (tokens.Length < 2 || (tokens.Length > 0 && string.IsNullOrEmpty(tokens[1])))
        {
            throw new AgenixSystemException($"Invalid endpoint uri '{endpointUri}'");
        }

        var componentName = tokens[0];
        var components = GetEndpointComponents(context.ReferenceResolver);

        if (components == null || !components.TryGetValue(componentName, out var component))
        // Try to get component from default Agenix modules
        {
            component = IEndpointComponent.Lookup(componentName).OrElse(null);
        }

        if (component == null)
        {
            throw new AgenixSystemException($"Unable to create endpoint component with name '{componentName}'");
        }

        var parameters = component.GetParameters(endpointUri);
        if (!parameters.TryGetValue(IEndpointComponent.EndpointName, out var cachedEndpointName))
        {
            cachedEndpointName = endpointUri;
        }

        return _endpointCache.GetOrAdd(cachedEndpointName, _ =>
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Creating new endpoint for uri '{CachedEndpointName}'", cachedEndpointName);
            }

            var endpoint = component.CreateEndpoint(endpointUri, context);
            return endpoint;
        });
    }

    public IEndpoint Create(string endpointName, Attribute endpointConfig, TestContext context)
    {
        var attribute = endpointConfig.GetType().GetCustomAttribute<AgenixEndpointConfigAttribute>();
        var qualifier = attribute?.Qualifier;

        var resolvers = context.ReferenceResolver.ResolveAll<IAnnotationConfigParser>();
        var parser = Optional<IAnnotationConfigParser>
            .OfNullable(qualifier != null && resolvers != null && resolvers.TryGetValue(qualifier, out var resolver)
                ? resolver
                : null);

        if (!parser.IsPresent)
        // Try to get parser from default Agenix modules
        {
            parser = IAnnotationConfigParser<Attribute, IEndpoint>.Lookup(qualifier);
        }

        if (parser.IsPresent)
        {
            var endpoint = parser.Value.Parse(endpointConfig, context.ReferenceResolver) as IEndpoint;
            if (endpoint == null)
            {
                throw new AgenixSystemException($"Unable to create endpoint annotation parser with name '{qualifier}'");
            }

            endpoint.SetName(endpointName);
            return endpoint;
        }

        throw new AgenixSystemException($"Unable to create endpoint annotation parser with name '{qualifier}'");
    }

    /// <summary>
    ///     Creates an endpoint instance based on the given parameters.
    /// </summary>
    /// <param name="endpointName">The name to assign to the created endpoint.</param>
    /// <param name="endpointConfig">An instance of AgenixEndpointAttribute containing configuration details for the endpoint.</param>
    /// <param name="endpointType">The type of the endpoint to be created.</param>
    /// <param name="context">An instance of TestContext, which provides context information and access to necessary services.</param>
    /// <returns>An instance of IEndpoint that matches the specified type and configuration.</returns>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when an appropriate endpoint builder cannot be found for the specified
    ///     type.
    /// </exception>
    public IEndpoint Create(string endpointName, AgenixEndpointAttribute endpointConfig, Type endpointType,
        TestContext context)
    {
        var builder = context.ReferenceResolver.ResolveAll<IEndpointBuilder<IEndpoint>>()
            .Values
            .FirstOrDefault(endpointBuilder => endpointBuilder.Supports(endpointType));

        if (builder != null)
        {
            var endpoint = builder.Build(endpointConfig, context.ReferenceResolver);
            endpoint.SetName(endpointName);
            return endpoint;
        }

        // Try to get builder from default Agenix modules
        var lookup = IEndpointBuilder<IEndpoint>.Lookup()
            .Values
            .FirstOrDefault(endpointBuilder => endpointBuilder.Supports(endpointType));

        if (lookup != null)
        {
            var endpoint = lookup.Build(endpointConfig, context.ReferenceResolver);
            endpoint.SetName(endpointName);
            return endpoint;
        }

        throw new AgenixSystemException($"Unable to create endpoint builder for type '{endpointType.Name}'");
    }

    /// <summary>
    ///     Retrieves all registered endpoint components from the provided reference resolver.
    /// </summary>
    /// <param name="referenceResolver">An instance of IReferenceResolver used to resolve all endpoint components.</param>
    /// <returns>
    ///     A dictionary containing endpoint component names as keys and their corresponding IEndpointComponent instances
    ///     as values.
    /// </returns>
    private Dictionary<string, IEndpointComponent> GetEndpointComponents(IReferenceResolver referenceResolver)
    {
        return referenceResolver.ResolveAll<IEndpointComponent>();
    }
}
