using System;
using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
/// Represents a validation matcher that skips validation for a specified field.
/// This matcher does not enforce any validation logic and effectively ignores
/// the provided input data for the given field during the validation process.
/// </summary>
public class IgnoreValidationMatcher : IValidationMatcher
{
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        Console.WriteLine($"Ignoring value for field '{fieldName}'");
    }
}