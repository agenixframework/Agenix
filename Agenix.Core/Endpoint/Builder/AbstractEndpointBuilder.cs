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

using Agenix.Api.Endpoint;

namespace Agenix.Core.Endpoint.Builder;

/// <summary>
///     The AbstractEndpointBuilder class provides a base implementation for endpoint builders.
///     This abstract class is designed to streamline the creation of endpoint instances
///     by providing core functionalities required by specific endpoint builders.
/// </summary>
/// <typeparam name="TB">The type of the concrete endpoint builder.</typeparam>
public abstract class AbstractEndpointBuilder<TB>
    where TB : IEndpointBuilder<IEndpoint>
{
    protected readonly TB _builder;

    /// <summary>
    ///     Default constructor using the provided builder implementation.
    /// </summary>
    /// <param name="builder">The specific endpoint builder.</param>
    public AbstractEndpointBuilder(TB builder)
    {
        _builder = builder;
    }
}
