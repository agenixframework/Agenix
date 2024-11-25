using Agenix.Core.Validation.Context;

namespace Agenix.Core.Validation;

/// <summary>
///     Represents the default implementation of the MessageValidatorRegistry,
///     initializing with a set of message validators retrieved from IMessageValidator Lookup method.
/// </summary>
public class DefaultMessageValidatorRegistry : MessageValidatorRegistry
{
    /// <summary>
    ///     Represents the default implementation of the MessageValidatorRegistry,
    ///     initializing with a set of message validators retrieved from IMessageValidator Lookup method.
    /// </summary>
    public DefaultMessageValidatorRegistry()
    {
        foreach (var keyValuePair in IMessageValidator<IValidationContext>.Lookup())
            AddMessageValidator(keyValuePair.Key, keyValuePair.Value);
    }
}