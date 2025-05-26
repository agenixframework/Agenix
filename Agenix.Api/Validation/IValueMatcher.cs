using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Util;
using Agenix.Core.Spi;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Validation;

public interface IValueMatcher
{
    /// <summary>
    ///     A static dictionary that holds the collection of available value matchers.
    /// </summary>
    /// <remarks>
    ///     The collection maps a string identifier to an implementation of the <c>IValueMatcher</c> interface.
    ///     It is used to retrieve or register value matcher instances within the application. This dictionary
    ///     stores all the known implementations that support value validation functionality.
    /// </remarks>
    private static readonly IDictionary<string, IValueMatcher> Validators = new Dictionary<string, IValueMatcher>();

    /// <summary>
    ///     A logger instance used to log operations and diagnostics within the <c>IValueMatcher</c> interface.
    /// </summary>
    /// <remarks>
    ///     This static logger is configured via the Log4Net framework and is used to record debug and
    ///     operational messages, aiding in identifying the state and behavior of value-matcher-related processes.
    /// </remarks>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IValueMatcher));

    /// <summary>
    ///     A static instance of the <c>ResourcePathTypeResolver</c> used for resolving resource paths
    ///     and retrieving type-related information during runtime.
    /// </summary>
    /// <remarks>
    ///     This resolver provides methods to dynamically locate and instantiate types or resources
    ///     based on a specified resource path and configuration. It is used for tasks like
    ///     loading type implementations or validating types in context-specific scenarios.
    /// </remarks>
    static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Represents the path used to identify and locate the resource associated with the ValueMatcher implementation.
    /// </summary>
    static string ResourcePath => "Extension/agenix/value/matcher";

    /// <summary>
    ///     Value matcher verifies the match of given received and control values.
    /// </summary>
    /// <param name="received"></param>
    /// <param name="control"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    bool Validate(object received, object control, TestContext context);

    // <summary>
    /// <summary>
    ///     Determines whether the matcher supports the specified control type.
    /// </summary>
    /// <param name="controlType">The control type to be evaluated.</param>
    /// <returns>True if the control type is supported; otherwise, false.</returns>
    bool Supports(Type controlType);

    /// <summary>
    ///     Resolves all available validators from the resource path lookup.
    ///     Scans assemblies for validator meta-information and instantiates those validators.
    /// </summary>
    /// <returns>A dictionary containing the registered header validators.</returns>
    static IDictionary<string, IValueMatcher> Lookup()
    {
        if (Validators.Count != 0) return Validators;

        var resolvedValidators = TypeResolver.ResolveAll<IValueMatcher>();

        foreach (var kvp in resolvedValidators) Validators[kvp.Key] = kvp.Value;

        if (!Log.IsEnabled(LogLevel.Debug)) return Validators;
        {
            foreach (var kvp in Validators)
                Log.LogDebug("Found validator '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());
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
    public static Optional<IValueMatcher> Lookup(string validator)
    {
        try
        {
            var instance = TypeResolver.Resolve<IValueMatcher>(validator);
            return Optional<IValueMatcher>.Of(instance);
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning("Failed to resolve value matcher from resource '{Validator}'", validator);
        }

        return Optional<IValueMatcher>.Empty;
    }
}