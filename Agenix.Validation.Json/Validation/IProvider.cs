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
using Agenix.Api.Validation.Context;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     Defines a provider interface for creating instances of JsonElementValidator.
/// </summary>
public interface IProvider
{
    /// <summary>
    ///     Creates and returns a new instance of JsonElementValidator based on the provided parameters.
    /// </summary>
    /// <param name="isStrict">Determines if validation should be strict (true) or lenient (false).</param>
    /// <param name="context">The test context which provides necessary data and utilities for validation.</param>
    /// <param name="validationContext">The validation context specific to JSON message validation.</param>
    /// <returns>A JsonElementValidator configured with the specified parameters.</returns>
    JsonElementValidator GetValidator(bool isStrict, TestContext context,
        IMessageValidationContext validationContext);
}
