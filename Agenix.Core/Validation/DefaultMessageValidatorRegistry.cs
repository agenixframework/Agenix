#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

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
        {
            AddMessageValidator(validator.Key, validator.Value);
        }
    }

    /// <summary>
    ///     Registers all schema validators by retrieving available validators through the Lookup method and
    ///     adding them to the registry using the corresponding keys and values.
    /// </summary>
    private void RegisterSchemaValidators()
    {
        foreach (var validator in ISchemaValidator<ISchemaValidationContext>.Lookup())
        {
            AddSchemeValidator(validator.Key, validator.Value);
        }
    }
}
