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

using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Validation;

namespace Agenix.Core.Validation;

public abstract class AbstractValidationProcessor<T> : IValidationProcessor, IGenericValidationProcessor<T>,
    IReferenceResolverAware
{
    /**
     * POCO reference resolver injected before the validation callback is called
     */
    protected IReferenceResolver ReferenceResolver;

    public abstract void Validate(T payload, IDictionary<string, object> headers, TestContext context);

    /// <summary>
    ///     Sets the reference resolver for the validation processor.
    /// </summary>
    /// <param name="referenceResolver">The reference resolver to be set.</param>
    public virtual void SetReferenceResolver(IReferenceResolver referenceResolver)
    {
        ReferenceResolver = referenceResolver;
    }

    /// <summary>
    ///     Validates the given message within the provided context.
    /// </summary>
    /// <param name="message">The message to be validated.</param>
    /// <param name="context">The test context in which the validation occurs.</param>
    public virtual void Validate(IMessage message, TestContext context)
    {
        Validate((T)message.Payload, message.GetHeaders(), context);
    }

    /// <summary>
    ///     Processes the given message by invoking the validation process.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The context in which the message is processed.</param>
    public void Process(IMessage message, TestContext context)
    {
        Validate(message, context);
    }
}
