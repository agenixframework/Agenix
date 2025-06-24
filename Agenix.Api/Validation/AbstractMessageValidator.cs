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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;

namespace Agenix.Api.Validation;

/// <summary>
///     Abstract class that provides a template for validating messages using various validation contexts.
/// </summary>
/// <typeparam name="T">The type of validation context that this validator supports.</typeparam>
public abstract class AbstractMessageValidator<T> : IMessageValidator<T> where T : IValidationContext
{
    /// <summary>
    ///     Validates the received message against the control message using the most appropriate validation context available.
    /// </summary>
    /// <param name="receivedMessage">The message that was received and needs validation.</param>
    /// <param name="controlMessage">The message used as a reference for validation.</param>
    /// <param name="context">The test context in which the validation is performed.</param>
    /// <param name="validationContexts">
    ///     A list of available validation contexts to find the most appropriate context for
    ///     validation.
    /// </param>
    public void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        List<IValidationContext> validationContexts)
    {
        var validationContext = FindValidationContext(validationContexts);

        // check if we were able to find a proper validation context
        if (validationContext != null)
        {
            try
            {
                ValidateMessage(receivedMessage, controlMessage, context, validationContext);
                validationContext.UpdateStatus(ValidationStatus.PASSED);
            }
            catch (ValidationException e)
            {
                validationContext.UpdateStatus(ValidationStatus.FAILED);
                throw;
            }
        }
    }

    public abstract bool SupportsMessageType(string messageType, IMessage message);

    /// <summary>
    ///     Validates a message with the most appropriate validation context.
    /// </summary>
    /// <param name="receivedMessage"></param>
    /// <param name="controlMessage"></param>
    /// <param name="context"></param>
    /// <param name="validationContext"></param>
    public virtual void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        T validationContext)
    {
    }

    /// <summary>
    ///     Provides a class type of the most appropriate validation context.
    /// </summary>
    /// <returns></returns>
    protected abstract Type GetRequiredValidationContextType();

    /// <summary>
    ///     Finds the message validation context that is most appropriate for this validator implementation.
    /// </summary>
    /// <param name="validationContexts"></param>
    /// <returns></returns>
    public virtual T FindValidationContext(List<IValidationContext> validationContexts)
    {
        foreach (var validationContext in validationContexts.Where(validationContext =>
                     GetRequiredValidationContextType().IsInstanceOfType(validationContext)))
        {
            return (T)validationContext;
        }

        return default;
    }
}
