using System;
using System.Collections.Generic;
using Agenix.Core.Annotations;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Exceptions;
using Agenix.Core.Spi;
using Agenix.Core.Util;
using log4net;

namespace Agenix.Core.Endpoint;

public interface IEndpointBuilder<out T> where T : IEndpoint
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(IEndpointBuilder<T>).Name);

    /// <summary>
    ///     Builds the endpoint.
    /// </summary>
    /// <returns></returns>
    T Build();

    bool Supports(Type endpointType);

    /// <summary>
    ///     Retrieves a dictionary of endpoint builders, with endpoint names as keys and endpoint builders as values.
    /// </summary>
    /// <returns>A dictionary containing mappings of endpoint names to their corresponding builders.</returns>
    public static Dictionary<string, IEndpointBuilder<DirectEndpoint>> Lookup()
    {
        var validators = new Dictionary<string, IEndpointBuilder<DirectEndpoint>>
        {
            { "direct.async", new DirectEndpointBuilder() },
            { "direct.asynchronous", new DirectEndpointBuilder() },
            { "direct.sync", new DirectSyncEndpointBuilder() },
            { "direct.synchronous", new DirectSyncEndpointBuilder() }
        };

        if (!Log.IsDebugEnabled) return validators;
        foreach (var kvp in validators) Log.Debug($"Found endpoint builder '{kvp.Key}' as {kvp.Value.GetType().Name}");

        return validators;
    }

    /// <summary>
    ///     Resolves endpoint builder from resource path lookup with given resource name. Scans classpath for endpoint builder
    ///     meta information with given name and returns instance of the builder. Returns optional instead of throwing
    ///     exception when no endpoint builder could be found. Given builder name is a combination of resource file name and
    ///     type property separated by '.' character.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static Optional<IEndpointBuilder<DirectEndpoint>> Lookup(string builder)
    {
        try
        {
            switch (builder)
            {
                case "direct.async":
                {
                    var instance = new DirectEndpointBuilder();
                    return Optional<IEndpointBuilder<DirectEndpoint>>.Of(instance);
                }
                case "direct.asynchronous":
                {
                    var instance = new DirectEndpointBuilder();
                    return Optional<IEndpointBuilder<DirectEndpoint>>.Of(instance);
                }
                case "direct.sync":
                {
                    var instance = new DirectSyncEndpointBuilder();
                    return Optional<IEndpointBuilder<DirectEndpoint>>.Of(instance);
                }
                case "direct.synchronous":
                {
                    var instance = new DirectSyncEndpointBuilder();
                    return Optional<IEndpointBuilder<DirectEndpoint>>.Of(instance);
                }
            }
        }
        catch (CoreSystemException)
        {
            Log.Warn($"Failed to resolve endpoint builder from resource '{builder}'");
        }

        return Optional<IEndpointBuilder<DirectEndpoint>>.Empty;
    }

    /// <summary>
    ///     Builds an endpoint instance with properties set from the given annotation and reference resolver.
    /// </summary>
    /// <param name="endpointAnnotation">
    ///     An instance of <see cref="AgenixEndpointAttribute" /> containing the endpoint
    ///     configuration.
    /// </param>
    /// <param name="referenceResolver">
    ///     An instance of <see cref="IReferenceResolver" /> used for resolving references in the
    ///     endpoint properties.
    /// </param>
    /// <returns>
    ///     An instance of <see cref="IEndpoint" /> with properties set according to the given annotation and reference
    ///     resolver.
    /// </returns>
    public T Build(AgenixEndpointAttribute endpointAnnotation, IReferenceResolver referenceResolver)
    {
        var nameSetter = ReflectionHelper.FindMethod(GetType(), "Name", typeof(string));
        if (nameSetter != null) ReflectionHelper.InvokeMethod(nameSetter, this, endpointAnnotation.Name);

        foreach (var endpointProperty in endpointAnnotation.Properties)
        {
            var propertyMethod = ReflectionHelper.FindMethod(GetType(), endpointProperty.Name, endpointProperty.Type);
            if (propertyMethod != null)
            {
                if (endpointProperty.Type != typeof(string) && referenceResolver.IsResolvable(endpointProperty.Value))
                {
                    var resolvedValue = referenceResolver.Resolve<T>(endpointProperty.Value);
                    ReflectionHelper.InvokeMethod(propertyMethod, this, resolvedValue);
                }
                else
                {
                    var convertedValue =
                        TypeConversionUtils.ConvertStringToType<dynamic>(endpointProperty.Value, endpointProperty.Type);
                    ReflectionHelper.InvokeMethod(propertyMethod, this, convertedValue);
                }
            }
        }

        return Build();
    }
}