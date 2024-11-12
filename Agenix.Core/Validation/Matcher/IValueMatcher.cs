using System;
using System.Collections.Generic;
using log4net;

namespace Agenix.Core.Validation.Matcher;

public interface IValueMatcher
{
    private static readonly IDictionary<string, IValueMatcher> _validators = new Dictionary<string, IValueMatcher>();
    private static readonly ILog _log = LogManager.GetLogger(typeof(IValueMatcher));

    /// <summary>
    ///     Value matcher verifies the match of given received and control values.
    /// </summary>
    /// <param name="received"></param>
    /// <param name="control"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    bool Validate(object received, object control, TestContext context);

    // <summary>
    /// Filter supported value types
    /// </summary>
    /// <param name="controlType"></param>
    /// <returns></returns>
    bool Supports(Type controlType);

    /// <summary>
    ///     Retrieves a dictionary of all registered value matchers.
    /// </summary>
    /// <returns>
    ///     A dictionary where the keys are the names of the value matchers and the values are the corresponding
    ///     IValueMatcher instances.
    /// </returns>
    static IDictionary<string, IValueMatcher> Lookup()
    {
        if (_validators.Count != 0) return _validators;
        var resolvedValidators = new Dictionary<string, IValueMatcher> { { "hamcrest", new HamcrestValueMatcher() } };
        foreach (var kvp in resolvedValidators) _validators[kvp.Key] = kvp.Value;

        if (!_log.IsDebugEnabled) return _validators;
        {
            foreach (var kvp in _validators) _log.Debug($"Found validator '{kvp.Key}' as {kvp.Value.GetType()}");
        }
        return _validators;
    }
}