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
