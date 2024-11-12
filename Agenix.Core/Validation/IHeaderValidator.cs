using System;
using System.Collections.Generic;
using Agenix.Core.Validation.Context;
using log4net;

namespace Agenix.Core.Validation;

public interface IHeaderValidator
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(IHeaderValidator));

    /// <summary>
    ///     Dictionary to store header validators.
    /// </summary>
    private static readonly IDictionary<string, IHeaderValidator> _validators =
        new Dictionary<string, IHeaderValidator>();

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
    ///     Retrieves the dictionary of all registered header validators.
    /// </summary>
    /// <returns>A dictionary containing the registered header validators.</returns>
    static IDictionary<string, IHeaderValidator> Lookup()
    {
        if (_validators.Count != 0) return _validators;
        var resolvedValidators = new Dictionary<string, IHeaderValidator>
            { { "default", new DefaultHeaderValidator() } };
        foreach (var kvp in resolvedValidators) _validators[kvp.Key] = kvp.Value;

        if (!_log.IsDebugEnabled) return _validators;
        {
            foreach (var kvp in _validators) _log.Debug($"Found header validator '{kvp.Key}' as {kvp.Value.GetType()}");
        }
        return _validators;
    }
}