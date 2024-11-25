using System;
using System.Collections.Generic;
using Agenix.Core.Endpoint;
using Agenix.Core.Endpoint.Direct.Annotation;
using Agenix.Core.Exceptions;
using Agenix.Core.Spi;
using Agenix.Core.Util;
using log4net;

namespace Agenix.Core.Config.Annotation;

/// <summary>
///     Interface for parsing annotation configurations.
/// </summary>
public interface IAnnotationConfigParser
{
    object Parse(Attribute annotation, IReferenceResolver referenceResolver);
}

/// Interface for parsing specific annotation configurations.
/// @typeparam TAttribute The type of the annotation attribute.
/// @typeparam TEndpoint The type of the endpoint created from the annotation.
/// /
public interface IAnnotationConfigParser<in TAttribute, out TEndpoint> : IAnnotationConfigParser
    where TAttribute : Attribute
    where TEndpoint : IEndpoint
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog
        Log = LogManager.GetLogger(typeof(IAnnotationConfigParser<TAttribute, TEndpoint>).Name);

    /// Parses the given annotation and resolves references to create an endpoint.
    /// <param name="annotation">The annotation attribute used for parsing.</param>
    /// <param name="referenceResolver">The reference resolver to resolve references during parsing.</param>
    /// <returns>The created endpoint based on the provided annotation and resolved references.</returns>
    TEndpoint Parse(TAttribute annotation, IReferenceResolver referenceResolver);

    /// Retrieves a dictionary of annotation config parsers, mapped by their name.
    /// <returns>
    ///     A dictionary where the key is the name of the annotation config parser
    ///     and the value is the corresponding IAnnotationConfigParser instance.
    /// </returns>
    public static Dictionary<string, IAnnotationConfigParser> Lookup()
    {
        var validators = new Dictionary<string, IAnnotationConfigParser>
        {
            { "direct.async", new DirectEndpointConfigParser() },
            { "direct.sync", new DirectSyncEndpointConfigParser() }
        };

        if (!Log.IsDebugEnabled) return validators;
        foreach (var kvp in validators)
            Log.Debug($"Found annotation config parser '{kvp.Key}' as {kvp.Value.GetType().Name}");

        return validators;
    }

    /// Retrieves an annotation config parser based on the given validator string.
    /// <param name="validator">The string key representing the desired annotation config parser.</param>
    /// <returns>
    ///     An Optional containing the corresponding IAnnotationConfigParser instance if found, otherwise an empty
    ///     Optional.
    /// </returns>
    public static Optional<IAnnotationConfigParser> Lookup(string validator)
    {
        try
        {
            switch (validator)
            {
                case "direct.async":
                {
                    var instance = new DirectEndpointConfigParser();
                    return Optional<IAnnotationConfigParser>.Of(instance);
                }
                case "direct.sync":
                {
                    var instance = new DirectSyncEndpointConfigParser();
                    return Optional<IAnnotationConfigParser>.Of(instance);
                }
            }
        }
        catch (CoreSystemException)
        {
            Log.Warn($"Failed to resolve annotation config parser from resource '{validator}'");
        }

        return Optional<IAnnotationConfigParser>.Empty;
    }
}