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

using Agenix.Api.Annotations;
using Agenix.Api.Context;

namespace Agenix.Api.Endpoint;

/// <summary>
///     Endpoint factory tries to get an endpoint instance by parsing an endpoint uri. Uri can have parameters that get
///     passed
///     to the endpoint configuration. If Spring application context is given searches for matching endpoint component bean
///     and delegates to a component for endpoint creation.
/// </summary>
public interface IEndpointFactory
{
    /// <summary>
    ///     Finds endpoint by parsing the given endpoint URI. The test context helps to create endpoints
    ///     by providing the reference resolver so registered beans and bean references can be set as
    ///     configuration properties.
    /// </summary>
    /// <param name="endpointUri">The endpoint URI.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The created endpoint.</returns>
    IEndpoint Create(string endpointUri, TestContext context);

    /// <summary>
    ///     Finds endpoint by parsing the given endpoint annotation. The test context helps to create endpoints
    ///     by providing the reference resolver so registered beans and bean references can be set as
    ///     configuration properties.
    /// </summary>
    /// <param name="endpointName">The endpoint name.</param>
    /// <param name="endpointConfig">The endpoint configuration as an annotation.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The created endpoint.</returns>
    IEndpoint Create(string endpointName, Attribute endpointConfig, TestContext context);

    /// <summary>
    ///     Finds endpoint by parsing the given endpoint properties. The test context helps to create endpoints
    ///     by providing the reference resolver so registered beans and bean references can be set as
    ///     configuration properties.
    /// </summary>
    /// <param name="endpointName">The endpoint name.</param>
    /// <param name="endpointConfig">The endpoint configuration as AgenixEndpoint.</param>
    /// <param name="endpointType">The type of the endpoint.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The created endpoint.</returns>
    IEndpoint Create(string endpointName, AgenixEndpointAttribute endpointConfig, Type endpointType,
        TestContext context);
}
