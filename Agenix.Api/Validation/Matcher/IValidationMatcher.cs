using Agenix.Api.Context;
using Agenix.Core.Spi;
using log4net;

namespace Agenix.Api.Validation.Matcher;

/// <summary>
/// Represents an interface for implementing validation matchers in the validation framework.
/// </summary>
public interface IValidationMatcher
{
    /// <summary>
    /// A static dictionary that stores a collection of validation matchers identified by their respective string keys.
    /// </summary>
    /// <remarks>
    /// This variable serves as a central repository for registering and retrieving instances of classes
    /// that provide validation logic, implementing the <see cref="IValidationMatcher"/> interface.
    /// It supports dynamic resolution and reuse of validation matchers within the system.
    /// </remarks>
    private static readonly IDictionary<string, IValidationMatcher> Validators = new Dictionary<string, IValidationMatcher>();

    /// <summary>
    /// A static instance of the logging mechanism used for capturing and managing log events within the system.
    /// </summary>
    /// <remarks>
    /// This variable is initialized using the Log4Net logging framework and is primarily used to log diagnostic
    /// and operational information to assist with debugging and monitoring of the validation processes.
    /// It is tied to the <see cref="IValidationMatcher"/> interface for context-specific logging.
    /// Typical use cases include logging validation events, warnings, errors, or informational messages.
    /// </remarks>
    private static readonly ILog Log = LogManager.GetLogger(typeof(IValidationMatcher));
    
    /// <summary>
    ///     Represents the path used to identify and locate the resource associated with the ValueMatcher implementation.
    /// </summary>
    static string ResourcePath => "Extension/agenix/validation/matcher";

    /// <summary>
    /// A static instance of the <c>ResourcePathTypeResolver</c> used for resolving resource paths
    /// and retrieving type-related information during runtime.
    /// </summary>
    /// <remarks>
    /// This resolver provides methods to dynamically locate and instantiate types or resources
    /// based on a specified resource path and configuration. It is used for tasks like
    /// loading type implementations or validating types in context-specific scenarios.
    /// </remarks>
    static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);
    
    /// <summary>
    ///     Method called on validation.
    /// </summary>
    /// <param name="fieldName">the fieldName for logging purpose</param>
    /// <param name="value">the value to be validated.</param>
    /// <param name="controlParameters">the control parameters.</param>
    /// <param name="context"></param>
    void Validate(string fieldName, string value, List<string> controlParameters, TestContext context);
    
    static IDictionary<string, IValidationMatcher> Lookup()
    {
        if (Validators.Count != 0) return Validators;
        var resolvedValidators = TypeResolver.ResolveAll<dynamic>();
        foreach (var kvp in resolvedValidators) Validators[kvp.Key] = kvp.Value;

        if (!Log.IsDebugEnabled) return Validators;
        {
            foreach (var kvp in Validators) Log.Debug($"Found validation matcher '{kvp.Key}' as {kvp.Value.GetType()}");
        }
        return Validators;
    }
}