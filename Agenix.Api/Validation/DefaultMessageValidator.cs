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

using Agenix.Api.Message;
using Agenix.Api.Validation.Context;

namespace Agenix.Api.Validation;

/// <summary>
///     The DefaultMessageValidator class provides a default implementation for validating messages.
///     It extends the AbstractMessageValidator class and operates on instances of IValidationContext.
/// </summary>
public class DefaultMessageValidator : AbstractMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Determines whether the validator supports the specified message type.
    /// </summary>
    /// <param name="messageType">The type of the message to be validated.</param>
    /// <param name="message">The message instance to be validated.</param>
    /// <returns>A boolean value indicating whether the message type is supported.</returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return true;
    }

    /// <summary>
    ///     Finds and returns the appropriate validation context from a list of validation contexts,
    ///     excluding contexts of the type HeaderValidationContext.
    /// </summary>
    /// <param name="validationContexts">The list of available validation contexts to search through.</param>
    /// <returns>The first validation context that matches the criteria, with HeaderValidationContext types excluded.</returns>
    public override IValidationContext FindValidationContext(List<IValidationContext> validationContexts)
    {
        return base.FindValidationContext(validationContexts
            .Where(it => !(it is HeaderValidationContext))
            .ToList());
    }

    /// <summary>
    ///     Provides the required type of validation context for the message validator.
    /// </summary>
    /// <returns>The type of the validation context required by the message validator.</returns>
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(IValidationContext);
    }
}
