using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;

namespace Agenix.Core.Validation;

/// <summary>
///     Represents the default implementation of the MessageValidatorRegistry.
///     It initializes the registry with message validators and schema validators
///     retrieved from the underlying IMessageValidator Lookup method.
/// </summary>
public class DefaultMessageValidatorRegistry : MessageValidatorRegistry
{
    /// <summary>
    ///     Represents the default implementation of the MessageValidatorRegistry,
    ///     initializing with a set of message validators retrieved from the IMessageValidator Lookup method.
    /// </summary>
    public DefaultMessageValidatorRegistry()
    {
        RegisterMessageValidators();
        RegisterSchemaValidators();
    }

    // <summary>
    /// <summary>
    ///     Registers all available message validators by retrieving them
    ///     from the IMessageValidator Lookup method and adding them
    ///     to the MessageValidatorRegistry.
    /// </summary>
    private void RegisterMessageValidators()
    {
        foreach (var validator in IMessageValidator<IValidationContext>.Lookup())
            AddMessageValidator(validator.Key, validator.Value);
    }

    /// <summary>
    ///     Registers all schema validators by retrieving available validators through the Lookup method and
    ///     adding them to the registry using the corresponding keys and values.
    /// </summary>
    private void RegisterSchemaValidators()
    {
        foreach (var validator in ISchemaValidator<ISchemaValidationContext>.Lookup())
            AddSchemeValidator(validator.Key, validator.Value);
    }
}