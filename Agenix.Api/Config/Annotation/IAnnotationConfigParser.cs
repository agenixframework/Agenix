using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Core.Spi;
using log4net;

namespace Agenix.Api.Config.Annotation;

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

    /// <summary>
    /// Path to the resource used for endpoint parser lookup.
    /// </summary>
    private const string ResourcePath = "Extension/agenix/endpoint/parser";

    /// <summary>
    /// Resolver for locating and handling types based on a specified resource path.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

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
        var validators = new Dictionary<string, IAnnotationConfigParser>(
            TypeResolver.ResolveAll<IAnnotationConfigParser>("", ITypeResolver.TYPE_PROPERTY_WILDCARD)
        );
            

        if (!Log.IsDebugEnabled) return validators;
        foreach (var kvp in validators)
            Log.Debug($"Found annotation config parser '{kvp.Key}' as {kvp.Value.GetType().Name}");

        return validators;
    }

    /// Retrieves an annotation config parser based on the given validator string.
    /// <param name="parser">The string key representing the desired annotation config parser.</param>
    /// <returns>
    ///     An Optional containing the corresponding IAnnotationConfigParser instance if found, otherwise an empty
    ///     Optional.
    /// </returns>
    public static Optional<IAnnotationConfigParser> Lookup(string parser)
    {
        try
        {
            IAnnotationConfigParser instance;
            if (parser.Contains('.')) {
                var separatorIndex = parser.LastIndexOf('.');
                instance = TypeResolver.Resolve<IAnnotationConfigParser>(parser[..separatorIndex], parser[(separatorIndex + 1)..]);
            } else {
                instance = TypeResolver.Resolve<IAnnotationConfigParser>(parser);
            }
            
            return Optional<IAnnotationConfigParser>.Of(instance);
        }
        catch (AgenixSystemException)
        {
            Log.Warn($"Failed to resolve annotation config parser from resource '{parser}'");
        }

        return Optional<IAnnotationConfigParser>.Empty;
    }
}