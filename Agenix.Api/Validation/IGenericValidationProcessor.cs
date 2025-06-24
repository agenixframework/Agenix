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

namespace Agenix.Api.Validation;

/// <summary>
///     Delegate for executing validation logic on a specified payload.
/// </summary>
/// <typeparam name="T">The type of the message payload.</typeparam>
/// <param name="payload">The message payload to be validated.</param>
/// <param name="headers">A dictionary containing header information related to the payload.</param>
/// <param name="context">The context in which the test is executed, providing various utilities and information.</param>
public delegate void GenericValidationProcessor<in T>(T payload, IDictionary<string, object> headers,
    TestContext context);

/// <summary>
///     Provides a contract for processing generic validation mechanisms.
///     Implementing classes should override the Validate method to specify the validation logic.
/// </summary>
/// <typeparam name="T">The type of the message payload.</typeparam>
public interface IGenericValidationProcessor<in T>
{
    /// <summary>
    ///     Subclasses do override this method for validation purposes.
    /// </summary>
    /// <param name="payload">The message payload object.</param>
    /// <param name="headers">The message headers.</param>
    /// <param name="context">The current test context.</param>
    void Validate(T payload, IDictionary<string, object> headers, TestContext context);
}
