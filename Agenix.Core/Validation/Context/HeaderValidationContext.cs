using System.Collections.Generic;

namespace Agenix.Core.Validation.Context;

/// <summary>
///     Represents a context for validating headers within a given scope.
///     It maintains a list of header validators and provides options to add validators
///     and their references. This context allows for case-insensitive header name comparisons.
/// </summary>
public class HeaderValidationContext : IValidationContext
{
    /// <summary>
    ///     Should header name validation ignore case sensitivity.
    /// </summary>
    private bool _headerNameIgnoreCase;

    /// <summary>
    ///     List of special header validator references.
    /// </summary>
    private List<string> _validatorNames = [];

    /// <summary>
    ///     List of special header validators.
    /// </summary>
    private List<IHeaderValidator> _validators = [];

    /// <summary>
    ///     Gets or sets a value indicating whether header name validation should ignore case sensitivity.
    /// </summary>
    public bool HeaderNameIgnoreCase
    {
        get => _headerNameIgnoreCase;
        set => _headerNameIgnoreCase = value;
    }

    /// <summary>
    ///     Gets the list of validators.
    /// </summary>
    public List<IHeaderValidator> Validators
    {
        get => _validators;
        set => _validators = value;
    }

    /// <summary>
    ///     Gets the list of validator names.
    /// </summary>
    public List<string> ValidatorNames
    {
        get => _validatorNames;
        set => _validatorNames = value;
    }

    /// <summary>
    ///     Adds a header validator.
    /// </summary>
    /// <param name="validator">The header validator to add.</param>
    public void AddHeaderValidator(IHeaderValidator validator)
    {
        _validators.Add(validator);
    }

    /// <summary>
    ///     Adds a header validator reference.
    /// </summary>
    /// <param name="validatorName">The name of the header validator reference to add.</param>
    public void AddHeaderValidator(string validatorName)
    {
        _validatorNames.Add(validatorName);
    }
}