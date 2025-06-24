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

namespace Agenix.Api.Message.Correlation;

/// <summary>
///     Correlation manager stores objects with a correlation key. Clients can access the same objects some time later with
///     the same correlation key. This mechanism is used in synchronous communication where request and response messages
///     are
///     stored for correlating consumer and producer components.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICorrelationManager<T>
{
    /// <summary>
    ///     Creates a new correlation key in the test context by saving it as a test variable.
    ///     Method is called when synchronous communication is initialized.
    /// </summary>
    /// <param name="correlationKeyName">The name of the correlation key variable.</param>
    /// <param name="correlationKey">The correlation key value.</param>
    /// <param name="context">The test context.</param>
    void SaveCorrelationKey(string correlationKeyName, string correlationKey, TestContext context);

    /// <summary>
    ///     Gets the correlation key for the given identifier.
    ///     Consults the test context with test variables for retrieving the stored correlation key.
    /// </summary>
    /// <param name="correlationKeyName">The name of the correlation key variable.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The correlation key.</returns>
    string GetCorrelationKey(string correlationKeyName, TestContext context);

    /// <summary>
    ///     Stores an object in the correlation storage using the given correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <param name="obj">The object to store.</param>
    void Store(string correlationKey, T obj);

    /// <summary>
    ///     Finds the stored object by its correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <param name="timeout">The timeout period in milliseconds.</param>
    /// <returns>The found object.</returns>
    T Find(string correlationKey, long timeout);

    /// <summary>
    ///     Sets the object store implementation.
    /// </summary>
    /// <param name="store">The object store.</param>
    void SetObjectStore(IObjectStore<T> store);

    /// <summary>
    ///     Gets the object store implementation.
    /// </summary>
    /// <returns>The object store.</returns>
    IObjectStore<T> GetObjectStore();
}
