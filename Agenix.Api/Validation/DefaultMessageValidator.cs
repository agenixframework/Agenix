#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
