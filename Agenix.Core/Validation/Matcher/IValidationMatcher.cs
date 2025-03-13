using System.Collections.Generic;
using Agenix.Core.Validation.Matcher.Hamcrest;
using log4net;

namespace Agenix.Core.Validation.Matcher;

public interface IValidationMatcher
{
    private static readonly IDictionary<string, IValidationMatcher> Validators = new Dictionary<string, IValidationMatcher>();
    private static readonly ILog Log = LogManager.GetLogger(typeof(IValidationMatcher));
    
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
        var resolvedValidators = new Dictionary<string, IValidationMatcher> { { "AssertThat", new HamcrestValidationMatcher() } };
        foreach (var kvp in resolvedValidators) Validators[kvp.Key] = kvp.Value;

        if (!Log.IsDebugEnabled) return Validators;
        {
            foreach (var kvp in Validators) Log.Debug($"Found validation matcher '{kvp.Key}' as {kvp.Value.GetType()}");
        }
        return Validators;
    }
}