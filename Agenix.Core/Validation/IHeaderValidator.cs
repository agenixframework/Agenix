using System;
using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Spi;
using Agenix.Core.Util;
using Agenix.Core.Validation.Context;
using log4net;

namespace Agenix.Core.Validation;

public interface IHeaderValidator
{
    /// <summary>
    ///     Logger instance used for capturing and managing logging information
    ///     within the IHeaderValidator interface and its implementations.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(IHeaderValidator));

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
    ///    Resolves all available validators from the resource path lookup.
    /// Scans assemblies for validator meta-information and instantiates those validators.
    /// </summary>
    /// <returns>A dictionary containing the registered header validators.</returns>
    static IDictionary<string, IHeaderValidator> Lookup()
    {
        if (Validators.Count != 0) return Validators;
        
        var resolvedValidators = TypeResolver.ResolveAll<dynamic>(ResourcePath, ITypeResolver.DEFAULT_TYPE_PROPERTY, "name");

        foreach (var kvp in resolvedValidators) Validators[kvp.Key] = kvp.Value;

        if (!Log.IsDebugEnabled) return Validators;
        {
            foreach (var kvp in Validators) Log.Debug($"Found header validator '{kvp.Key}' as {kvp.Value.GetType()}");
        }
        return Validators;
    }

    /// <summary>
    /// Resolves validator from resource path lookup with given validator resource name.
    /// Scans assemblies for validator meta-information with the given name and returns instance of validator.
    /// Returns optional instead of throwing an exception when no validator could be found.
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
        catch (CoreSystemException)
        {
            Log.Warn($"Failed to resolve header validator from resource '{validator}'");
        }

        return Optional<IHeaderValidator>.Empty;
    }
}